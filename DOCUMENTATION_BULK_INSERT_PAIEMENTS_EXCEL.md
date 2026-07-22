# 💰 Documentation : Bulk Insert Excel pour les Paiements

## 📖 Vue d'ensemble

Cette documentation explique comment utiliser l'API pour créer plusieurs paiements en lot à partir d'un fichier Excel. Le système permet d'importer des centaines de paiements en une seule requête, avec validation automatique et gestion des erreurs.

**Endpoint principal :** `POST /api/Paiement/bulk-excel`

---


### Prérequis

#### Authentification
- **Token JWT requis** : L'utilisateur doit être authentifié et posséder un token JWT valide
- **Rôles autorisés** : `Admin`, `Super-Admin`, ou `Directeur`
- Le token doit être inclus dans l'en-tête `Authorization` : `Bearer {token}`

> **Note importante** : 
> - L'ID de l'utilisateur et l'ID de l'école sont **automatiquement extraits du token JWT** côté serveur, il n'est donc **pas nécessaire** de les passer en paramètres.
> - Les élèves et frais référencés dans le fichier Excel doivent appartenir à l'école de l'utilisateur connecté.

---

## 📥 Télécharger le Template Excel

### Endpoint
```
GET /api/Paiement/template-excel
```

### Description
Télécharge un fichier Excel template pré-formaté avec les colonnes requises et des exemples de données.

---

## 📊 Format du Fichier Excel

### Colonnes Requises (Obligatoires)

