# 📋 Guide : Création du Template Excel pour Inscriptions

## Colonnes Requises

Le fichier Excel doit contenir les colonnes suivantes (dans l'ordre) :

### Colonnes Obligatoires

1. **Type** : "Inscription" ou "Réinscription"
2. **IdEcole** : ID numérique de l'école
3. **IdClasse** : ID numérique de la classe
4. **IdAnneeScolaire** : ID numérique de l'année scolaire
5. **DateInscription** : Date au format JJ/MM/AAAA ou AAAA-MM-JJ
6. **NomEleve** : Nom de l'élève
7. **PostnomEleve** : Postnom de l'élève
8. **PrenomEleve** : Prénom de l'élève
9. **GenreEleve** : "M" ou "F"
10. **DateNaissanceEleve** : Date au format JJ/MM/AAAA ou AAAA-MM-JJ
11. **LieuNaissanceEleve** : Lieu de naissance
12. **NationaliteEleve** : Nationalité
13. **NomCompletTuteur** : Nom complet du tuteur
14. **GenreTuteur** : "M" ou "F"

### Colonnes Optionnelles

15. **StatutInscription** : "En attente" (par défaut)
16. **PhotoEleveUrl** : URL de la photo de l'élève
17. **MatriculeEleve** : Matricule (généré automatiquement si vide)
18. **ProvinceEleve** : Province
19. **VilleEleve** : Ville
20. **CommuneEleve** : Commune
21. **QuartierEleve** : Quartier
22. **AvenueEleve** : Avenue
23. **NumeroEleve** : Numéro
24. **CommentaireEleve** : Commentaire
25. **EmailTuteur** : Email du tuteur
26. **TelephoneTuteur** : Téléphone du tuteur (format : 8-15 chiffres)
27. **NomCompletRepresentant** : Nom du représentant
28. **TelephoneRepresentant** : Téléphone du représentant
29. **PhotoTuteurUrl** : URL de la photo du tuteur
30. **PieceIdentiteTuteur** : Pièce d'identité
31. **IdEleveExistant** : ID de l'élève existant (pour réinscription)
32. **IdTuteurExistant** : ID du tuteur existant (pour réinscription)

## Format du Fichier

- **Format** : .xlsx (Excel 2007+)
- **Taille maximum** : 10 MB
- **Ligne 1** : En-têtes (noms des colonnes)
- **Lignes 2+** : Données

## Exemple de Données

| Type | IdEcole | IdClasse | IdAnneeScolaire | DateInscription | NomEleve | PostnomEleve | PrenomEleve | GenreEleve | DateNaissanceEleve | LieuNaissanceEleve | NationaliteEleve | NomCompletTuteur | GenreTuteur | EmailTuteur | TelephoneTuteur |
|------|---------|----------|-----------------|-----------------|----------|--------------|-------------|------------|-------------------|-------------------|------------------|------------------|-------------|-------------|-----------------|
| Inscription | 1 | 5 | 1 | 01/09/2024 | KABEYA | MULENGA | Jean | M | 15/05/2010 | Kinshasa | Congolaise | KABEYA MULENGA Pierre | M | pierre@example.com | +243900123456 |
| Inscription | 1 | 5 | 1 | 01/09/2024 | MUKAMBA | KASONGO | Marie | F | 20/08/2011 | Kinshasa | Congolaise | MUKAMBA KASONGO Paul | M | paul@example.com | +243900123457 |

## Notes Importantes

1. **Dates** : Format accepté : JJ/MM/AAAA, AAAA-MM-JJ, ou format Excel standard
2. **Genres** : Uniquement "M" (Masculin) ou "F" (Féminin)
3. **Doublons** : Les doublons dans le fichier seront détectés et rejetés
4. **Validation** : Toutes les données sont validées avant insertion
5. **Transactions** : Les inscriptions sont traitées par lots de 50 avec transactions

## Script de Création du Template

Un script PowerShell ou Python peut être créé pour générer automatiquement le template Excel avec validation.

