# ⚡ COMMANDES RAPIDES - SOFT DELETE PRÉSENCE

**Date:** 16 Octobre 2025

---

## 🚀 APPLIQUER LA MIGRATION (3 commandes)

```powershell
# 1. Aller dans le répertoire
cd G:\KelasiNaBiso\KelasiNaBisoAPI

# 2. Exécuter le script (RECOMMANDÉ)
.\apply-soft-delete-migration.ps1

# OU manuellement:
dotnet ef migrations add AjoutSoftDeletePresence
dotnet ef database update
```

---

## 🧪 LANCER L'API

```powershell
# Build
dotnet build

# Run
dotnet run

# L'API sera disponible sur:
# - http://localhost:5002
# - https://localhost:7102
# - Swagger: http://localhost:5002/swagger
```

---

## ✅ TESTS RAPIDES

### Option 1: REST Client (VS Code)
```
Ouvrir: test-soft-delete-presence.http
Exécuter les requêtes une par une
```

### Option 2: PowerShell
```powershell
# Créer une présence
Invoke-RestMethod -Method POST -Uri "http://localhost:5002/api/Presence" `
-ContentType "application/json" -Body '{
  "idEleve": 1,
  "heureArrivee": "08:30",
  "heureDepart": "12:00",
  "dateDuJour": "2025-10-16",
  "statutPresence": "Present",
  "idHoraire": 1
}'

# Lister toutes les présences
Invoke-RestMethod -Uri "http://localhost:5002/api/Presence"

# Désactiver une présence (soft delete)
Invoke-RestMethod -Method PUT -Uri "http://localhost:5002/api/Presence/toggle-statut/1"

# Vérifier qu'elle est invisible
Invoke-RestMethod -Uri "http://localhost:5002/api/Presence"
```

---

## 🔍 VÉRIFIER EN BASE DE DONNÉES

```sql
-- Vérifier structure
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Presences';

-- Voir toutes les présences (actives et inactives)
SELECT IdPresence, IdEleve, Statut, StatutPresence, DateDuJour
FROM Presences
ORDER BY DateCreation DESC;

-- Compter actives/inactives
SELECT Statut, COUNT(*) as Nombre
FROM Presences
GROUP BY Statut;
```

---

## 📚 DOCUMENTATION COMPLÈTE

```
📄 IMPLEMENTATION_SOFT_DELETE_PRESENCE.md  (Guide complet - 50+ pages)
📄 ANALYSE_SYSTEME_POINTAGE_PRESENCE.md    (Analyse détaillée)
📄 test-soft-delete-presence.http          (13 scénarios de test)
📄 apply-soft-delete-migration.ps1         (Script automatique)
```

---

## 🎯 CE QUI A ÉTÉ FAIT

✅ Modèle Presence.cs modifié (Statut bool + StatutPresence string)
✅ PresenceController.cs mis à jour (endpoint toggle-statut)
✅ PresenceService.cs mis à jour (filtrage + ToggleStatutAsync)
✅ Interface IPresenceRepository.cs complétée
✅ Tests HTTP créés (13 scénarios)
✅ Documentation complète

---

## 🔄 CE QUI RESTE À FAIRE

1. Appliquer la migration (1 commande)
2. Tester l'API (fichier .http fourni)
3. Valider en base de données (requêtes SQL fournies)

---

## ⚡ PROCHAINE ÉTAPE

**Après validation du soft delete :**

```
Phase 2: JWT Authentication
Phase 3: Firebase Push Notifications
Phase 4: Email Service
Phase 5: Etc.
```

Voir: `GUIDE_DEMARRAGE_MIGRATION.md` pour le plan complet

---

**🚀 PRÊT À DÉMARRER !**

```powershell
cd G:\KelasiNaBiso\KelasiNaBisoAPI
.\apply-soft-delete-migration.ps1
```