| Colonne | Type | Description | Exemple |
|---------|------|-------------|---------|
| `DatePaiement` | Date | Date du paiement | `2024-01-15` |
| `Montant` | Nombre | Montant payé | `100` |
| `Devise` | Texte | Devise du paiement (`USD`, `CDF`, etc.) | `USD` |
| `ModePaiement` | Texte | Mode de paiement (`Cash`, `Carte`, `Mobile Money`, etc.) | `Cash` |
| `NomCompletEleve` | Texte | Nom complet de l'élève (doit correspondre exactement à un élève de l'école) | `MUKENDI Jean Pierre` |
| `LibelleFrais` | Texte | Libellé du type de frais (doit correspondre exactement à un frais de l'école) | `Minerval` |

### ⚠️ Important

- **NomCompletEleve** : Le nom doit correspondre **exactement** (insensible à la casse) au nom complet d'un élève actif de l'école
- **LibelleFrais** : Le libellé doit correspondre **exactement** (insensible à la casse) au libellé d'un frais actif de l'école
- Le système charge tous les élèves et frais de l'école en mémoire pour une recherche rapide
- Les espaces en début/fin sont automatiquement supprimés lors de la correspondance

---

## 🚀 Upload du Fichier Excel

### Endpoint
```
POST /api/Paiement/bulk-excel
```

### Body (Form Data)
- **Type** : `multipart/form-data`
- **Champ** : `file` (fichier Excel `.xlsx`)
- **Taille maximale** : 10 MB

---

## 📤 Réponse de l'API

### Structure de la Réponse (Succès - 200 OK)

```json
{
  "success": true,
  "message": "Traitement terminé : 45 paiement(s) réussi(s) sur 50 ligne(s)",
  "totalLignes": 50,
  "lignesReussies": 45,
  "lignesEchouees": 5,
  "doublonsDetectes": 2,
  "dateTraitement": "2024-01-15T10:30:00Z",
  "lignesAvecErreurs": [
    {
      "numeroLigne": 3,
      "datePaiement": "2024-01-15",
      "montant": 100,
      "nomCompletEleve": "DUPONT Jean",
      "libelleFrais": "Minerval",
      "erreurs": [
        "Élève non trouvé : DUPONT Jean",
        "Le montant doit être supérieur à 0"
      ]
    }
  ],
  "paiementsCrees": [
    {
      "idPaiement": 123,
      "datePaiement": "2024-01-15T10:00:00Z",
      "montant": 100,
      "devise": "USD",
      "modePaiement": "Cash",
      "statutPaiement": "Confirmé",
      "idEleve": 456,
      "idFrais": 789
    }
  ]
}
```

### Propriétés de la Réponse

| Propriété | Type | Description |
|-----------|------|-------------|
| `success` | `boolean` | Indique si le traitement global a réussi |
| `message` | `string` | Message récapitulatif du traitement |
| `totalLignes` | `int` | Nombre total de lignes dans le fichier Excel |
| `lignesReussies` | `int` | Nombre de paiements créés avec succès |
| `lignesEchouees` | `int` | Nombre de lignes ayant échoué |
| `doublonsDetectes` | `int` | Nombre de doublons détectés dans le fichier |
| `dateTraitement` | `DateTime` | Date et heure du traitement |
| `lignesAvecErreurs` | `Array<PaiementExcelDto>` | Liste des lignes avec erreurs |
| `paiementsCrees` | `Array<Paiement>` | Liste des paiements créés avec succès |

### Réponses d'Erreur

#### 400 Bad Request - Fichier manquant
```json
{
  "message": "Le fichier Excel est requis"
}
```

#### 401 Unauthorized - Token JWT invalide
```json
{
  "message": "Token JWT invalide ou utilisateur non identifié"
}
```

#### 401 Unauthorized - Aucune école associée
```json
{
  "message": "Token JWT invalide : aucune école associée à l'utilisateur"
}
```

#### 400 Bad Request - Format de fichier invalide
```json
{
  "success": false,
  "message": "Le fichier doit être au format Excel (.xlsx)"
}
```

#### 400 Bad Request - Fichier trop volumineux
```json
{
  "success": false,
  "message": "Le fichier est trop volumineux (maximum 10 MB)"
}
```

#### 401 Unauthorized - Token manquant ou invalide
```json
{
  "message": "Non autorisé"
}
```

#### 403 Forbidden - Rôle insuffisant
```json
{
  "message": "Accès refusé. Rôles autorisés : Admin, Super-Admin, Directeur"
}
```

#### 500 Internal Server Error
```json
{
  "message": "Erreur lors du traitement du fichier Excel",
  "error": "Détails de l'erreur technique"
}
```

---

## 🔍 Validation et Gestion des Erreurs

### Validations Effectuées

1. **Validation du fichier**
   - Format : `.xlsx` uniquement
   - Taille : Maximum 10 MB
   - Présence des colonnes requises

2. **Validation des paramètres**
   - L'ID utilisateur et l'ID école sont extraits du token JWT et validés

3. **Validation des données par ligne**
   - Colonnes requises non vides
   - Format des dates valide
   - Montant doit être > 0
   - NomCompletEleve doit correspondre à un élève de l'école
   - LibelleFrais doit correspondre à un frais de l'école
   - Détection des doublons dans le fichier (basée sur DatePaiement + IdEleve + IdFrais + Montant)

---

## 💻 Exemple d'Intégration (Vue.js)

### Composant Vue.js Complet

```vue
<template>
  <div class="bulk-paiement-upload">
    <h2>Import de paiements en lot</h2>

    <!-- Téléchargement du template -->
    <div class="mb-4">
      <button 
        @click="downloadTemplate" 
        class="btn btn-secondary"
        :disabled="isDownloading"
      >
        {{ isDownloading ? '⏳ Téléchargement...' : '📥 Télécharger le template Excel' }}
      </button>
    </div>

    <!-- Upload du fichier -->
    <div class="form-group mb-4">
      <label for="fileInput" class="form-label">
        Fichier Excel (.xlsx) :
      </label>
      <input 
        id="fileInput"
        type="file" 
        accept=".xlsx" 
        @change="onFileSelect"
        :disabled="isUploading"
        class="form-control"
      />
      <small class="form-text text-muted">
        Taille maximale : 10 MB
      </small>
    </div>

    <!-- Bouton d'upload -->
    <button 
      @click="uploadFile" 
      :disabled="!canUpload || isUploading"
      class="btn btn-primary"
    >
      {{ isUploading ? '⏳ Traitement en cours...' : '📤 Importer les paiements' }}
    </button>

    <!-- Résultats -->
    <div v-if="result" class="mt-5">
      <h3>Résultats du traitement</h3>
      
      <!-- Statistiques -->
      <div class="alert" :class="getAlertClass()">
        <h4>{{ result.message }}</h4>
        <ul class="mb-0">
          <li>✅ Réussies : <strong>{{ result.lignesReussies }}</strong> / {{ result.totalLignes }}</li>
          <li>❌ Échouées : <strong>{{ result.lignesEchouees }}</strong></li>
          <li v-if="result.doublonsDetectes > 0">
            ⚠️ Doublons : <strong>{{ result.doublonsDetectes }}</strong>
          </li>
        </ul>
      </div>

      <!-- Erreurs détaillées -->
      <div v-if="result.lignesAvecErreurs.length > 0" class="mt-4">
        <h4>Erreurs détectées :</h4>
        <div class="table-responsive">
          <table class="table table-sm table-bordered">
            <thead>
              <tr>
                <th>Ligne</th>
                <th>Élève</th>
                <th>Frais</th>
                <th>Montant</th>
                <th>Erreurs</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="ligne in result.lignesAvecErreurs" :key="ligne.numeroLigne">
                <td>{{ ligne.numeroLigne }}</td>
                <td>{{ ligne.nomCompletEleve || 'N/A' }}</td>
                <td>{{ ligne.libelleFrais || 'N/A' }}</td>
                <td>{{ ligne.montant || 'N/A' }}</td>
                <td>
                  <ul class="mb-0">
                    <li v-for="(erreur, index) in ligne.erreurs" :key="index" class="text-danger">
                      {{ erreur }}
                    </li>
                  </ul>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Paiements créés (optionnel, pour debug) -->
      <div v-if="result.paiementsCrees.length > 0 && showDetails" class="mt-4">
        <h4>Paiements créés ({{ result.paiementsCrees.length }}) :</h4>
        <div class="table-responsive">
          <table class="table table-sm table-striped">
            <thead>
              <tr>
                <th>ID</th>
                <th>Date</th>
                <th>Montant</th>
                <th>Devise</th>
                <th>Mode</th>
                <th>Statut</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="paiement in result.paiementsCrees" :key="paiement.idPaiement">
                <td>{{ paiement.idPaiement }}</td>
                <td>{{ formatDate(paiement.datePaiement) }}</td>
                <td>{{ paiement.montant }}</td>
                <td>{{ paiement.devise }}</td>
                <td>{{ paiement.modePaiement }}</td>
                <td>{{ paiement.statutPaiement }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Message d'erreur global -->
    <div v-if="errorMessage" class="alert alert-danger mt-4">
      <strong>Erreur :</strong> {{ errorMessage }}
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue';
import axios from 'axios';

export default {
  name: 'BulkPaiementUpload',
  setup() {
    const selectedFile = ref(null);
    const isUploading = ref(false);
    const isDownloading = ref(false);
    const result = ref(null);
    const errorMessage = ref(null);
    const showDetails = ref(false);

    // Récupérer le token depuis localStorage
    const token = localStorage.getItem('token');

    const canUpload = computed(() => {
      return selectedFile.value && token;
    });

    const downloadTemplate = async () => {
      isDownloading.value = true;
      errorMessage.value = null;

      try {
        const response = await axios.get('/api/Paiement/template-excel', {
          headers: {
            'Authorization': `Bearer ${token}`
          },
          responseType: 'blob'
        });

        // Créer un lien de téléchargement
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Template_Paiements_${new Date().toISOString().split('T')[0]}.xlsx`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Erreur lors du téléchargement du template:', error);
        errorMessage.value = error.response?.data?.message || 'Erreur lors du téléchargement du template';
      } finally {
        isDownloading.value = false;
      }
    };

    const onFileSelect = (event) => {
      const file = event.target.files[0];
      if (file) {
        // Validation côté client
        if (!file.name.endsWith('.xlsx')) {
          errorMessage.value = 'Le fichier doit être au format Excel (.xlsx)';
          selectedFile.value = null;
          return;
        }

        const maxSize = 10 * 1024 * 1024; // 10 MB
        if (file.size > maxSize) {
          errorMessage.value = 'Le fichier est trop volumineux (maximum 10 MB)';
          selectedFile.value = null;
          return;
        }

        selectedFile.value = file;
        errorMessage.value = null;
        result.value = null;
      }
    };

    const uploadFile = async () => {
      if (!canUpload.value) return;

      isUploading.value = true;
      errorMessage.value = null;
      result.value = null;

      try {
        const formData = new FormData();
        formData.append('file', selectedFile.value);

        const response = await axios.post(
          '/api/Paiement/bulk-excel',
          formData,
          {
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'multipart/form-data'
            }
          }
        );

        result.value = response.data;

        // Afficher un message de succès/alerte selon le résultat
        if (result.value.success && result.value.lignesReussies === result.value.totalLignes) {
          // Toutes les lignes ont réussi
          console.log('✅ Tous les paiements ont été créés avec succès !');
        } else if (result.value.lignesReussies > 0) {
          // Traitement partiel
          console.warn(`⚠️ ${result.value.lignesReussies} paiement(s) créé(s) sur ${result.value.totalLignes}`);
        } else {
          // Aucun paiement créé
          console.error('❌ Aucun paiement n\'a été créé. Veuillez vérifier les erreurs.');
        }
      } catch (error) {
        console.error('Erreur lors de l\'upload:', error);
        
        if (error.response) {
          const status = error.response.status;
          const data = error.response.data;

          switch (status) {
            case 400:
              errorMessage.value = `Erreur de validation : ${data.message || 'Données invalides'}`;
              break;
            case 401:
              errorMessage.value = 'Session expirée. Veuillez vous reconnecter.';
              // Rediriger vers la page de connexion
              // router.push('/login');
              break;
            case 403:
              errorMessage.value = 'Vous n\'avez pas les permissions nécessaires.';
              break;
            case 500:
              errorMessage.value = 'Erreur serveur. Veuillez réessayer plus tard.';
              break;
            default:
              errorMessage.value = data.message || 'Erreur inconnue';
          }
        } else {
          errorMessage.value = 'Erreur de connexion. Vérifiez votre connexion internet.';
        }
      } finally {
        isUploading.value = false;
      }
    };

    const getAlertClass = () => {
      if (!result.value) return '';
      
      if (result.value.lignesReussies === result.value.totalLignes) {
        return 'alert-success';
      } else if (result.value.lignesReussies > 0) {
        return 'alert-warning';
      } else {
        return 'alert-danger';
      }
    };

    const formatDate = (dateString) => {
      if (!dateString) return 'N/A';
      const date = new Date(dateString);
      return date.toLocaleDateString('fr-FR');
    };

    return {
      selectedFile,
      isUploading,
      isDownloading,
      result,
      errorMessage,
      showDetails,
      canUpload,
      downloadTemplate,
      onFileSelect,
      uploadFile,
      getAlertClass,
      formatDate
    };
  }
};
</script>

