# 📋 Guide d'Exécution : Index Unique Composite en Production

**Date** : 1er décembre 2025  
**Scripts disponibles** : 2 versions (complète et simplifiée)

---

## 🎯 Choix du Script

### Script Complet : `CREATE_UNIQUE_INDEX_ELEVES_PRODUCTION.sql`
**Utiliser si** :
- ✅ Vous voulez vérifier les doublons avant création
- ✅ Vous voulez nettoyer les doublons automatiquement
- ✅ Vous voulez des vérifications détaillées
- ✅ Première exécution en production

### Script Simplifié : `CREATE_UNIQUE_INDEX_ELEVES_PRODUCTION_SIMPLE.sql`
**Utiliser si** :
- ✅ Vous avez déjà vérifié et nettoyé les doublons
- ✅ Vous voulez juste créer l'index rapidement
- ✅ Exécution rapide sans vérifications

---

## 🚀 Exécution en Production

### Option 1 : Via MySQL/MariaDB CLI

```bash
# Se connecter à la base de données
mysql -u kansa -pkansa2025 KelasiNaBisoDb

# Exécuter le script complet
source /chemin/vers/CREATE_UNIQUE_INDEX_ELEVES_PRODUCTION.sql

# Ou exécuter directement
mysql -u kansa -pkansa2025 KelasiNaBisoDb < CREATE_UNIQUE_INDEX_ELEVES_PRODUCTION.sql
```

### Option 2 : Via Outil Graphique (phpMyAdmin, DBeaver, etc.)

1. Ouvrir l'outil de gestion de base de données
2. Sélectionner la base `KelasiNaBisoDb`
3. Ouvrir l'onglet SQL
4. Copier-coller le contenu du script
5. Exécuter

### Option 3 : Via Entity Framework (Migration)

```bash
# Appliquer la migration
dotnet ef database update

# Ou spécifiquement cette migration
dotnet ef database update Add_Unique_Index_Eleves_Prevent_Duplicates
```

---

## ⚠️ Checklist Avant Exécution

- [ ] **Sauvegarde de la base de données** effectuée
- [ ] **Test sur environnement de staging** réussi
- [ ] **Doublons identifiés** et nettoyés si nécessaire
- [ ] **Période de faible activité** choisie
- [ ] **Script choisi** (complet ou simplifié)

---

## 📊 Vérifications Post-Exécution

### 1. Vérifier que l'Index Existe

```sql
SELECT 
    INDEX_NAME,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX SEPARATOR ', ') as Colonnes,
    NON_UNIQUE
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
GROUP BY INDEX_NAME, NON_UNIQUE;
```

**Résultat attendu** :
- `INDEX_NAME` : `IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe`
- `Colonnes` : `Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse`
- `NON_UNIQUE` : `0` (UNIQUE)

### 2. Tester l'Index (Optionnel)

```sql
-- Récupérer un élève existant
SET @test_id = (SELECT IdEleve FROM Eleves WHERE Statut = 1 LIMIT 1);
SET @test_nom = (SELECT Nom FROM Eleves WHERE IdEleve = @test_id);
SET @test_postnom = (SELECT Postnom FROM Eleves WHERE IdEleve = @test_id);
SET @test_prenom = (SELECT Prenom FROM Eleves WHERE IdEleve = @test_id);
SET @test_date = (SELECT DateNaissance FROM Eleves WHERE IdEleve = @test_id);
SET @test_tuteur = (SELECT IdTuteur FROM Eleves WHERE IdEleve = @test_id);
SET @test_classe = (SELECT IdClasse FROM Eleves WHERE IdEleve = @test_id);

-- Tenter d'insérer un doublon (devrait échouer)
-- ⚠️ NE PAS EXÉCUTER EN PRODUCTION - C'est juste un test
```

---

## 🔄 Rollback (Si Nécessaire)

### Supprimer l'Index

```sql
DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;
```

### Vérifier la Suppression

```sql
SELECT COUNT(*) as IndexExiste
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe';
```

**Résultat attendu** : `0` (index supprimé)

---

## 📝 Notes Importantes

### 1. **Erreur : Duplicate Entry**
Si vous obtenez l'erreur :
```
Error: Duplicate entry '...' for key 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
```

**Cause** : Des doublons existent encore dans la base

**Solution** :
1. Exécuter la section "ÉTAPE 1" du script complet pour identifier les doublons
2. Nettoyer les doublons avec la section "ÉTAPE 3" (décommentée et adaptée)
3. Réessayer de créer l'index

### 2. **Performance**
- ✅ L'index améliore les performances de recherche
- ⚠️ L'insertion peut être légèrement plus lente (acceptable, ~1-2ms)

### 3. **NULL dans l'Index**
- ⚠️ MariaDB/MySQL traite NULL comme une valeur distincte
- ⚠️ Plusieurs élèves avec `Nom=NULL` peuvent coexister
- ✅ Le code C# vérifie NULL avant création

### 4. **Statut**
- ⚠️ MariaDB ne supporte pas les index filtrés avec `WHERE Statut = 1`
- ✅ Le code C# vérifie `Statut = 1` avant création
- ✅ Les élèves inactifs peuvent avoir des "doublons" (normal)

---

## ✅ Résultat Attendu

Après exécution réussie :

1. ✅ Index créé : `IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe`
2. ✅ 6 colonnes dans l'index : Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
3. ✅ Type : UNIQUE
4. ✅ Protection contre les doublons activée

---

## 📞 Support

En cas de problème :

1. **Vérifier les logs** de l'exécution SQL
2. **Vérifier les doublons** avec la section ÉTAPE 1
3. **Vérifier que l'index existe** avec la section ÉTAPE 6
4. **Consulter** `README_INDEX_UNIQUE_ELEVES.md` pour plus de détails

---

## 🎯 Commandes Rapides

### Création Simple
```bash
mysql -u kansa -pkansa2025 KelasiNaBisoDb < CREATE_UNIQUE_INDEX_ELEVES_PRODUCTION_SIMPLE.sql
```

### Vérification
```sql
SELECT INDEX_NAME, NON_UNIQUE 
FROM INFORMATION_SCHEMA.STATISTICS 
WHERE TABLE_NAME = 'Eleves' 
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe';
```

### Rollback
```sql
DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;
```

