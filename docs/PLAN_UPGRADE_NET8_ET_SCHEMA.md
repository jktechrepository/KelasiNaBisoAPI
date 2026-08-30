# Plan d'unification du schéma et upgrade net8

Date: 2026-07-26  
Projet: KelasiNaBisoAPI

## État actuel (problème)

Trois sources de vérité coexistent :

1. Migrations EF Core (`Migrations/*.cs`) — `Database.Migrate()` est **commenté** dans `Program.cs`
2. Création de vues SQL au démarrage (`KelasiNaBisoDbContext.CreateView*`)
3. Scripts manuels (`*.sql` racine / `Migrations/*_PRODUCTION.sql` / `SCRIPTS_SQL/`)

Conséquence : drift production vs code, démarrage possible avec vues absentes (erreurs avalées).

## Objectif

Une seule voie documentée :

```text
dotnet ef database update  →  schéma + vues versionnées
```

Plus de `ExecuteSqlRaw` opportuniste au boot (sauf seed optionnel contrôlé).

## Étapes recommandées

### Phase A — Stabiliser (court terme)

1. Documenter la commande officielle d'apply migrations en Dev/Prod.
2. Remettre `context.Database.Migrate()` derrière un flag config `Database:ApplyMigrationsOnStartup` (défaut `false` en Prod, `true` en Dev).
3. Extraire chaque `CreateView*` en migration EF `migrationBuilder.Sql(...)` idempotente.
4. Archiver les scripts one-shot dans `docs/archive/sql/` (ne plus les exécuter ad hoc sans ticket).

### Phase B — Upgrade net8 (fait 2026-07-27)

- `TargetFramework` → `net8.0` (API + tests)
- Packages AspNetCore 2.3 / AutoMapper 15 retirés ; JwtBearer 8.x aligné ; `AssemblyResolve` JWT supprimé
- Pomelo EF Core MySQL 8.0.2 ; CI GitHub Actions sur `8.0.x`
- **Ops serveur :** installer ASP.NET Core 8 Hosting Bundle sur IIS/LWS avant déploiement

### Phase C — Multi-instance (après net8)

1. File de notifications bornée + dead-letter.
2. Lock distribué (ou une seule instance) pour `MokoPayoutWorker`.
3. Remplacer `Task.Run` campagnes par `IHostedService`.

## Critères de done

- [x] `Migrate()` (ou pipeline CI/CD) est la seule façon d'appliquer le schéma
- [x] Vues créées uniquement via migrations (`AddReportingViews`)
- [x] Build/test verts sur net8
- [x] Plus de packages ASP.NET Core 2.x ni AssemblyResolve JWT
