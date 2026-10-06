# Integration frontend - HoraireDeCours

Ce document décrit l'intégration HTTP de la gestion des horaires de cours pour
les applications Vue.js et Flutter. Il ne dépend d'aucune librairie cliente
particulière et peut être utilisé avec `fetch`, Axios ou `package:http`.

## 1. Pré-requis

Définir l'URL de l'API dans la configuration de l'application :

```text
https://api.example.com
```

Toutes les routes utilisent un token JWT :

```http
Authorization: Bearer <token-jwt>
```

Le token doit contenir l'identité de l'utilisateur. Les permissions sont
contrôlées par l'API :

| Opération | Permission |
|---|---|
| Consulter | `HoraireDeCours.Read` |
| Créer | `HoraireDeCours.Create` |
| Modifier | `HoraireDeCours.Update` |
| Supprimer logiquement | `HoraireDeCours.Delete` |

Les rôles actuellement alimentés par le seeder sont :

- `Super-Admin`, `Admin`, `Directeur` : lecture et écriture ;
- `Enseignant`, `Eleve`, `Tuteur` : lecture.

## 2. Format des horaires

Les heures sont toujours transmises sous forme de texte `HH:mm` :

```json
{
  "heureDebut": "08:00",
  "heureFin": "09:00"
}
```

Les valeurs suivantes sont invalides :

- `"8:00"` ;
- `"08:00:00"` ;
- une heure supérieure à `23:59` ;
- une heure de fin inférieure ou égale à l'heure de début.

Les jours disponibles sont :

| Valeur JSON | Signification |
|---|---|
| `Lundi` | 1 |
| `Mardi` | 2 |
| `Mercredi` | 3 |
| `Jeudi` | 4 |
| `Vendredi` | 5 |
| `Samedi` | 6 |

Utiliser les noms (`"Lundi"`, `"Mardi"`, etc.) dans le JSON, comme dans les
exemples ci-dessous.

## 3. Modèle de réponse

Les réponses utilisent le camelCase JSON :

```json
{
  "idHoraireDeCours": 12,
  "jour": "Lundi",
  "heureDebut": "08:00",
  "heureFin": "09:00",
  "idCours": 1,
  "nomCours": "Mathématiques",
  "idClasse": 4,
  "nomClasse": "6e A",
  "idAgent": 7,
  "nomAgent": "Jean Kabasele",
  "idAnneeScolaire": 2,
  "libelleAnneeScolaire": "2026-2027",
  "salle": "Salle 203",
  "statut": true
}
```

Les champs `nomCours`, `nomClasse`, `nomAgent` et
`libelleAnneeScolaire` peuvent être `null`. Ne pas utiliser ces champs pour
identifier une ressource : utiliser les champs `id*`.

## 4. Consulter les horaires

### 4.1 Liste non paginée

```http
GET /api/HoraireDeCours
```

Filtres optionnels :

| Paramètre | Type | Description |
|---|---|---|
| `idClasse` | entier | Limite à une classe |
| `idAgent` | entier | Limite à un enseignant/agent |
| `idAnneeScolaire` | entier | Limite à une année scolaire |
| `idEcole` | entier | Limite à une école autorisée |

Exemple :

```http
GET /api/HoraireDeCours?idClasse=4&idAnneeScolaire=2
Authorization: Bearer <token-jwt>
```

La réponse est un tableau de `HoraireDeCoursResponse` :

```json
[
  {
    "idHoraireDeCours": 12,
    "jour": "Lundi",
    "heureDebut": "08:00",
    "heureFin": "09:00",
    "idCours": 1,
    "nomCours": "Mathématiques",
    "idClasse": 4,
    "nomClasse": "6e A",
    "idAgent": 7,
    "nomAgent": "Jean Kabasele",
    "idAnneeScolaire": 2,
    "libelleAnneeScolaire": "2026-2027",
    "salle": "Salle 203",
    "statut": true
  }
]
```

### 4.2 Liste paginée

```http
GET /api/HoraireDeCours/paged
```

Les filtres métier sont identiques au endpoint non paginé. Ajouter :

| Paramètre | Type | Défaut | Limite |
|---|---|---:|---:|
| `PageNumber` | entier | 1 | minimum 1 |
| `PageSize` | entier | 15 | de 1 à 100 |

`SortBy`, `SortDescending`, `SearchTerm` et `IncludeInactive` existent dans le
DTO générique de pagination, mais le service `HoraireDeCours` applique son tri
déterministe par jour, heure de début puis identifiant. Les clients doivent
donc utiliser principalement `PageNumber` et `PageSize`.

Exemple :

