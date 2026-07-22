# 📋 Documentation : Bulk Insert Excel pour les Inscriptions

## 📖 Vue d'ensemble

Cette documentation explique comment utiliser l'API pour créer plusieurs inscriptions en lot à partir d'un fichier Excel. Le système permet d'importer des centaines d'inscriptions en une seule requête, avec validation automatique et gestion des erreurs.

**Endpoint principal :** `POST /api/Inscription/bulk-excel`

---

## 🔄 Flux de Traitement

```
1. Frontend : L'utilisateur sélectionne la classe, l'année scolaire et le type d'inscription
2. Frontend : L'utilisateur télécharge le template Excel (optionnel)
3. Frontend : L'utilisateur remplit le template avec les données
4. Frontend : L'utilisateur sélectionne le fichier Excel rempli
5. Frontend : Validation côté client (format, taille)
6. Frontend → Backend : POST /api/Inscription/bulk-excel avec FormData (token JWT dans les headers)
7. Backend : Extraction de l'ID utilisateur et de l'ID école depuis le token JWT
8. Backend : Validation du fichier (format, taille, colonnes)
9. Backend : Validation des paramètres (classe, année scolaire doivent appartenir à l'école de l'utilisateur)
10. Backend : Lecture du fichier Excel ligne par ligne
11. Backend : Validation de chaque ligne
12. Backend : Détection des doublons (dans le fichier et en base)
13. Backend : Traitement par lots de 50 inscriptions
14. Backend : Création des inscriptions (élèves, tuteurs, inscriptions)
15. Backend → Frontend : Retour du résultat avec statistiques et erreurs
16. Frontend : Affichage des résultats à l'utilisateur
```


### Paramètres requis avant l'upload
Avant de télécharger le fichier Excel, le frontend doit avoir :
- ✅ `idClasse` : ID de la classe dans laquelle inscrire les élèves
- ✅ `idAnneeScolaire` : ID de l'année scolaire
- ✅ `typeInscription` : Type d'inscription (`"Inscription"` ou `"Réinscription"`, par défaut `"Inscription"`)

> **Note importante** : 
> - L'ID de l'utilisateur et l'ID de l'école sont **automatiquement extraits du token JWT** côté serveur, il n'est donc **pas nécessaire** de les passer en paramètres.
> - Ces paramètres sont validés côté serveur. La classe et l'année scolaire doivent appartenir à l'école de l'utilisateur connecté.

---

## 📥 Télécharger le Template Excel

### Endpoint
```
GET /api/Inscription/template-excel
```

### Description
Télécharge un fichier Excel template pré-formaté avec les colonnes requises et des exemples de données.

### Exemple de requête (JavaScript)
```javascript
async function downloadTemplate() {
  try {
    const response = await fetch('https://votre-api.com/api/Inscription/template-excel', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      throw new Error('Erreur lors du téléchargement du template');
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Template_Inscriptions_${new Date().toISOString().split('T')[0]}.xlsx`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  } catch (error) {
    console.error('Erreur:', error);
  }
}
```

---

## 📊 Format du Fichier Excel

### Colonnes Requises (Obligatoires)

| Colonne | Type | Description | Exemple |
|---------|------|-------------|---------|
| `DateInscription` | Date | Date d'inscription | `2024-01-15` |
| `NomEleve` | Texte | Nom de l'élève | `MUKENDI` |
| `PostnomEleve` | Texte | Postnom de l'élève | `KALALA` |
| `PrenomEleve` | Texte | Prénom de l'élève | `Jean` |
| `GenreEleve` | Texte | Genre de l'élève (`M` ou `F`) | `M` |
| `DateNaissanceEleve` | Date | Date de naissance | `2010-05-15` |
| `LieuNaissanceEleve` | Texte | Lieu de naissance | `Kinshasa` |
| `NationaliteEleve` | Texte | Nationalité | `Congolaise` |
| `NomCompletTuteur` | Texte | Nom complet du tuteur | `MUKENDI Pierre` |
| `GenreTuteur` | Texte | Genre du tuteur (`M` ou `F`) | `M` |

### Colonnes Optionnelles

| Colonne | Type | Description | Exemple |
|---------|------|-------------|---------|
| `ProvinceEleve` | Texte | Province de résidence | `Kinshasa` |
| `VilleEleve` | Texte | Ville de résidence | `Kinshasa` |
| `CommuneEleve` | Texte | Commune | `Gombe` |
| `QuartierEleve` | Texte | Quartier | `Centre-ville` |
| `AvenueEleve` | Texte | Avenue | `Avenue de la République` |
| `NumeroEleve` | Texte | Numéro de maison | `123` |
| `CommentaireEleve` | Texte | Commentaires sur l'élève | `Élève brillant` |
| `PhotoEleveUrl` | Texte | URL de la photo | `https://...` |
| `MatriculeEleve` | Texte | Matricule (généré automatiquement si vide) | `ECOLE001-2024-001` |
| `EmailTuteur` | Texte | Email du tuteur | `pierre.mukendi@email.com` |
| `TelephoneTuteur` | Texte | Téléphone du tuteur | `+243900000000` |
| `NomCompletRepresentant` | Texte | Nom du représentant légal | `MUKENDI Marie` |
| `TelephoneRepresentant` | Texte | Téléphone du représentant | `+243900000001` |



