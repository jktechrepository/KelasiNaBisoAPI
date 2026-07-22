# 🚀 Guide du Développeur Frontend - API KelasiNaBiso

## 📋 Table des matières

1. [Prérequis](#prérequis)
2. [Configuration initiale](#configuration-initiale)
3. [Authentification](#authentification)
4. [Exemples d'intégration](#exemples-dintégration)
5. [Gestion des erreurs](#gestion-des-erreurs)
6. [Bonnes pratiques](#bonnes-pratiques)
7. [FAQ](#faq)

---

## 🔧 Prérequis

### Outils nécessaires
- **Postman** ou **Insomnia** pour tester les API
- **Node.js** et **npm** (pour les projets JavaScript/TypeScript)
- **Git** pour la gestion de version

### Connaissances requises
- Maîtrise de JavaScript/TypeScript
- Connaissance des requêtes HTTP (GET, POST, PUT, DELETE)
- Compréhension du format JSON
- Notions de gestion d'état (Redux, Context API, etc.)

---

## ⚙️ Configuration initiale

### 1. Import de la collection Postman

1. Téléchargez le fichier `KelasiNaBiso_API_Collection.postman_collection.json`
2. Ouvrez Postman
3. Cliquez sur "Import" et sélectionnez le fichier
4. La collection sera importée avec toutes les requêtes organisées par catégories

### 2. Configuration des variables d'environnement

Dans Postman, créez un environnement avec ces variables :

```json
{
  "baseUrl": "https://localhost:7155",
  "httpBaseUrl": "http://localhost:5002",
  "authToken": "",
  "userId": "",
  "ecoleId": ""
}
```

### 3. Configuration pour votre projet frontend

#### Variables d'environnement (.env)
```env
REACT_APP_API_BASE_URL=https://localhost:7155
REACT_APP_API_HTTP_URL=http://localhost:5002
```

#### Configuration Axios (si utilisé)
```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercepteur pour ajouter le token d'authentification
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

---

## 🔐 Authentification

### Flux d'authentification recommandé

```javascript
// 1. Authentification
const authenticateUser = async (emailOrPhone, password) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Utilisateur/authentifier`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        emailOuTelephone: emailOrPhone,
        motDePasse: password,
      }),
    });

    if (!response.ok) {
      throw new Error('Authentification échouée');
    }

    const userData = await response.json();
    
    // Stockage des données utilisateur
    localStorage.setItem('authToken', userData.referenceUtilisateur);
    localStorage.setItem('userData', JSON.stringify(userData));
    
    return userData;
  } catch (error) {
    console.error('Erreur d\'authentification:', error);
    throw error;
  }
};

// 2. Vérification de l'état d'authentification
const checkAuthStatus = () => {
  const token = localStorage.getItem('authToken');
  const userData = localStorage.getItem('userData');
  
  if (token && userData) {
    return JSON.parse(userData);
  }
  
  return null;
};

// 3. Déconnexion
const logout = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('userData');
  // Redirection vers la page de connexion
};
```

---

## 💻 Exemples d'intégration

### 1. Gestion des utilisateurs

```javascript
// Récupération de tous les utilisateurs
const getUsers = async () => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Utilisateur`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des utilisateurs:', error);
    throw error;
  }
};

// Création d'un utilisateur
const createUser = async (userData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Utilisateur`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });
    
    if (!response.ok) {
      throw new Error('Erreur lors de la création de l\'utilisateur');
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la création:', error);
    throw error;
  }
};
```

### 2. Gestion des écoles

```javascript
// Récupération des écoles
const getSchools = async () => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Ecole`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des écoles:', error);
    throw error;
  }
};

// Récupération des classes d'une école
const getSchoolClasses = async (schoolId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Ecole/${schoolId}/classes`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des classes:', error);
    throw error;
  }
};
```

### 3. Gestion des élèves

```javascript
// Récupération des élèves (vue complète)
const getStudents = async () => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/V_Eleve`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des élèves:', error);
    throw error;
  }
};

// Récupération des élèves d'une classe
const getStudentsByClass = async (classId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Eleve/classe/${classId}`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des élèves:', error);
    throw error;
  }
};

// Création d'un élève
const createStudent = async (studentData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Eleve`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(studentData),
    });
    
    if (!response.ok) {
      throw new Error('Erreur lors de la création de l\'élève');
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la création:', error);
    throw error;
  }
};
```

### 4. Gestion des notes

```javascript
// Récupération des notes d'un élève
const getStudentGrades = async (studentId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Eleve/${studentId}/notes`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des notes:', error);
    throw error;
  }
};

// Ajout d'une note
const addGrade = async (gradeData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Note`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(gradeData),
    });
    
    if (!response.ok) {
      throw new Error('Erreur lors de l\'ajout de la note');
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de l\'ajout:', error);
    throw error;
  }
};
```

### 5. Gestion des présences

```javascript
// Récupération des présences d'une classe
const getClassAttendance = async (classId) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Presence/classe/${classId}`);
    return await response.json();
  } catch (error) {
    console.error('Erreur lors de la récupération des présences:', error);
    throw error;
  }
};

// Marquer une présence
const markAttendance = async (attendanceData) => {
  try {
    const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Presence`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(attendanceData),
    });
    
    if (!response.ok) {
      throw new Error('Erreur lors du marquage de la présence');
    }
    
    return await response.json();
  } catch (error) {
    console.error('Erreur lors du marquage:', error);
    throw error;
  }
};
```

---

## ⚠️ Gestion des erreurs

### Hook personnalisé pour la gestion des erreurs

```javascript
import { useState, useCallback } from 'react';

