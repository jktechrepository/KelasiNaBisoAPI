using System.Security.Claims;
using System.Text;
using KelasiNaBiso.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace KelasiNaBiso.Services
{
    public interface ISimpleJwtService
    {
        string GenerateToken(Utilisateur utilisateur, int? idAgent = null, int? idTuteur = null);
        bool ValidateToken(string token);
    }

    public class SimpleJwtService : ISimpleJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public SimpleJwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey manquant dans la configuration.");
            _issuer = _configuration["Jwt:Issuer"] ?? "KelasiNaBiso";
            _audience = _configuration["Jwt:Audience"] ?? "KelasiNaBisoUsers";
            _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440");
        }

        public string GenerateToken(Utilisateur utilisateur, int? idAgent = null, int? idTuteur = null)
        {
            try
            {
                // ✅ Utilisation de JwtSecurityTokenHandler pour générer un token standard
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // ✅ Utiliser les paramètres passés explicitement, sinon utiliser les valeurs de l'utilisateur
                var agentId = idAgent ?? utilisateur.IdAgent;
                var tuteurId = idTuteur ?? utilisateur.IdTuteur;

                // 🔍 Debug: Vérifier si IdAgent est présent
                Console.WriteLine($"🔍 [SIMPLEJWT] GenerateToken - Utilisateur {utilisateur.IdUtilisateur}: IdAgent (param) = {idAgent?.ToString() ?? "NULL"}, IdAgent (user) = {utilisateur.IdAgent?.ToString() ?? "NULL"}, IdAgent (final) = {agentId?.ToString() ?? "NULL"}");

                // Créer les claims (informations utilisateur dans le token)
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, utilisateur.IdUtilisateur.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Name, utilisateur.NomUtilisateur ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Ajouter IdEcole uniquement s'il existe (ne pas créer de claim vide)
                if (utilisateur.IdEcole.HasValue && utilisateur.IdEcole.Value > 0)
                {
                    claims.Add(new Claim("idEcole", utilisateur.IdEcole.Value.ToString()));
                }

                // Ajouter le nom de l'école uniquement s'il existe
                if (!string.IsNullOrWhiteSpace(utilisateur.Ecole?.Nom))
                {
                    claims.Add(new Claim("ecole", utilisateur.Ecole.Nom));
                }

                // ✅ Ajouter IdAgent si disponible (utiliser la valeur finale)
                if (agentId.HasValue)
                {
                    claims.Add(new Claim("IdAgent", agentId.Value.ToString()));
                    Console.WriteLine($"✅ [SIMPLEJWT] IdAgent ajouté au token: {agentId.Value}");
                }
                else
                {
                    Console.WriteLine($"⚠️ [SIMPLEJWT] IdAgent est NULL pour utilisateur {utilisateur.IdUtilisateur}");
                }

                // ✅ Ajouter IdTuteur si disponible (utiliser la valeur finale)
                if (tuteurId.HasValue)
                {
                    claims.Add(new Claim("IdTuteur", tuteurId.Value.ToString()));
                    Console.WriteLine($"✅ [SIMPLEJWT] IdTuteur ajouté au token: {tuteurId.Value}");
                }

                // ✅ MULTI-RÔLES : Ajouter tous les rôles si disponibles via UserRoles
                if (utilisateur.UserRoles != null && utilisateur.UserRoles.Any(ur => ur.Statut == true))
                {
                    var activeRoles = utilisateur.UserRoles
                        .Where(ur => ur.Statut == true)
                        .Select(ur => ur.Role)
                        .Where(r => r != null)
                        .ToList();

                    // Ajouter chaque rôle comme ClaimTypes.Role (pour [Authorize(Roles = "...")])
                    foreach (var role in activeRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Nom));
                    }

                    // Rétrocompatibilité: idRole et primaryRole
                    var primaryRole = utilisateur.PrimaryRole ?? activeRoles.OrderBy(r => r.Niveau ?? 999).FirstOrDefault();
                    if (primaryRole != null)
                    {
                        claims.Add(new Claim("idRole", primaryRole.IdRole.ToString()));
                        claims.Add(new Claim("primaryRole", primaryRole.Nom));
                    }

                    // Ajouter une représentation JSON (utile pour le frontend)
                    var roleNames = activeRoles.Select(r => r.Nom).ToArray();
                    var roleIds = activeRoles.Select(r => r.IdRole.ToString()).ToArray();
                    claims.Add(new Claim("roles", System.Text.Json.JsonSerializer.Serialize(roleNames)));
                    claims.Add(new Claim("roleIds", System.Text.Json.JsonSerializer.Serialize(roleIds)));
                }
                else
                {
                    // Rétrocompatibilité: Si UserRoles n'est pas chargé, utiliser l'ancien système
                    claims.Add(new Claim("idRole", utilisateur.IdRole.ToString()));
                    if (utilisateur.Role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, utilisateur.Role.Nom));
                        claims.Add(new Claim("primaryRole", utilisateur.Role.Nom));
                    }
                }

                // Créer le token JWT
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    signingCredentials: credentials
                );

                // Retourner le token encodé
                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la génération du JWT: {ex.Message}");
                throw;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                // ✅ Utilisation de JwtSecurityTokenHandler pour valider le token
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
