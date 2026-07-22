using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Services.Reporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBisoAPI.Services;
using Serilog;
using AspNetCoreRateLimit;
using System.Reflection;
using Amazon.S3;
using Amazon;
using FastReport.Web;

// ═══════════════════════════════════════════════════════════════════════════════════
// Assembly pour JWT (compatibilité entre JwtBearer 6.0.25 et JWT 8.3.1)
// ═══════════════════════════════════════════════════════════════════════════════════
AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
    string assemblyName = new AssemblyName(args.Name).Name;
    if (assemblyName == "System.IdentityModel.Tokens.Jwt")
    {
        // Charger la version 8.3.1 au lieu de 6.10.0.0
        var assembly = Assembly.LoadFrom(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.IdentityModel.Tokens.Jwt.dll")
        );
        return assembly;
    }
    return null;
};

// ═══════════════════════════════════════════════════════════════════════════════════
//  CONFIGURATION SERILOG (Étape 1 : Charger la configuration avant CreateBuilder)
// ═══════════════════════════════════════════════════════════════════════════════════

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information(" Démarrage de KelasiNaBisoAPI...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    //  Configurer Serilog à partir d'appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithEnvironmentName());

    Log.Information(" Serilog configuré avec succès");

// En local uniquement : port fixe. Sur IIS/LWS (dev-knb), laisser le reverse proxy gérer les URLs.
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://0.0.0.0:7102");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// ═══════════════════════════════════════════════════════════════════════════════════
//  PERFORMANCE OPTIMIZATIONS
// ═══════════════════════════════════════════════════════════════════════════════════

//  1. Response Compression (Gzip/Brotli) - Réduit la taille des réponses de 70-90%
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

//  2. In-Memory Cache - Accélère les données statiques/semi-statiques
// Note : MemoryCache est configuré plus bas pour le Rate Limiting (sans SizeLimit)

Log.Information(" Performance optimizations configurées (Compression + Cache)");

// ═══════════════════════════════════════════════════════════════════════════════════
// RATE LIMITING - Protection contre abus et attaques brute-force
// ═══════════════════════════════════════════════════════════════════════════════════

// 1. Configuration du stockage en mémoire pour Rate Limiting
builder.Services.AddMemoryCache();

// 2. Configuration du Rate Limiting par IP
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

// 3. Configuration des politiques de Rate Limiting
builder.Services.Configure<IpRateLimitPolicies>(options =>
{
    options.IpRules = new List<IpRateLimitPolicy>();
});

// 4. Enregistrement des services requis
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

Log.Information(" Rate Limiting configuré (AspNetCoreRateLimit)");

// Configuration JWT avec authentification Bearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false; // Pour le développement
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "KelasiNaBiso-SecretKey-2025-V1-Ultra-Secure-Key-For-JWT-Token-Generation")
            ),
            ValidateIssuer = false, // Pas de validation d'issuer pour simplifier
            ValidateAudience = false, // Pas de validation d'audience pour simplifier
            ValidateLifetime = true, // Valider l'expiration du token
            ClockSkew = TimeSpan.Zero, // Pas de tolérance sur l'expiration
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KelasiNaBisoAPI",
        Version = "v2",
        Description = "Kelasi Na Biso - API sécurisée avec JWT"
    });
    
    // Configuration de l'authentification JWT dans Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez le token JWT comme : Bearer {votre_token}"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<KelasiNaBisoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("KelasiConnection"),
        new MariaDbServerVersion(new Version(10, 11, 0))
    ));

// Enregistrement du service JWT
builder.Services.AddScoped<ISimpleJwtService, SimpleJwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>(); //  REFRESH TOKEN : Service de gestion des refresh tokens

// AUDIT TRAIL: Service d'audit pour tracer toutes les modifications
builder.Services.AddScoped<IAuditService, AuditService>();

// CACHE SERVICE: Service de cache in-memory pour données statiques
builder.Services.AddScoped<ICacheService, CacheService>();

