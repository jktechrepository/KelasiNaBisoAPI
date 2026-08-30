# Diagnostic — GET /api/Eleve/ecole/{idEcole}

Script : [`DIAGNOSTIC_ELEVE_ECOLE_API.sql`](DIAGNOSTIC_ELEVE_ECOLE_API.sql)

## Usage (phpMyAdmin)

1. Ouvrir la base `knb_db`
2. Modifier en tête de script :
   - `@idEcole` (ex. 28)
   - `@idAnneeScolaire` (ex. 19)
   - `@page`, `@pageSize`
3. Exécuter le script section par section ou en entier

## Interprétation

| Résultat section 6 | Signification |
|--------------------|---------------|
| `InscriptionsTotal > 0` et `ElevesApiEquivalent = 0` | Des inscriptions existent mais **aucune confirmée** → liste API vide |
| Section 4 non vide | Voir colonne `RaisonExclusion` |

## Filtres API (obligatoires)

- `Eleves.Statut = 1`
- `Inscriptions.Statut = 1`
- `Inscriptions.IdEcole` + `IdAnneeScolaire`
- `StatutInscription` = `Confirmé`, `Confirme`, ou commence par `Confirm`
- **Exclut** : `En attente`, `EN_ATTENTE`, NULL, etc.

## Cas école 28 / année 19 (dev-knb_db)

Diagnostic local : **106 inscriptions**, toutes `En attente` / `EN_ATTENTE` → **totalCount API = 0**.

Correction possible en SQL (prod, après validation métier) :

```sql
UPDATE Inscriptions
SET StatutInscription = 'Confirmé'
WHERE IdEcole = 28 AND IdAnneeScolaire = 19
  AND Statut = 1
  AND (StatutInscription IN ('En attente', 'EN_ATTENTE', 'En Attente')
       OR StatutInscription LIKE 'En%attente%');
```

Ou utiliser l'endpoint / workflow de confirmation d'inscriptions existant dans l'API.
