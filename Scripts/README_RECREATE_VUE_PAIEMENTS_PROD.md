# Correction production — vue VuePaiementsFraisParEcole

Erreur corrigée historiquement : `Unknown column 'v.NomCompletFormate' in 'SELECT'` sur  
`GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}` (et autres endpoints de la vue).

**Mise à jour 2026-09-17** : la vue joint désormais la **classe via `Inscriptions`** (plus via `Eleves.IdClasse`, colonne supprimée après `DropEleveIdClasse`). L’école vient de **`Frais.IdEcole`**.

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

Scripts uniques (sans `USE` ni migration history selon le fichier) :

- [`RECREATE_VUE_PAIEMENTS_FRAIS_PAR_ECOLE.sql`](RECREATE_VUE_PAIEMENTS_FRAIS_PAR_ECOLE.sql)
- Copie apply manuel : [`../docs/sql/20260917_RecreateVuePaiementsFraisParEcole_ViaInscription.sql`](../docs/sql/20260917_RecreateVuePaiementsFraisParEcole_ViaInscription.sql)

## Résultats attendus après correction

| Vérification | Attendu |
|--------------|---------|
| `NomCompletFormate` | Colonne présente sur la vue |
| `ReferenceTransaction` | Colonne présente sur la vue |
| `SELECT COUNT(*)` / `LIMIT 5` | Sans erreur SQL (plus d’erreur 1356 liée à `Eleves.IdClasse`) |
| `__EFMigrationsHistory` | `20260831153000_RecreateVuePaiementsFraisParEcole` (si FIX avec history) |

`IdClasse` / `NomClasse` peuvent être **NULL** pour un paiement dont l’élève n’a pas d’inscription confirmée active — le paiement reste visible.

## Prérequis

- Table `Inscriptions` avec `IdEleve`, `IdClasse`, `DateInscription`, `Statut`, `StatutInscription`
- `Frais.IdEcole` présent
- **Plus besoin** de `Eleves.IdClasse`

Inscription « active » (même logique que `V_Eleve`) :

- `Statut = 1`
- `StatutInscription` Confirmé / Confirme / Confirm%
- dernière `DateInscription` par élève

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
Les migrations EF historiques de cette vue peuvent encore référencer `e.IdClasse` : **préférer le script SQL manuel** ci-dessus (comme pour `V_Eleve`).

## Références

- Script apply : `docs/sql/20260917_RecreateVuePaiementsFraisParEcole_ViaInscription.sql`
- DTO : `Models/DTOs/VuePaiementsFraisParEcoleDTO.cs`
- Vue élève (même pattern inscription) : `Scripts/PRODUCTION_RECREATE_V_ELEVE.sql`
- Modèle prod similaire : [`README_PRODUCTION_DASHBOARD_FIX.md`](README_PRODUCTION_DASHBOARD_FIX.md)
