# Résolution du problème du champ Genre

## Problème rencontré

L'erreur suivante s'est produite lors de l'ajout du champ Genre au modèle Utilisateur :

```
Microsoft.Data.SqlClient.SqlException (0x80131904): Nom de colonne non valide : 'Genre'.
```

## Cause du problème

Le problème est survenu car :
1. La vue `V_Utilisateur` existait déjà dans la base de données
2. Cette vue essayait d'accéder au champ `Genre` qui n'existait pas encore dans la table `Utilisateurs`
3. Entity Framework ne pouvait pas appliquer la migration à cause de cette vue

## Solution

### Étape 1 : Exécuter le script SQL de correction

1. Ouvrir SQL Server Management Studio (SSMS)
2. Se connecter à la base de données
3. Ouvrir le fichier `resolve-genre-issue.sql`
4. Exécuter le script SQL

Le script va :
- Supprimer la vue `V_Utilisateur` existante
- Ajouter la colonne `Genre` à la table `Utilisateurs`
- Recréer la vue `V_Utilisateur` avec le champ `Genre`

### Étape 2 : Tester l'API

Après avoir exécuté le script SQL, tester l'API avec le fichier `test-genre-utilisateur.http`.

### Étape 3 : Nettoyer les migrations inutiles (optionnel)

Si tout fonctionne correctement, vous pouvez supprimer les migrations inutiles avec le script `cleanup-migrations.ps1`.

## Fichiers créés/modifiés

### Modèles
- ✅ `Models/Utilisateur.cs` - Ajout du champ `Genre`
- ✅ `Models/V_Utilisateur.cs` - Ajout du champ `Genre`

### Scripts de résolution
- ✅ `resolve-genre-issue.sql` - Script SQL pour corriger le problème
- ✅ `resolve-genre-issue.ps1` - Guide pour résoudre le problème
- ✅ `cleanup-migrations.ps1` - Script pour nettoyer les migrations inutiles

### Tests
- ✅ `test-genre-utilisateur.http` - Tests pour vérifier le fonctionnement

## Utilisation du champ Genre

Le champ Genre est maintenant disponible dans :

1. **Création d'utilisateur** : Inclure `"genre": "Masculin"` ou `"genre": "Féminin"`
2. **Mise à jour d'utilisateur** : Modifier via l'API PUT
3. **Récupération d'utilisateur** : Inclus dans les réponses JSON
4. **Vue V_Utilisateur** : Disponible pour les requêtes complexes

## Exemple d'utilisation

```json
{
  "nomUtilisateur": "Test",
  "postNomUtilisateur": "Utilisateur",
  "prenomUtilisateur": "Genre",
  "email": "test.genre@example.com",
  "téléphone": "+243123456789",
  "genre": "Masculin",
  "motDePasseHash": "password123",
  "idRole": 1,
  "idEcole": 1
}
```

## Notes importantes

- Le champ `Genre` est de type `string` et peut être `null`
- Les valeurs recommandées sont "Masculin" et "Féminin"
- Le champ est inclus dans la vue `V_Utilisateur` pour les requêtes complexes
- Tous les contrôleurs existants supportent automatiquement ce nouveau champ
