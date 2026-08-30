# Modèle inscription — source de vérité parcours scolaire

## Règles métier

- **Inscription active** : `Statut == true` ET `StatutInscription` confirmé (`Confirmé` / variantes `Confirm*`).
- **Une inscription active par élève et par année scolaire** (contrainte index unique).
- **Classe / école courantes** : lues via `IInscriptionActiveResolver`, pas via `Eleve.IdClasse` ni `Tuteur.IdEcole`.
- **Tuteur** : identité globale (téléphone/email). Lien à une école = au moins un enfant avec inscription active dans cette école.

## API technique

- `IInscriptionActiveResolver.GetInscriptionActiveAsync(idEleve, idAnneeScolaire?)`
- `GetClasseCouranteAsync` / `GetEcoleCouranteAsync`
- `FilterElevesInClasse`, `FilterElevesInEcole`, `FilterTuteursInEcole` pour les requêtes EF

## Legacy (déprécié)

- `Eleve.IdClasse` / `Eleve.Classe` : **non mappés EF** (`[NotMapped]`). Ne génèrent plus de SQL. Classe courante uniquement via `Inscription`.
- `Tuteur.IdEcole` : ne plus écrire ; repli lecture uniquement dans les filtres.
- Endpoints élèves par école : `GET /api/Eleve/ecole/{idEcole}?idAnneeScolaire=` (année courante si omis).

## Commandes

```bash
dotnet ef database update --project KelasiNaBiso.csproj
```