// Enregistrement des repositories (suppression des duplications)
builder.Services.AddScoped<IEleveRepository, EleveService>();
builder.Services.AddScoped<IEcoleRepository, EcoleService>();
builder.Services.AddScoped<IVitrineService, VitrineService>();
builder.Services.AddScoped<IVitrineContactService, VitrineContactService>();
builder.Services.AddScoped<IClasseRepository, ClasseService>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurService>();
builder.Services.AddScoped<ITuteurRepository, TuteurService>();
builder.Services.AddScoped<IInscriptionRepository, InscriptionService>();
builder.Services.AddScoped<ExcelInscriptionService>();
builder.Services.AddScoped<ExcelInscriptionServiceV2>();
builder.Services.AddScoped<ExcelPaiementService>();
builder.Services.AddScoped<ExcelPaiementServiceV2>();
builder.Services.AddScoped<PaiementCrashedService>(); // ✅ Service pour gérer les paiements échoués
builder.Services.AddScoped<INoteRepository, NoteService>();
builder.Services.AddScoped<ICoursRepository, CoursService>();
builder.Services.AddScoped<IAffectationCoursRepository, AffectationCoursService>();
builder.Services.AddScoped<ITitulaireClasseRepository, TitulaireClasseService>(); //  Gestion titulaires Maternelle/Primaire
builder.Services.AddScoped<IMessageRepository, MessageService>();
builder.Services.AddScoped<IPresenceRepository, PresenceService>();
builder.Services.AddScoped<IPresenceReportingService, PresenceReportingService>(); //  Service de reporting présence
builder.Services.AddScoped<ISmsNotificationService, TwilioSmsService>(); //  Service SMS Twilio
builder.Services.AddScoped<IVacationRepository, VacationService>();
builder.Services.AddScoped<IAgentRepository, AgentService>();
builder.Services.AddScoped<IAnneeScolaireRepository, AnneeScolaireService>();
builder.Services.AddScoped<IFraisRepository, FraisService>();
builder.Services.AddScoped<IPaiementRepository, PaiementService>();
builder.Services.AddScoped<IRoleRepository, RoleService>();
builder.Services.AddScoped<ISectionRepository, SectionService>();
builder.Services.AddScoped<IOptionRepository, OptionService>();
builder.Services.AddScoped<IDirectionRepository, DirectionService>();
builder.Services.AddScoped<IGroupeMessageRepository, GroupeMessageService>();
builder.Services.AddScoped<IDocumentRepository, DocumentService>();
builder.Services.AddScoped<IRessourcePedagogiqueRepository, RessourcePedagogiqueService>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationService>();
// ✅ DEVOIRS À DOMICILE: Services pour la gestion des devoirs à domicile
// Configuration AWS S3
var awsAccessKeyId = builder.Configuration["AWS:S3:AccessKeyId"];
var awsSecretAccessKey = builder.Configuration["AWS:S3:SecretAccessKey"];
var awsRegion = builder.Configuration["AWS:S3:Region"] ?? "us-east-1";

if (!string.IsNullOrEmpty(awsAccessKeyId) && !string.IsNullOrEmpty(awsSecretAccessKey))
{
    // Configuration du client S3 avec credentials explicites
    var s3Config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(awsRegion)
    };
    
    builder.Services.AddSingleton<IAmazonS3>(sp =>
    {
        return new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, s3Config);
    });
    
    // Utiliser le service S3
    builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();
    Log.Information("✅ Stockage AWS S3 configuré et activé");
}
else
{
    // Fallback vers le stockage local si les credentials AWS ne sont pas configurés
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();
    Log.Warning("⚠️  Credentials AWS S3 non configurés. Utilisation du stockage local.");
}

builder.Services.AddScoped<IAntivirusService, AntivirusService>();
builder.Services.AddScoped<IDevoirADomicileRepository, DevoirADomicileService>();
//  NOTIFICATIONS AVANCÉES: Convocations, Réunions, Alertes, Communications
builder.Services.AddScoped<KelasiNaBiso.Services.Repositories.INotificationService, KelasiNaBiso.Services.NotificationService>();
builder.Services.AddScoped<KelasiNaBisoAPI.Services.Repositories.INotificationRepository, KelasiNaBiso.Services.NotificationService>();
builder.Services.AddScoped<IPresenceNotificationBuilder, PresenceNotificationBuilder>();
builder.Services.AddScoped<IPaiementNotificationBuilder, PaiementNotificationBuilder>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddSingleton<INotificationJobQueue, NotificationJobQueue>();
builder.Services.AddScoped<INotificationSender, NotificationSender>();
builder.Services.AddHostedService<NotificationJobWorker>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceService>();
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<ICommunicationCampaignService, CommunicationCampaignService>();
builder.Services.AddScoped<ICommunicationDispatchScheduler, CommunicationDispatchScheduler>();
builder.Services.AddScoped<ICommunicationDispatchWorker, CommunicationDispatchWorker>();

