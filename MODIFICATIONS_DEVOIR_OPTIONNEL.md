# 🔧 Modifications : Fichier optionnel pour DevoirADomicile

**Date**: $(date)  
**Objectif**: Rendre le fichier optionnel et supprimer la restriction de taille minimale

---

## ✅ Modifications effectuées

### 1. **Fichier rendu optionnel dans `DevoirADomicileController.cs`**

#### Avant :
```csharp
public async Task<ActionResult<DevoirADomicileDto>> PublierDevoir(
    [FromForm] CreateDevoirADomicileDto dto,
    [FromForm] IFormFile fichier)  // ❌ Requis
```

#### Après :
```csharp
public async Task<ActionResult<DevoirADomicileDto>> PublierDevoir(
    [FromForm] CreateDevoirADomicileDto dto)  // ✅ Fichier récupéré manuellement

// Récupération manuelle du fichier (optionnel)
IFormFile? fichier = null;
if (Request.Form.Files != null && Request.Form.Files.Count > 0)
{
    foreach (var file in Request.Form.Files)
    {
        if (file.Name == "fichier" || file.Name == "file")
        {
            fichier = file;
            break;
        }
    }
    if (fichier == null && Request.Form.Files.Count > 0)
    {
        fichier = Request.Form.Files[0];
    }
}
```

### 2. **Validation modifiée**

#### Avant :
```csharp
if (!hasFile && !hasContent)
{
    return BadRequest(new { message = "Vous devez fournir soit un fichier, soit un contenu textuel pour le devoir." });
}
```

#### Après :
```csharp
// Le fichier et le contenu sont tous les deux optionnels
// Un devoir peut être créé avec seulement un titre et une description
```

### 3. **Restriction de taille minimale supprimée dans `AntivirusService.cs`**

#### Avant :
```csharp
// - Taille minimale (un fichier valide doit avoir au moins quelques bytes)
var minSize = extension == ".pdf" ? 1024 : 100; // PDF : 1 KB minimum, Images : 100 bytes minimum
if (fileStream.Length < minSize)
{
    _logger.LogWarning($"Fichier suspect (trop petit) : {fileName} ({fileStream.Length} bytes)");
    return false;
}
```

#### Après :
```csharp
// Note: La validation de taille minimale a été supprimée pour permettre les fichiers de toute taille
```

---

## 🧪 Script de test créé

Un script de test a été créé : `test-creer-devoir.sh`

### Utilisation :
```bash
./test-creer-devoir.sh
```

### Le script teste :
1. ✅ Création d'un devoir **SANS fichier** (avec contenu textuel)
2. ✅ Création d'un devoir **SANS fichier ET SANS contenu** (titre + description uniquement)
3. ✅ Création d'un devoir **AVEC fichier** (si disponible)

---

## 📋 Paramètres de test

Le script utilise vos identifiants :
- **Email**: `jk2@kelasinabiso.cd`
- **Mot de passe**: `12345678`
- **ID École**: `13`
- **ID Direction**: `20`
- **ID Classe**: `43`

---

## 🚀 Pour tester

1. **Redémarrer l'application** :
   ```bash
   dotnet run
   ```

2. **Exécuter le script de test** :
   ```bash
   ./test-creer-devoir.sh
   ```

3. **Ou tester manuellement avec curl** :
   ```bash
   # Authentification
   TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/Authentifier" \
       -H "Content-Type: application/json" \
       -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}' \
       | jq -r '.accessToken')

   # Créer un devoir SANS fichier
   curl -k -s -X POST "https://localhost:7102/api/DevoirADomicile" \
       -H "Authorization: Bearer ${TOKEN}" \
       -F "Titre=Devoir de test" \
       -F "Description=Description du devoir" \
       -F "Contenu=Contenu textuel du devoir" \
       -F "IdClasse=43" \
       -F "DateLimite=2025-12-10T23:59:59Z" \
       | jq '.'
   ```

---

## ✅ Résultat attendu

- ✅ Un devoir peut être créé **sans fichier**
- ✅ Un devoir peut être créé **sans fichier et sans contenu** (titre + description uniquement)
- ✅ Un devoir peut être créé **avec fichier** (optionnel)
- ✅ **Aucune restriction de taille minimale** pour les fichiers

---

## 📝 Notes

- Le fichier reste optionnel même si aucun contenu textuel n'est fourni
- La taille maximale reste limitée à 5 MB
- Les types de fichiers autorisés restent : PDF, JPG, PNG

