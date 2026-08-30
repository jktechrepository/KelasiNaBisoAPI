# Correction Dashboard production (`knb_db`)

Erreur corrigée : `Unknown column 'p.MontantCollecte' in 'SELECT'` sur `GET /api/Dashboard/global`.

## Ordre d'exécution (phpMyAdmin)

1. Sélectionner la base **`knb_db`** dans le menu de gauche.
2. Onglet **SQL** → exécuter [`PRODUCTION_DASHBOARD_DIAGNOSTIC_knb_db.sql`](PRODUCTION_DASHBOARD_DIAGNOSTIC_knb_db.sql) (état avant).
3. Exécuter [`PRODUCTION_DASHBOARD_FIX_knb_db.sql`](PRODUCTION_DASHBOARD_FIX_knb_db.sql) (correction idempotente).
4. Exécuter [`PRODUCTION_DASHBOARD_VERIFY_knb_db.sql`](PRODUCTION_DASHBOARD_VERIFY_knb_db.sql) (contrôle après).

> **Compte restreint (`kelasiUser`)** : les scripts n'utilisent plus `information_schema`. Le script de correction utilise une procédure temporaire qui ignore les colonnes déjà présentes (erreur 1060).

## Résultats attendus après correction

| Vérification | Attendu |
|--------------|---------|
| Colonnes `Paiements` | `MontantNet`, `MontantCollecte`, `OperateurMobileMoney` |
| Tables MOKO | 6 tables présentes (peuvent être vides) |
| Colonne `Frais.IdAnneeScolaire` | Présente, `FraisSansAnnee = 0` |
| `__EFMigrationsHistory` | Entrées Moko + Frais année scolaire |

## Test API

```
GET https://prod-knb.asdc-rdc.org/api/Dashboard/global?idEcole=28
Authorization: Bearer <token>
```

Réponse attendue : **HTTP 200** avec `presence`, `paiement`, `statistiques`.

## Validation locale

Les mêmes scripts ont été validés sur `dev-knb_db` (colonnes créées, tests unitaires Dashboard OK).
