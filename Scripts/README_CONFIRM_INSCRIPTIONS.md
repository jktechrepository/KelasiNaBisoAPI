# Confirmer les inscriptions en attente (production)

Script : [`CONFIRM_INSCRIPTIONS_EN_ATTENTE.sql`](CONFIRM_INSCRIPTIONS_EN_ATTENTE.sql)

## Quand l'utiliser

Après déploiement de l'API qui normalise `StatutInscription` à la création/mise à jour, **les anciennes lignes** peuvent encore avoir `En attente` / `EN_ATTENTE`. Elles n'apparaissent pas dans :

- `GET /api/Eleve/ecole/{idEcole}`
- Dashboard, présence, paiements filtrés par année

Symptôme typique (diagnostic section 6) :

| InscriptionsTotal | ElevesApiEquivalent |
|-------------------|---------------------|
| 106 | 0 |

## Exécution phpMyAdmin (`knb_db`)

1. **Prévisualisation** (décommenter le SELECT en tête du script) :

```sql
USE knb_db;

SELECT IdInscription, IdEleve, IdEcole, IdAnneeScolaire, StatutInscription
FROM Inscriptions
WHERE Statut = 1
  AND (
    StatutInscription IS NULL
    OR TRIM(StatutInscription) = ''
    OR UPPER(REPLACE(StatutInscription, ' ', '_')) IN ('EN_ATTENTE', 'ENATTENTE')
    OR StatutInscription LIKE 'En attente%'
  );
```

2. Exécuter le `UPDATE` du script sur **`knb_db`**.

3. Vérifier avec [`DIAGNOSTIC_ELEVE_ECOLE_API.sql`](DIAGNOSTIC_ELEVE_ECOLE_API.sql) :
   - `@idEcole` / `@idAnneeScolaire` de l'école concernée
   - `ElevesApiEquivalent` doit être > 0

4. Retester l'API : `GET /api/Eleve/ecole/28?idAnneeScolaire=19`

## Nouvelles inscriptions (après déploiement code)

L'API force désormais `Confirmé` à la création et à la mise à jour (sauf `Annulé`) via `InscriptionActiveRules.NormalizeStatutInscription` dans :

- `InscriptionService.CreateInscriptionAsync`
- `InscriptionService.UpdateAsync`

Le client ou le frontend peut encore envoyer `EN_ATTENTE` — la valeur persistée sera **`Confirmé`**.