export const useApiError = () => {
  const [error, setError] = useState(null);

  const handleApiError = useCallback((error) => {
    let errorMessage = 'Une erreur inattendue s\'est produite';

    if (error.response) {
      // Erreur de réponse du serveur
      switch (error.response.status) {
        case 400:
          errorMessage = 'Données invalides';
          break;
        case 401:
          errorMessage = 'Authentification requise';
          // Redirection vers la page de connexion
          break;
        case 403:
          errorMessage = 'Accès refusé';
          break;
        case 404:
          errorMessage = 'Ressource non trouvée';
          break;
        case 500:
          errorMessage = 'Erreur serveur';
          break;
        default:
          errorMessage = `Erreur ${error.response.status}`;
      }
    } else if (error.request) {
      // Erreur de réseau
      errorMessage = 'Erreur de connexion au serveur';
    } else {
      // Autre erreur
      errorMessage = error.message;
    }

    setError(errorMessage);
    
    // Nettoyer l'erreur après 5 secondes
    setTimeout(() => setError(null), 5000);
  }, []);

  return { error, handleApiError, setError };
};
```

### Utilisation dans les composants

```javascript
import React, { useState, useEffect } from 'react';
import { useApiError } from './hooks/useApiError';

const StudentsList = () => {
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const { error, handleApiError } = useApiError();

  useEffect(() => {
    const fetchStudents = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/V_Eleve`);
        
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        setStudents(data);
      } catch (error) {
        handleApiError(error);
      } finally {
        setLoading(false);
      }
    };

    fetchStudents();
  }, [handleApiError]);

  if (loading) return <div>Chargement...</div>;
  if (error) return <div className="error">Erreur: {error}</div>;

  return (
    <div>
      <h2>Liste des élèves</h2>
      {students.map(student => (
        <div key={student.idEleve}>
          {student.nom} {student.prenom}
        </div>
      ))}
    </div>
  );
};
```

---

## ✅ Bonnes pratiques

### 1. Gestion d'état

```javascript
// Utilisation de Context API pour l'état global
import React, { createContext, useContext, useReducer } from 'react';

const AppContext = createContext();

const initialState = {
  user: null,
  school: null,
  students: [],
  classes: [],
  loading: false,
  error: null,
};

const appReducer = (state, action) => {
  switch (action.type) {
    case 'SET_USER':
      return { ...state, user: action.payload };
    case 'SET_SCHOOL':
      return { ...state, school: action.payload };
    case 'SET_STUDENTS':
      return { ...state, students: action.payload };
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    case 'SET_ERROR':
      return { ...state, error: action.payload };
    default:
      return state;
  }
};

export const AppProvider = ({ children }) => {
  const [state, dispatch] = useReducer(appReducer, initialState);

  return (
    <AppContext.Provider value={{ state, dispatch }}>
      {children}
    </AppContext.Provider>
  );
};

export const useAppContext = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useAppContext must be used within an AppProvider');
  }
  return context;
};
```

### 2. Validation des données

```javascript
// Validation des données avant envoi
const validateStudentData = (data) => {
  const errors = {};

  if (!data.nom || data.nom.trim().length < 2) {
    errors.nom = 'Le nom doit contenir au moins 2 caractères';
  }

  if (!data.prenom || data.prenom.trim().length < 2) {
    errors.prenom = 'Le prénom doit contenir au moins 2 caractères';
  }

  if (!data.dateNaissance) {
    errors.dateNaissance = 'La date de naissance est requise';
  }

  if (!data.idClasse) {
    errors.idClasse = 'La classe est requise';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};
```

### 3. Optimisation des performances

```javascript
// Mise en cache des données
const useCachedData = (key, fetchFunction, dependencies = []) => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const cachedData = localStorage.getItem(key);
    
    if (cachedData) {
      setData(JSON.parse(cachedData));
      setLoading(false);
    } else {
      fetchFunction().then(result => {
        setData(result);
        localStorage.setItem(key, JSON.stringify(result));
        setLoading(false);
      });
    }
  }, dependencies);

  return { data, loading };
};
```

---

## ❓ FAQ

### Q: Comment gérer les certificats SSL en développement ?
**R:** En développement, vous pouvez désactiver la vérification SSL ou utiliser HTTP au lieu de HTTPS.

### Q: Comment gérer les timeouts des requêtes ?
**R:** Utilisez AbortController pour annuler les requêtes en cours :

```javascript
const fetchWithTimeout = async (url, options = {}, timeout = 5000) => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeout);

  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal,
    });
    clearTimeout(timeoutId);
    return response;
  } catch (error) {
    clearTimeout(timeoutId);
    throw error;
  }
};
```

### Q: Comment gérer la pagination ?
**R:** L'API ne supporte pas encore la pagination native, mais vous pouvez l'implémenter côté client :

```javascript
const usePagination = (data, itemsPerPage = 10) => {
  const [currentPage, setCurrentPage] = useState(1);
  
  const totalPages = Math.ceil(data.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const currentData = data.slice(startIndex, endIndex);
  
  return {
    currentData,
    currentPage,
    totalPages,
    setCurrentPage,
  };
};
```

### Q: Comment gérer les mises à jour en temps réel ?
**R:** Pour l'instant, utilisez des sondages (polling) ou WebSockets si nécessaire :

```javascript
const usePolling = (fetchFunction, interval = 30000) => {
  useEffect(() => {
    const intervalId = setInterval(fetchFunction, interval);
    return () => clearInterval(intervalId);
  }, [fetchFunction, interval]);
};
```

---

## 📞 Support

Pour toute question ou problème :

1. **Consultez la documentation complète** : `API_DOCUMENTATION.md`
2. **Testez avec Postman** : Utilisez la collection fournie
3. **Vérifiez les logs** : Regardez la console du navigateur et les logs du serveur
4. **Contactez l'équipe backend** : Pour les problèmes spécifiques à l'API

---

*Guide mis à jour le: ${new Date().toLocaleDateString('fr-FR')}*
