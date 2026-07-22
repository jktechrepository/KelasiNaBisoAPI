# 🚀 Exécution Immédiate du Script SQL

## 📍 Situation Actuelle

MySQL n'est pas installé localement, la base de données est probablement sur le serveur distant `dev-knb.asdc-rdc.org`.

## ✅ **OPTION 1 : Via SSH (Recommandé pour Production)**

### Étape 1 : Se connecter au serveur
```bash
ssh utilisateur@dev-knb.asdc-rdc.org
```

### Étape 2 : Naviguer vers le projet
```bash
cd /chemin/vers/KelasiNaBisoAPI
```

### Étape 3 : Créer une sauvegarde
```bash
mysqldump -u kansa -p dev-knb_db PaiementsCrashed > backup_paiement_crashed_$(date +%Y%m%d_%H%M%S).sql
```
Entrez le mot de passe : `kansa@2025`

### Étape 4 : Exécuter le script
```bash
mysql -u kansa -p dev-knb_db < Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql
```
Entrez le mot de passe : `kansa@2025`

---

## ✅ **OPTION 2 : Via phpMyAdmin (Plus Simple)**

1. **Ouvrir phpMyAdmin** : `https://dev-knb.asdc-rdc.org/phpmyadmin` (ou l'URL de votre serveur)

2. **Se connecter** avec :
   - Utilisateur : `kansa`
   - Mot de passe : `kansa@2025`
   - Base de données : `dev-knb_db`

3. **Créer une sauvegarde** (optionnel mais recommandé) :
   - Cliquer sur `dev-knb_db` dans le menu de gauche
   - Onglet "Exporter"
   - Sélectionner uniquement la table `PaiementsCrashed`
   - Cliquer sur "Exécuter"

4. **Exécuter le script SQL** :
   - Cliquer sur l'onglet "SQL" (en haut)
   - Ouvrir le fichier : `/Users/mac/Desktop/KelasiNaBisoAPI/Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql`
   - Copier tout le contenu
   - Coller dans la zone de texte SQL
   - Cliquer sur "Exécuter"

5. **Vérifier le résultat** :
   - Vous devriez voir : `✅ Migration terminée avec succès`
   - Vérifier que les colonnes sont maintenant NULLABLE

---

## ✅ **OPTION 3 : Via MySQL Workbench (Si installé)**

1. **Ouvrir MySQL Workbench**

2. **Créer une nouvelle connexion** :
   - Hostname : `dev-knb.asdc-rdc.org` (ou l'IP du serveur)
   - Port : `3306`
   - Username : `kansa`
   - Password : `kansa@2025`
   - Default Schema : `dev-knb_db`

3. **Se connecter**

4. **Ouvrir le script SQL** :
   - File → Open SQL Script
   - Sélectionner : `/Users/mac/Desktop/KelasiNaBisoAPI/Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql`

5. **Exécuter** :
   - Cliquer sur l'icône "Execute" (⚡) ou `Ctrl+Shift+Enter`

---

## ✅ **OPTION 4 : Via l'Application (Endpoint Temporaire)**

Si vous avez accès au code de l'application, vous pouvez créer un endpoint temporaire pour exécuter le script :

```csharp
[HttpPost("admin/execute-migration")]
[Authorize(Roles = "Super-Admin")]
public async Task<IActionResult> ExecuteMigration()
{
    var sql = System.IO.File.ReadAllText("Migrations/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql");
    await _context.Database.ExecuteSqlRawAsync(sql);
    return Ok(new { message = "Migration exécutée avec succès" });
}
```

⚠️ **À supprimer après utilisation !**

---

## 🔍 **VÉRIFICATION APRÈS EXÉCUTION**

Exécutez cette requête SQL pour vérifier :

```sql
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    COLUMN_TYPE
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    TABLE_SCHEMA = 'dev-knb_db' AND 
    TABLE_NAME = 'PaiementsCrashed' AND 
    COLUMN_NAME IN ('DateEchec', 'DateCreation');
```

**Résultat attendu :**
```
+--------------+-------------+-------------+
| COLUMN_NAME  | IS_NULLABLE | COLUMN_TYPE |
+--------------+-------------+-------------+
| DateEchec    | YES         | datetime    |
| DateCreation | YES         | datetime    |
+--------------+-------------+-------------+
```

---

## 📝 **APRÈS L'EXÉCUTION**

1. ✅ **Redéployer l'application** pour que les modifications du code prennent effet
2. ✅ **Tester l'API** : `GET /api/PaiementCrashed/ecole`
3. ✅ **Vérifier les logs** pour confirmer qu'il n'y a plus d'erreur

---

## ⚠️ **EN CAS DE PROBLÈME**

Si vous rencontrez une erreur :

1. **Vérifier les permissions** de l'utilisateur MySQL
2. **Vérifier que la table existe** : `SHOW TABLES LIKE 'PaiementsCrashed';`
3. **Vérifier les logs MySQL** : `/var/log/mysql/error.log`
4. **Restaurer la sauvegarde** si nécessaire

---

**💡 Recommandation : Utilisez l'Option 2 (phpMyAdmin) si vous avez accès, c'est la plus simple et la plus sûre.**