//  ACTIVATION FIREBASE: Initialiser Firebase Admin SDK au démarrage
Console.WriteLine("\n === INITIALISATION FIREBASE ===");
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"] ?? "firebase-credentials.json";
Console.WriteLine($"📋 Chemin configuré: {firebaseCredentialsPath}");

var fullPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseCredentialsPath);
Console.WriteLine($"📂 Chemin complet: {fullPath}");
Console.WriteLine($"📁 Répertoire actuel: {Directory.GetCurrentDirectory()}");

if (File.Exists(fullPath))
{
    Console.WriteLine($" Fichier trouvé ! Taille: {new FileInfo(fullPath).Length} octets");
    try
    {
        Console.WriteLine(" Initialisation Firebase en cours...");
        FirebaseNotificationService.InitializeFirebase(fullPath);
        Console.WriteLine(" Firebase Admin SDK initialisé avec succès");
        Console.WriteLine($" Credentials: {fullPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur lors de l'initialisation de Firebase: {ex.Message}");
        Console.WriteLine($"📚 Stack trace: {ex.StackTrace}");
        Console.WriteLine($"⚠️  Les notifications push ne fonctionneront pas.");
    }
}
else
{
    Console.WriteLine($"❌ FICHIER INTROUVABLE: {fullPath}");
    Console.WriteLine($"⚠️  Les notifications push ne fonctionneront PAS.");
    Console.WriteLine($"💡 Solution: Placer le fichier {firebaseCredentialsPath} à la racine du projet.");
}
Console.WriteLine("🔥 === FIN INITIALISATION FIREBASE ===\n");

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IDashboardHubService, DashboardHubService>(); // ✨ Service pour dashboards en temps réel
builder.Services.AddScoped<IUsernameGeneratorService, UsernameGeneratorService>(); // Service de génération de noms d'utilisateur

// MOKO Afrika — paiements Mobile Money / carte (PayIn + PayOut)
builder.Services.Configure<KelasiNaBiso.Services.MokoAfrika.MokoSettings>(
    builder.Configuration.GetSection("MokoSettings"));
builder.Services.AddHttpClient<KelasiNaBiso.Services.MokoAfrika.IMokoAfrikaGatewayClient,
    KelasiNaBiso.Services.MokoAfrika.MokoAfrikaGatewayClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IMokoFeeCalculator,
    KelasiNaBiso.Services.MokoAfrika.MokoFeeCalculator>();
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IEcolePaiementMobileService,
    KelasiNaBiso.Services.MokoAfrika.EcolePaiementMobileService>();
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IMokoWalletService,
    KelasiNaBiso.Services.MokoAfrika.MokoWalletService>();
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IMokoAfrikaService,
    KelasiNaBiso.Services.MokoAfrika.MokoAfrikaService>();
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IPaiementMokoOrchestrator,
    KelasiNaBiso.Services.MokoAfrika.PaiementMokoOrchestrator>();
builder.Services.AddScoped<KelasiNaBiso.Services.MokoAfrika.IMokoCallbackHandler,
    KelasiNaBiso.Services.MokoAfrika.MokoCallbackHandler>();
builder.Services.AddHostedService<KelasiNaBiso.Services.MokoAfrika.MokoPayoutWorker>();
Log.Information("MOKO Afrika services configurés (PayIn/PayOut/wallet/callback/worker)");

// FastReport — cartes eleves / personnel
builder.Services.AddFastReport();
builder.Services.AddHttpClient<IReportImageResolver, ReportImageResolver>();
builder.Services.AddScoped<ICarteReportService, CarteReportService>();
Log.Information("FastReport cartes scolaires configuré");

// ✨ NOUVEAU : Services RBAC avec permissions
builder.Services.AddHttpContextAccessor(); // Nécessaire pour ICurrentUserService
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IV_UtilisateurRepository, V_UtilisateurService>();
builder.Services.AddScoped<IV_EleveRepository, V_EleveService>();
builder.Services.AddScoped<IEleveParEcoleRepository, EleveParEcoleService>();
builder.Services.AddScoped<IVuePaiementsFraisParEcoleRepository, VuePaiementsFraisParEcoleService>();
builder.Services.AddScoped<IVuePointagePresenceParEcoleRepository, VuePointagePresenceParEcoleService>();
builder.Services.AddScoped<IVueRepertoireAgentsParParentRepository, VueRepertoireAgentsParParentService>();