<style scoped>
.bulk-paiement-upload {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.form-group {
  margin-bottom: 1rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0056b3;
}

.btn-primary:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #545b62;
}

.btn-secondary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.alert {
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.alert-success {
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  color: #155724;
}

.alert-warning {
  background-color: #fff3cd;
  border: 1px solid #ffeaa7;
  color: #856404;
}

.alert-danger {
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  color: #721c24;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th,
.table td {
  padding: 0.75rem;
  border: 1px solid #dee2e6;
}

.table thead th {
  background-color: #f8f9fa;
  font-weight: bold;
}

.table-striped tbody tr:nth-of-type(odd) {
  background-color: rgba(0, 0, 0, 0.05);
}
</style>
```

---

## 🔄 Flux de Traitement

```
1. Frontend : L'utilisateur télécharge le template Excel (optionnel)
2. Frontend : L'utilisateur remplit le template avec les données de paiement
3. Frontend : L'utilisateur sélectionne le fichier Excel rempli
4. Frontend : Validation côté client (format, taille)
5. Frontend → Backend : POST /api/Paiement/bulk-excel avec FormData (token JWT dans les headers)
6. Backend : Extraction de l'ID utilisateur et de l'ID école depuis le token JWT
7. Backend : Validation du fichier (format, taille, colonnes)
8. Backend : Chargement de tous les élèves et frais de l'école en mémoire
8. Backend : Lecture du fichier Excel ligne par ligne
9. Backend : Conversion des noms d'élèves et libellés de frais en IDs
10. Backend : Validation de chaque ligne (élève existe, frais existe, montant > 0)
11. Backend : Détection des doublons (dans le fichier et en base)
12. Backend : Traitement par lots de 50 paiements
13. Backend : Création des paiements dans la base de données
14. Backend → Frontend : Retour du résultat avec statistiques et erreurs
15. Frontend : Affichage des résultats à l'utilisateur
```

---

## 💡 Bonnes Pratiques

### 1. Validation Côté Frontend

- ✅ Vérifier le format du fichier (`.xlsx` uniquement)
- ✅ Vérifier la taille du fichier (maximum 10 MB)
- ✅ S'assurer que le token JWT est valide et présent

### 2. Feedback Utilisateur

- ✅ Afficher une barre de progression pendant le traitement
- ✅ Afficher un message de succès avec le nombre de paiements créés
- ✅ Afficher les erreurs de manière claire et actionnable
- ✅ Permettre le téléchargement d'un rapport d'erreurs

### 3. Gestion des Fichiers Volumineux

- Le traitement se fait par lots de 50 paiements
- Pour des fichiers très volumineux (> 1000 lignes), informer l'utilisateur que le traitement peut prendre plusieurs minutes
- Ne pas fermer la page pendant le traitement

### 4. Correspondance des Noms

- ⚠️ **Important** : Les noms d'élèves et libellés de frais doivent correspondre **exactement** (insensible à la casse) aux valeurs en base de données
- Le système normalise les noms (suppression des espaces en début/fin, insensible à la casse)
- En cas d'erreur "Élève non trouvé" ou "Frais non trouvé", vérifier l'orthographe exacte dans le système

---

## 📞 Support

Pour toute question ou problème :
- Vérifier les logs serveur pour les erreurs détaillées
- S'assurer que le token JWT est valide
- Vérifier que les noms d'élèves et libellés de frais correspondent exactement à ceux en base de données
- Contacter l'équipe de développement avec les détails de l'erreur

---

**Dernière mise à jour :** 2024-01-15  
**Version de l'API :** 2.0 (Format simplifié)