```http
GET /api/HoraireDeCours/paged?idEcole=1&idAnneeScolaire=2&PageNumber=1&PageSize=15
Authorization: Bearer <token-jwt>
```

Réponse :

```json
{
  "data": [
    {
      "idHoraireDeCours": 12,
      "jour": "Lundi",
      "heureDebut": "08:00",
      "heureFin": "09:00",
      "idCours": 1,
      "nomCours": "Mathématiques",
      "idClasse": 4,
      "nomClasse": "6e A",
      "idAgent": 7,
      "nomAgent": "Jean Kabasele",
      "idAnneeScolaire": 2,
      "libelleAnneeScolaire": "2026-2027",
      "salle": "Salle 203",
      "statut": true
    }
  ],
  "pageNumber": 1,
  "pageSize": 15,
  "totalPages": 1,
  "totalRecords": 1,
  "hasPrevious": false,
  "hasNext": false,
  "firstRowOnPage": 1,
  "lastRowOnPage": 1
}
```

### 4.3 Consulter une ligne

```http
GET /api/HoraireDeCours/{idHoraireDeCours}
Authorization: Bearer <token-jwt>
```

Exemple :

```http
GET /api/HoraireDeCours/12
Authorization: Bearer <token-jwt>
```

## 5. Règles d'accès école et tuteur

Pour un utilisateur non tuteur, `idEcole` est facultatif :

- sans `idEcole`, l'API utilise l'école présente dans le token ;
- avec `idEcole`, l'école demandée doit être celle autorisée par le token ;
- un administrateur global peut fournir explicitement une école.

Pour un tuteur sans rôle administratif prioritaire :

- sans `idEcole`, plusieurs écoles peuvent être retournées ;
- avec `idEcole`, l'école doit être accessible via l'inscription d'un de ses
  enfants ;
- seules les classes dans lesquelles ses enfants ont une inscription
  active/confirmée sont retournées ;
- l'année scolaire de l'inscription doit correspondre à celle de l'horaire.

Le frontend ne doit donc pas supposer qu'un tuteur possède une école unique.
Il doit afficher les résultats par école ou par classe lorsque plusieurs
écoles sont concernées.

Si un utilisateur possède également l'un des rôles `Admin`, `Directeur`,
`Sous-Directeur`, `Super-Admin` ou `IT-Support`, le scope administratif prévaut
sur le scope tuteur pour cet endpoint : les horaires ne sont pas limités aux
inscriptions de ses enfants. Les rôles d'administration d'école restent
toutefois limités à l'école du token; seuls `Super-Admin` et `IT-Support`
peuvent accéder à une école globale. Le rôle `Enseignant` seul ne prévaut pas
sur le scope tuteur.

## 6. Créer une ligne

```http
POST /api/HoraireDeCours
Authorization: Bearer <token-jwt>
Content-Type: application/json
```

Corps :

```json
{
  "jour": "Lundi",
  "heureDebut": "08:00",
  "heureFin": "09:00",
  "idCours": 1,
  "idClasse": 4,
  "idAgent": 7,
  "idAnneeScolaire": 2,
  "salle": "Salle 203"
}
```

Réponse attendue : `201 Created` avec la ressource créée.

L'API peut retourner `409 Conflict` lorsqu'un créneau chevauche un horaire
existant pour la classe, l'agent ou la salle.

## 7. Modifier une ligne

```http
PUT /api/HoraireDeCours/12
Authorization: Bearer <token-jwt>
Content-Type: application/json
```

Corps :

```json
{
  "jour": "Mardi",
  "heureDebut": "09:00",
  "heureFin": "10:00",
  "idCours": 1,
  "idClasse": 4,
  "idAgent": 7,
  "idAnneeScolaire": 2,
  "salle": "Salle 203",
  "statut": true
}
```

Réponses principales :

- `200 OK` : ligne mise à jour ;
- `404 Not Found` : identifiant inexistant ;
- `400 Bad Request` : données invalides ;
- `409 Conflict` : chevauchement détecté.

## 8. Suppression logique

```http
DELETE /api/HoraireDeCours/12
Authorization: Bearer <token-jwt>
```

Réponses :

- `204 No Content` : désactivation réussie ;
- `404 Not Found` : identifiant inexistant.

Le endpoint ne supprime pas physiquement la ligne. La ligne est désactivée
(`statut = false`) et les consultations courantes retournent uniquement les
horaires actifs.

## 9. Exemples Vue.js

### Avec `fetch`

