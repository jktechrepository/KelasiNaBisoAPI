# 📋 Guide : Index Unique Composite pour Prévenir les Doublons d'Élèves

**Date** : 1er décembre 2025  
**Migration** : `20251201194213_Add_Unique_Index_Eleves_Prevent_Duplicates`

---

## 🎯 Objectif

Créer un index unique composite en base de données pour empêcher la création de doublons d'élèves au niveau de la base de données, en complément de la vérification applicative.

---

## 📊 Index Créé

**Nom** : `IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe`

**Colonnes** :
- `Nom`
- `Postnom`
- `Prenom`
- `DateNaissance`
- `IdTuteur`
- `IdClasse`

**Type** : UNIQUE

**Filtre** : `Statut = 1` (uniquement pour les élèves actifs) - Note : MariaDB ne supporte pas les index filtrés, mais le code C# vérifie déjà Statut = 1

---

## ⚠️ IMPORTANT : Avant d'Appliquer en Production

### 1. Vérifier les Doublons Existants

Exécuter cette requête pour identifier les doublons :

```sql
SELECT 
    Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse,
    COUNT(*) as NombreDoublons,
    GROUP_CONCAT(IdEleve) as IdsEleves
FROM Eleves
WHERE Statut = 1
    AND Nom IS NOT NULL
    AND Postnom IS NOT NULL
    AND Prenom IS NOT NULL
    AND IdTuteur IS NOT NULL
    AND IdClasse IS NOT NULL
GROUP BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
HAVING COUNT(*) > 1;
```

### 2. Nettoyer les Doublons

Si des doublons existent, les nettoyer avant d'appliquer l'index. Voir le script SQL pour des exemples de nettoyage.

### 3. Vérifier qu'il n'y a Plus de Doublons

Cette requête doit retourner 0 :

```sql
SELECT COUNT(*) as NombreDoublonsActifs
FROM (
    SELECT Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse, COUNT(*) as Nb
    FROM Eleves
    WHERE Statut = 1
        AND Nom IS NOT NULL
        AND Postnom IS NOT NULL
        AND Prenom IS NOT NULL
        AND IdTuteur IS NOT NULL
        AND IdClasse IS NOT NULL
    GROUP BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
    HAVING COUNT(*) > 1
) as Doublons;
```

---

## 🚀 Application de la Migration

### Option 1 : Via Entity Framework (Recommandé)

```bash
# Appliquer la migration
dotnet ef database update

# Ou spécifiquement cette migration
dotnet ef database update Add_Unique_Index_Eleves_Prevent_Duplicates
```

### Option 2 : Via Script SQL Direct (Production)

```bash
# Exécuter le script SQL
mysql -u kansa -pkansa2025 KelasiNaBisoDb < Migrations/ADD_UNIQUE_INDEX_ELEVES_PRODUCTION.sql
```

---

## ✅ Vérification Post-Migration

### Vérifier que l'Index Existe

```sql
SELECT 
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX,
    NON_UNIQUE
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
ORDER BY SEQ_IN_INDEX;
```

**Résultat attendu** : 6 lignes (une par colonne de l'index), `NON_UNIQUE = 0`

---

## 🔄 Rollback (Si Nécessaire)

### Via Entity Framework

```bash
# Revenir à la migration précédente
dotnet ef database update <nom_migration_précédente>

# Ou supprimer cette migration
dotnet ef migrations remove
```

### Via SQL Direct

```sql
DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;
```

---

## 📝 Notes Importantes

### 1. **NULL dans l'Index**
- MariaDB/MySQL traite NULL comme une valeur distincte
- Plusieurs élèves avec `Nom=NULL` peuvent coexister
- C'est pourquoi le code C# vérifie les valeurs NULL avant création

### 2. **Performance**
- ✅ L'index améliore les performances de recherche
- ⚠️ L'insertion peut être légèrement plus lente (acceptable)

### 3. **Doublons Existants**
- ❌ Si des doublons existent, l'index ne peut pas être créé
- ✅ Nettoyer d'abord les doublons avec le script SQL

### 4. **Statut**
- ⚠️ MariaDB ne supporte pas les index filtrés avec `WHERE Statut = 1`
- ✅ Le code C# vérifie déjà `Statut = 1` avant création
- ✅ Les élèves inactifs peuvent avoir des "doublons" (normal)

---

## 🎯 Protection Complète

Avec cette migration + le code C#, vous avez une **double protection** :

1. ✅ **Vérification C#** : Meilleure expérience utilisateur, réutilise l'élève existant
2. ✅ **Index Unique BDD** : Protection absolue contre les race conditions

---

## 📞 Support

En cas de problème :
1. Vérifier les logs de migration
2. Vérifier les doublons existants
3. Vérifier que l'index a été créé correctement

