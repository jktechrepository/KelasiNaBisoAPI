# Correction production — vue VuePaiementsFraisParEcole

Erreur corrigée : `Unknown column 'v.NomCompletFormate' in 'SELECT'` sur  
`GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}` (et autres endpoints de la vue).

Le déploiement **code API seul ne suffit pas** : il faut recréer la vue SQL en base.

## Ordre d'exécution (phpMyAdmin ou CLI)

1. **Backup** de la base production.
2. Remplacer `USE knb_db;` par votre nom de base si différent dans les scripts.
3. Exécuter [`PRODUCTION_VUE_PAIEMENTS_DIAGNOSTIC.sql`](PRODUCTION_VUE_PAIEMENTS_DIAGNOSTIC.sql).
4. Exécuter [`PRODUCTION_VUE_PAIEMENTS_FIX.sql`](PRODUCTION_VUE_PAIEMENTS_FIX.sql) (recréation vue + entrée EF).
5. Exécuter [`PRODUCTION_VUE_PAIEMENTS_VERIFY.sql`](PRODUCTION_VUE_PAIEMENTS_VERIFY.sql).

Alternative CLI :

```bash
mysql -h HOST -P 3306 -u USER -p VOTRE_BASE < Scripts/PRODUCTION_VUE_PAIEMENTS_DIAGNOSTIC.sql
mysql -h HOST -P 3306 -u USER -p VOTRE_BASE < Scripts/PRODUCTION_VUE_PAIEMENTS_FIX.sql
mysql -h HOST -P 3306 -u USER -p VOTRE_BASE < Scripts/PRODUCTION_VUE_PAIEMENTS_VERIFY.sql
```

Script unique (sans `USE` ni migration history) : [`RECREATE_VUE_PAIEMENTS_FRAIS_PAR_ECOLE.sql`](RECREATE_VUE_PAIEMENTS_FRAIS_PAR_ECOLE.sql).

## Résultats attendus après correction

| Vérification | Attendu |
|--------------|---------|
| `NomCompletFormate` | Colonne présente sur la vue |
| `ReferenceTransaction` | Colonne présente sur la vue |
| `__EFMigrationsHistory` | `20260831153000_RecreateVuePaiementsFraisParEcole` |
| `SELECT ... LIMIT 5` | Lignes sans erreur SQL |

## Prérequis — `Eleves.IdClasse`

Le script de correction joint `Classes` via `e.IdClasse`. Si la colonne a été supprimée (`DropEleveIdClasse`), le `CREATE VIEW` échouera.

Diagnostic :

```sql
SHOW COLUMNS FROM Eleves LIKE 'IdClasse';
```

Si absent, contacter l'équipe backend avant d'exécuter le fix (variante vue via `Inscriptions` nécessaire).

## Test API

```
GET https://VOTRE-HOST/api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
Authorization: Bearer {jwt}
```

Réponse attendue : **HTTP 200** (tableau JSON).

## Option EF (si historique migrations cohérent)

```bash
dotnet ef database update 20260831153000_RecreateVuePaiementsFraisParEcole --project KelasiNaBiso.csproj
```

Ne pas utiliser si de nombreuses migrations sont `Pending` sans audit préalable de `__EFMigrationsHistory`.

## Références

- Migration : `Migrations/20260831153000_RecreateVuePaiementsFraisParEcole.cs`
- DTO : `Models/DTOs/VuePaiementsFraisParEcoleDTO.cs`
- Modèle prod similaire : [`README_PRODUCTION_DASHBOARD_FIX.md`](README_PRODUCTION_DASHBOARD_FIX.md)