```javascript
const apiBaseUrl = import.meta.env.VITE_API_URL;

async function getHoraires(token, filters = {}) {
  const params = new URLSearchParams();

  for (const [name, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null && value !== '') {
      params.set(name, String(value));
    }
  }

  const response = await fetch(
    `${apiBaseUrl}/api/HoraireDeCours?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: 'application/json'
      }
    }
  );

  if (!response.ok) {
    throw new Error(`Erreur HTTP ${response.status}`);
  }

  return response.json();
}

const horaires = await getHoraires(token, {
  idEcole: 1,
  idAnneeScolaire: 2,
  idClasse: 4
});
```

### Avec Axios pour la pagination

```javascript
import axios from 'axios';

async function getHorairesPaged(token, filters = {}) {
  const { data } = await axios.get(
    `${import.meta.env.VITE_API_URL}/api/HoraireDeCours/paged`,
    {
      headers: { Authorization: `Bearer ${token}` },
      params: {
        ...filters,
        PageNumber: filters.pageNumber ?? 1,
        PageSize: filters.pageSize ?? 15
      }
    }
  );

  return data;
}

const page = await getHorairesPaged(token, {
  idAnneeScolaire: 2,
  pageNumber: 1,
  pageSize: 15
});
```

## 10. Exemples Flutter

### Requête avec `package:http`

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

Future<List<dynamic>> getHoraires({
  required String apiBaseUrl,
  required String token,
  int? idEcole,
  int? idAnneeScolaire,
  int? idClasse,
}) async {
  final query = <String, String>{};
  if (idEcole != null) query['idEcole'] = '$idEcole';
  if (idAnneeScolaire != null) {
    query['idAnneeScolaire'] = '$idAnneeScolaire';
  }
  if (idClasse != null) query['idClasse'] = '$idClasse';

  final uri = Uri.parse('$apiBaseUrl/api/HoraireDeCours')
      .replace(queryParameters: query);
  final response = await http.get(uri, headers: {
    'Authorization': 'Bearer $token',
    'Accept': 'application/json',
  });

  if (response.statusCode != 200) {
    throw Exception('Erreur HTTP ${response.statusCode}');
  }

  return jsonDecode(response.body) as List<dynamic>;
}
```

### Requête paginée

```dart
Future<Map<String, dynamic>> getHorairesPaged({
  required String apiBaseUrl,
  required String token,
  int pageNumber = 1,
  int pageSize = 15,
  int? idEcole,
  int? idAnneeScolaire,
}) async {
  final query = <String, String>{
    'PageNumber': '$pageNumber',
    'PageSize': '$pageSize',
  };
  if (idEcole != null) query['idEcole'] = '$idEcole';
  if (idAnneeScolaire != null) {
    query['idAnneeScolaire'] = '$idAnneeScolaire';
  }

  final uri = Uri.parse('$apiBaseUrl/api/HoraireDeCours/paged')
      .replace(queryParameters: query);
  final response = await http.get(uri, headers: {
    'Authorization': 'Bearer $token',
    'Accept': 'application/json',
  });

  if (response.statusCode != 200) {
    throw Exception('Erreur HTTP ${response.statusCode}');
  }

  return jsonDecode(response.body) as Map<String, dynamic>;
}
```

## 11. Statuts HTTP à gérer

| Statut | Signification | Comportement frontend |
|---:|---|---|
| `200` | Lecture ou modification réussie | Afficher/actualiser les données |
| `201` | Création réussie | Ajouter la ligne retournée |
| `204` | Suppression logique réussie | Retirer la ligne de la liste |
| `400` | Validation ou format `HH:mm` invalide | Afficher les erreurs du formulaire |
| `401` | Token absent ou invalide | Renouveler la session ou rediriger vers la connexion |
| `403` | Permission ou périmètre insuffisant | Afficher un accès refusé |
| `404` | Ligne inexistante | Informer l'utilisateur et actualiser la liste |
| `409` | Chevauchement métier | Afficher le conflit et demander un autre créneau |

## 12. Checklist d'intégration

- [ ] Configurer l'URL de l'API par environnement.
- [ ] Ajouter automatiquement le token JWT à chaque requête.
- [ ] Envoyer les heures au format `HH:mm`, sans secondes.
- [ ] Utiliser les noms d'énumération du jour (`Lundi` à `Samedi`).
- [ ] Ne pas envoyer `idEcole` pour forcer une école différente de celle du
      token.
- [ ] Prévoir le cas tuteur multi-écoles et grouper les résultats si nécessaire.
- [ ] Limiter `PageSize` à 100 maximum.
- [ ] Gérer explicitement `401`, `403`, `400` et `409`.
- [ ] Désactiver les boutons de création/modification/suppression lorsque le
      rôle ne possède pas la permission correspondante.
- [ ] Utiliser `idHoraireDeCours` comme identifiant stable côté client.