// SignalR: Ajouter SignalR avec configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Ping toutes les 15 secondes
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // Timeout après 30 secondes
    options.HandshakeTimeout = TimeSpan.FromSeconds(15); // Timeout de handshake
});

// CORS : toutes origines (web + apps hybrides). Les apps natives n'utilisent pas CORS.
// SetIsOriginAllowed (et non AllowAnyOrigin) pour rester compatible avec AllowCredentials + JWT.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//  ACTIVATION DE LA COMPRESSION (à mettre TRÈS TÔT dans le pipeline)
app.UseResponseCompression();
Log.Information(" Response Compression activée (Brotli/Gzip)");

//  ACTIVATION DU RATE LIMITING (AVANT l'authentification)
app.UseIpRateLimiting();
Log.Information(" Rate Limiting activé - Protection contre brute-force et abus");

// Swagger disponible dans tous les environnements
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KelasiNaBiso v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "KelasiNaBiso - Documentation";
    c.EnableFilter();
    c.EnableDeepLinking();
    c.InjectStylesheet("/swagger-ui/kelasi-swagger-search.css");
    c.InjectJavascript("/swagger-ui/kelasi-swagger-search.js?v=2");
});

// Redirection HTTPS seulement en production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseFastReport();

app.UseRouting();

// CORS après UseRouting (requis pour les preflight OPTIONS avec endpoint routing)
app.UseCors("AllowFrontend");

// Activation de l'authentification et de l'autorisation JWT
app.UseAuthentication(); // DOIT être avant UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Configuration des hubs SignalR
app.MapHub<KelasiNaBisoAPI.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<KelasiNaBisoAPI.Hubs.DashboardHub>("/hubs/dashboard"); // Hub pour dashboards en temps réel
app.MapHub<KelasiNaBisoAPI.Hubs.DevoirADomicileHub>("/hubs/devoirs-adomicile"); // Hub pour devoirs à domicile en temps réel

// Apply migrations and initialize default data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<KelasiNaBisoDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // 1. Appliquer les migrations d'abord
        //  TEMPORAIREMENT DÉSACTIVÉ : Les migrations doivent être appliquées manuellement
        // logger.LogInformation("Application des migrations à la base de données...");
        // context.Database.Migrate();
        // logger.LogInformation("Migrations appliquées avec succès.");
        
        // 2. Créer les vues et procédures stockées
        logger.LogInformation("Création des vues et procédures stockées...");
        context.CreateViewUtilisateur();
        context.CreateViewEleve();
        // context.CreateInscriptionStoredProcedure(); // Temporairement désactivé pour MySQL/MariaDB
        context.CreateViewEleveParEcole();
        context.CreateViewVuePaiementsFraisParEcole();
        context.CreateViewVuePointagePresenceParEcole();
        context.CreateViewVueRepertoireAgentsParParent();
        //  OBSOLÈTE: CreateViewVueRepertoireEnseignantsParParent supprimé
        logger.LogInformation("Vues et procédures stockées créées avec succès.");

        // 3. Initialiser les données par défaut (Super-Admin, Ekelasi School, etc.)
        logger.LogInformation("Initialisation des données par défaut...");
        await context.InitializeDefaultDataAsync();
        logger.LogInformation("Initialisation des données par défaut terminée avec succès.");
        
        // 4.  NOUVEAU : Initialiser les permissions RBAC
        logger.LogInformation("Initialisation des permissions RBAC...");
        await PermissionSeeder.SeedPermissionsAsync(context);
        logger.LogInformation("Permissions RBAC initialisées avec succès.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de l'initialisation de la base de données.");
    }
}

Log.Information("✅ KelasiNaBisoAPI démarré et prêt à recevoir des requêtes");
Log.Information("📊 Environnement : {Environment}", app.Environment.EnvironmentName);
Log.Information("🔗 Swagger UI : https://localhost:7102/swagger");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ L'application s'est arrêtée de manière inattendue");
}
finally
{
    Log.Information("🛑 Arrêt de KelasiNaBisoAPI");
    Log.CloseAndFlush();
}

// Exposer Program pour les tests d'intégration
public partial class Program { }
