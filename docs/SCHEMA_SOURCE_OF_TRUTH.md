# Source de vérité schéma base de données

**Source officielle :** migrations Entity Framework dans `Migrations/`, y compris les vues de reporting (`20260727084551_AddReportingViews`).

```bash
# Appliquer le schéma + vues
dotnet ef database update --project KelasiNaBiso.csproj
```

**Démarrage API :**

- Development : `Database:ApplyMigrationsOnStartup` défaut `true` (ou via `appsettings.template.json`)
- Production : défaut `false` — appliquer via CI/CD / `dotnet ef database update`

**À éviter :**

- Exécuter des scripts `*_PRODUCTION.sql` hors procédure de release
- Recréer les vues via `ExecuteSqlRaw` opportuniste au boot (supprimé)

Voir aussi : [PLAN_UPGRADE_NET8_ET_SCHEMA.md](PLAN_UPGRADE_NET8_ET_SCHEMA.md).
