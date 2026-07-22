# 📋 Guide d'Exécution : Migration Contenu DevoirADomicile

**Date** : 1er décembre 2025  
**Fichier SQL** : `ADD_CONTENU_NULLABLE_FIELDS_DEVOIR_PRODUCTION.sql`

---

## 🎯 Objectif

Modifier la table `DevoirsADomicile` pour :
1. ✅ Ajouter le champ `Contenu` (textuel, max 5000 caractères)
2. ✅ Rendre nullable les champs `NomFichier`, `CheminFichier`, `TailleFichier`, `TypeMIME`

**Raison** : Permettre la création de devoirs avec uniquement du contenu textuel (sans fichier).

---

## 📊 Modifications de la Base de Données

| Colonne | Avant | Après |
|---------|-------|-------|
| `Contenu` | ❌ N'existe pas | ✅ VARCHAR(5000), NULL |
| `NomFichier` | NOT NULL | ✅ NULL |
| `CheminFichier` | NOT NULL | ✅ NULL |
| `TailleFichier` | NOT NULL | ✅ NULL |
| `TypeMIME` | NOT NULL | ✅ NULL |

---

## 📝 Instructions d'Exécution

### Option 1 : Via MySQL/MariaDB CLI

```bash
# Se connecter à la base de données
mysql -u root -p FactureNormaliseeRDC

# Exécuter le script
source /chemin/vers/ADD_CONTENU_NULLABLE_FIELDS_DEVOIR_PRODUCTION.sql
```

### Option 2 : Via phpMyAdmin ou Adminer

1. Ouvrir phpMyAdmin/Adminer
2. Sélectionner la base de données `FactureNormaliseeRDC`
3. Aller dans l'onglet "SQL"
4. Copier-coller le contenu du fichier `ADD_CONTENU_NULLABLE_FIELDS_DEVOIR_PRODUCTION.sql`
5. Cliquer sur "Exécuter"

### Option 3 : Via un client SQL (DBeaver, MySQL Workbench, etc.)

1. Ouvrir le fichier `ADD_CONTENU_NULLABLE_FIELDS_DEVOIR_PRODUCTION.sql`
2. Se connecter à la base de données de production
3. Exécuter le script complet

---

## ✅ Vérification Post-Exécution

Après l'exécution du script, vérifier que :

1. **La colonne `Contenu` existe** :
```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME = 'Contenu';
```

2. **Les champs sont nullable** :
```sql
SELECT COLUMN_NAME, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME IN ('NomFichier', 'CheminFichier', 'TailleFichier', 'TypeMIME');
```

3. **Les données existantes sont intactes** :
```sql
SELECT 
    COUNT(*) AS Total,
    COUNT(NomFichier) AS AvecFichier,
    COUNT(Contenu) AS AvecContenu
FROM DevoirsADomicile;
```

---

## 🔒 Sécurité

- ✅ Le script utilise des vérifications conditionnelles pour éviter les erreurs
- ✅ Les données existantes sont préservées
- ✅ Aucune suppression de données
- ✅ Le script est idempotent (peut être exécuté plusieurs fois sans erreur)

---

## ⚠️ Notes Importantes

1. **Backup recommandé** : Faire une sauvegarde de la base de données avant l'exécution
2. **Exécution unique** : Le script peut être exécuté plusieurs fois sans problème (idempotent)
3. **Production uniquement** : Ce script est destiné à la base de données de production
4. **Vérification** : Toujours vérifier les résultats après l'exécution

---

## 📚 Documentation Associée

- `RESUME_MODIFICATIONS_DEVOIR_ADOMICILE.md` : Résumé détaillé des modifications
- `Models/DevoirADomicile.cs` : Modèle mis à jour
- `Controllers/DevoirADomicileController.cs` : Contrôleur adapté

---

## 🔄 Rollback (Si Nécessaire)

Si vous devez annuler les modifications, exécutez :

```sql
-- Supprimer la colonne Contenu
ALTER TABLE DevoirsADomicile DROP COLUMN Contenu;

-- Rendre les champs NOT NULL (⚠️ Nécessite que tous les enregistrements aient des valeurs)
ALTER TABLE DevoirsADomicile 
MODIFY COLUMN TypeMIME VARCHAR(100) CHARACTER SET utf8mb4 NOT NULL,
MODIFY COLUMN TailleFichier BIGINT NOT NULL,
MODIFY COLUMN NomFichier VARCHAR(500) CHARACTER SET utf8mb4 NOT NULL,
MODIFY COLUMN CheminFichier VARCHAR(1000) CHARACTER SET utf8mb4 NOT NULL;
```

**⚠️ ATTENTION** : Le rollback nécessite que tous les enregistrements aient des valeurs non-null pour les champs de fichier.

---

## 📞 Support

En cas de problème :
1. Vérifier les logs d'erreur MySQL/MariaDB
2. Vérifier que la table `DevoirsADomicile` existe
3. Vérifier les permissions de l'utilisateur de base de données