---

## 🚀 Upload du Fichier Excel

### Endpoint
```
POST /api/Inscription/bulk-excel
```

### Paramètres de Requête (Query Parameters)

| Paramètre | Type | Requis | Description |
|-----------|------|--------|-------------|
| `idClasse` | `int` | ✅ Oui | ID de la classe |
| `idAnneeScolaire` | `int` | ✅ Oui | ID de l'année scolaire |
| `typeInscription` | `string` | ⚠️ Optionnel | Type d'inscription (`"Inscription"` ou `"Réinscription"`). Par défaut : `"Inscription"` |

> **Note** : L'ID de l'utilisateur et l'ID de l'école sont automatiquement extraits du token JWT côté serveur, il n'est donc pas nécessaire de les passer en paramètres.

### Body (Form Data)
- **Type** : `multipart/form-data`
- **Champ** : `file` (fichier Excel `.xlsx`)
- **Taille maximale** : 10 MB

> **Important** : Le nom du champ dans le FormData doit être exactement `"file"` pour que le serveur puisse le récupérer.

---

## 🔍 Validation et Gestion des Erreurs

### Validations Effectuées

1. **Validation du fichier**
   - Format : `.xlsx` uniquement
   - Taille : Maximum 10 MB
   - Présence des colonnes requises

2. **Validation des paramètres**
   - `idClasse`, `idAnneeScolaire` doivent être > 0
   - L'ID utilisateur et l'ID école sont extraits du token JWT et validés
   - La classe doit appartenir à l'école de l'utilisateur connecté
   - L'année scolaire doit appartenir à l'école de l'utilisateur connecté

3. **Validation des données par ligne**
   - Colonnes requises non vides
   - Format des dates valide
   - Genre doit être `M` ou `F`
   - Détection des doublons dans le fichier
   - Détection des doublons dans la base de données

### Gestion des Erreurs Côté Frontend


---



### 2. Feedback Utilisateur

- ✅ Afficher une barre de progression pendant le traitement
- ✅ Afficher un message de succès avec le nombre d'inscriptions créées
- ✅ Afficher les erreurs de manière claire et actionnable
- ✅ Permettre le téléchargement d'un rapport d'erreurs (CSV/Excel)

### 3. Gestion des Fichiers Volumineux

- Le traitement se fait par lots de 50 inscriptions
- Pour des fichiers très volumineux (> 1000 lignes), informer l'utilisateur que le traitement peut prendre plusieurs minutes
- Ne pas fermer la page pendant le traitement

### 4. Sécurité

- ✅ Toujours valider le token JWT avant l'upload
- ✅ Ne jamais exposer les tokens dans les logs
- ✅ Valider les paramètres côté frontend ET serveur
- ✅ Limiter la taille des fichiers (déjà fait côté serveur : 10 MB)

---

## 📝 Exemple Complet d'Intégration (Vue.js)

