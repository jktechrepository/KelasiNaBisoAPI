# Templates FastReport — cartes scolaires

Templates actifs :
- **`RectoAgent.frx`** — carte personnel (recto + verso)
- **`RectoEleve.frx`** — carte eleve (recto + verso)
- **`BulletinEleve.frx`** — bulletin scolaire eleve (A4 portrait)

`CarteAgent.frx` / `CarteEleve.frx` sont d'anciens fichiers (placeholder / demo) — **ne pas les utiliser**.

## Important — apercu dans l'app web

L'application web appelle l'API deployee, **pas** le fichier ouvert dans FastReport Designer.

Apres modification d'un `.frx` : **redemarrer l'API**.

## Format CR80

- Taille : **86 x 54 mm**
- Source de donnees : `Carte`

### Agent — `RectoAgent.frx`
- **Recto** (`Data1`) : `Noms`, `Fonction`, `Matricule`, `Adresse`, `Ecole`, `Photo`, `Logo`, `Barcode`
- **Verso** (`Data2`) : `fond` + `logo1` ← logo ecole (`Carte.Logo`)

### Eleve — `RectoEleve.frx`
- **recto** (`Data1`) : `ecole`, `logo`, `photo`, `nomcomplet`, `matricule`, `classe`, `adresse`, `anneescolaire`
- **verso** (`Data2`) : `fond1` + `logo1` ← logo ecole (`Carte.Logo`)

## Bulletin eleve — `BulletinEleve.frx`

- Format : **A4 portrait** (210 x 297 mm)
- Entete (champs texte fixes) : `txtEcole`, `txtEleve`, `txtClasse`, `txtAnnee`, `txtPeriode`, `txtMoyenne`, `txtRang`, `txtEffectif`
- Bandeau detail : `LignesBand` sur `BulletinLignes` (`NomCours`, `MoyenneCoursTexte`, `CoefficientTexte`, `DetailNotes`)

## Comportement API

Si le `.frx` est absent ou invalide, fallback programmatique (`CarteReportLayoutBuilder` pour les cartes, `BulletinReportLayoutBuilder` pour les bulletins).