```vue
<template>
  <div class="bulk-upload-container">
    <h2>Import d'inscriptions en lot</h2>

    <!-- Sélection des paramètres -->
    <div class="form-group">
      <label>Classe :</label>
      <select v-model="selectedClasse" @change="onClasseChange">
        <option value="">Sélectionner une classe</option>
        <option v-for="classe in classes" :key="classe.id" :value="classe.id">
          {{ classe.nom }}
        </option>
      </select>
    </div>

    <div class="form-group">
      <label>Année scolaire :</label>
      <select v-model="selectedAnneeScolaire">
        <option value="">Sélectionner une année</option>
        <option v-for="annee in anneesScolaires" :key="annee.id" :value="annee.id">
          {{ annee.libelle }}
        </option>
      </select>
    </div>

    <div class="form-group">
      <label>Type d'inscription :</label>
      <select v-model="typeInscription">
        <option value="Inscription">Inscription</option>
        <option value="Réinscription">Réinscription</option>
      </select>
    </div>

    <!-- Téléchargement du template -->
    <button @click="downloadTemplate" class="btn-secondary">
      📥 Télécharger le template Excel
    </button>

    <!-- Upload du fichier -->
    <div class="form-group">
      <label>Fichier Excel :</label>
      <input 
        type="file" 
        accept=".xlsx" 
        @change="onFileSelect"
        :disabled="!canUpload"
      />
    </div>

    <button 
      @click="uploadFile" 
      :disabled="!canUpload || isUploading"
      class="btn-primary"
    >
      {{ isUploading ? '⏳ Traitement en cours...' : '📤 Importer' }}
    </button>

    <!-- Résultats -->
    <div v-if="result" class="results">
      <h3>Résultats du traitement</h3>
      <div class="stats">
        <p>✅ Réussies : {{ result.lignesReussies }} / {{ result.totalLignes }}</p>
        <p>❌ Échouées : {{ result.lignesEchouees }}</p>
        <p v-if="result.doublonsDetectes > 0">
          ⚠️ Doublons : {{ result.doublonsDetectes }}
        </p>
      </div>

      <div v-if="result.lignesAvecErreurs.length > 0" class="errors">
        <h4>Erreurs détectées :</h4>
        <ul>
          <li v-for="ligne in result.lignesAvecErreurs" :key="ligne.numeroLigne">
            Ligne {{ ligne.numeroLigne }} : {{ ligne.erreurs.join(', ') }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue';
import axios from 'axios';

export default {
  name: 'BulkInscriptionUpload',
  setup() {
    const selectedClasse = ref(null);
    const selectedAnneeScolaire = ref(null);
    const typeInscription = ref('Inscription');
    const selectedFile = ref(null);
    const isUploading = ref(false);
    const result = ref(null);

    const canUpload = computed(() => {
      return selectedClasse.value && 
             selectedAnneeScolaire.value && 
             selectedFile.value;
    });

    const downloadTemplate = async () => {
      try {
        const response = await axios.get('/api/Inscription/template-excel', {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          },
          responseType: 'blob'
        });

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Template_Inscriptions_${new Date().toISOString().split('T')[0]}.xlsx`);
        document.body.appendChild(link);
        link.click();
        link.remove();
      } catch (error) {
        console.error('Erreur lors du téléchargement du template:', error);
        alert('Erreur lors du téléchargement du template');
      }
    };

    const onFileSelect = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (!file.name.endsWith('.xlsx')) {
          alert('Le fichier doit être au format Excel (.xlsx)');
          return;
        }
        if (file.size > 10 * 1024 * 1024) {
          alert('Le fichier est trop volumineux (maximum 10 MB)');
          return;
        }
        selectedFile.value = file;
      }
    };

    const uploadFile = async () => {
      if (!canUpload.value) return;

      isUploading.value = true;
      result.value = null;

      try {
        const formData = new FormData();
        formData.append('file', selectedFile.value);

        const response = await axios.post(
          '/api/Inscription/bulk-excel',
          formData,
          {
            params: {
              idClasse: selectedClasse.value,
              idAnneeScolaire: selectedAnneeScolaire.value,
              typeInscription: typeInscription.value
            },
            headers: {
              'Authorization': `Bearer ${localStorage.getItem('token')}`,
              'Content-Type': 'multipart/form-data'
            }
          }
        );

        result.value = response.data;

        if (result.value.success && result.value.lignesReussies === result.value.totalLignes) {
          alert(`✅ Toutes les inscriptions ont été créées avec succès !`);
        } else if (result.value.lignesReussies > 0) {
          alert(`⚠️ ${result.value.lignesReussies} inscription(s) créée(s) sur ${result.value.totalLignes}`);
        } else {
          alert('❌ Aucune inscription n\'a été créée. Veuillez vérifier les erreurs.');
        }
      } catch (error) {
        console.error('Erreur lors de l\'upload:', error);
        const message = error.response?.data?.message || 'Erreur lors de l\'upload';
        alert(`Erreur : ${message}`);
      } finally {
        isUploading.value = false;
      }
    };

    return {
      selectedClasse,
      selectedAnneeScolaire,
      typeInscription,
      selectedFile,
      isUploading,
      result,
      canUpload,
      downloadTemplate,
      onFileSelect,
      uploadFile
    };
  }
};
</script>
```

---



**Dernière mise à jour :** 2024-01-15  
**Version de l'API :** 2.0 (Format simplifié)
