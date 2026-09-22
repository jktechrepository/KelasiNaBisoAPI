# Intégration Vue 3 & Flutter — Feuille d'appel (élèves & agents)

Guide d'implémentation pour l'écran **feuille d'appel** côté **Vue 3** (web) et **Flutter** (tablette / mobile).

**Références :**
- [DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md) — contrôle à l'entrée (pointage, dashboard)
- [DOCUMENTATION_FRONTEND_VITRINE_VUE.md](DOCUMENTATION_FRONTEND_VITRINE_VUE.md) — conventions client API Vue

**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Auth :** `Authorization: Bearer {jwt_token}`  
**Contrôle école :** `ForbidIfWrongSchool` sur le claim JWT `idEcole`

**Hors périmètre :** pointage (`POST /api/Presence`), dashboard présence, correction (`PUT /api/Presence`).

---

## Table des matières

1. [Prérequis](#1-prérequis)
2. [Endpoints](#2-endpoints)
3. [Contrats JSON](#3-contrats-json)
4. [Export binaire (xlsx / pdf)](#4-export-binaire-xlsx--pdf)
5. [UX recommandée](#5-ux-recommandée)
6. [Vue 3 — types et client API](#6-vue-3--types-et-client-api)
7. [Vue 3 — composable et téléchargement](#7-vue-3--composable-et-téléchargement)
8. [Flutter — modèles et service](#8-flutter--modèles-et-service)
9. [Flutter — Provider et ouverture fichier](#9-flutter--provider-et-ouverture-fichier)
10. [Gestion des erreurs](#10-gestion-des-erreurs)
11. [Checklist d'intégration](#11-checklist-dintégration)

---

## 1. Prérequis

### JWT — claims utiles

| Claim | Usage front |
|-------|-------------|
| `idEcole` | Obligatoire — filtrage école (403 si mauvaise école) |
| `role` | `Controleur`, `Directeur`, `Prefet`, `Admin`, `Super-Admin` |

**Lecture seule :** la feuille d'appel ne nécessite pas `Presence.Create`. Tout rôle autorisé à consulter le contrôle d'entrée peut charger et exporter.

### Routes suggérées

| Écran | Vue Router | GoRouter |
|-------|------------|----------|
| Feuille élèves / agents | `/controle-entree/feuille-appel` | `controle-entree/feuille-appel` |

---

## 2. Endpoints

| Méthode | Route | Réponse | Query |
|---------|-------|---------|-------|
| `GET` | `/api/Presence/eleves/classe/{idClasse}/feuille-appel` | JSON | `date` (optionnel, défaut = aujourd'hui), `idAnneeScolaire` (optionnel) |
| `GET` | `/api/Presence/eleves/classe/{idClasse}/feuille-appel/export` | Fichier | `date`, `idAnneeScolaire`, **`format=xlsx\|pdf`** (défaut `xlsx`) |
| `GET` | `/api/Presence/agents/ecole/{idEcole}/feuille-appel` | JSON | `date` (optionnel), `fonction` (optionnel) |
| `GET` | `/api/Presence/agents/ecole/{idEcole}/feuille-appel/export` | Fichier | `date`, `fonction`, **`format=xlsx\|pdf`** |

### Exemples

```http
GET /api/Presence/eleves/classe/42/feuille-appel?date=2026-09-21
GET /api/Presence/eleves/classe/42/feuille-appel/export?date=2026-09-21&format=pdf
GET /api/Presence/agents/ecole/13/feuille-appel?date=2026-09-21&fonction=Enseignant
GET /api/Presence/agents/ecole/13/feuille-appel/export?format=xlsx
```

**Périmètre métier :**

- **Élèves** : inscrits de la classe (année scolaire courante ou `idAnneeScolaire`).
- **Agents** : agents actifs (`Statut == true`) de l'école ; filtre optionnel `fonction` (correspondance exacte côté API).

---

## 3. Contrats JSON

### Élèves — `FeuilleAppelClasseDto`

```json
{
  "idClasse": 42,
  "nomClasse": "6e A",
  "idEcole": 13,
  "nomEcole": "École Exemple",
  "idAnneeScolaire": 5,
  "date": "2026-09-21T00:00:00",
  "effectif": 30,
  "nbPresents": 25,
  "nbAbsents": 3,
  "nbRetards": 2,
  "lignes": [
    {
      "idEleve": 101,
      "matricule": "E001",
      "nomComplet": "Kabongo Jean",
      "genre": "M",
      "statutJour": "Present",
      "idPresence": 9001,
      "isPresent": true,
      "heureArrivee": "07:45:00",
      "heureDepart": null,
      "observation": null
    }
  ]
}
```

### Agents — `FeuilleAppelAgentsDto`

```json
{
  "idEcole": 13,
  "nomEcole": "École Exemple",
  "fonctionFiltre": "Enseignant",
  "date": "2026-09-21T00:00:00",
  "effectif": 12,
  "nbPresents": 10,
  "nbAbsents": 1,
  "nbRetards": 1,
  "lignes": [
    {
      "idAgent": 7,
      "matricule": "A012",
      "nomComplet": "Mwamba Claire",
      "fonction": "Enseignant",
      "genre": "F",
      "statutJour": "Retard",
      "idPresence": 9100,
      "isPresent": true,
      "heureArrivee": "08:25:00",
      "heureDepart": null,
      "observation": null
    }
  ]
}
```

### `statutJour`

| Valeur | Signification |
|--------|----------------|
| `Present` | Présent, arrivée ≤ **08:00** |
| `Retard` | Présent, arrivée **> 08:00** |
| `Absent` | Pas de présence du jour (ou non présent) |

Utiliser ces chaînes **exactement** (casse sensible) pour pastilles / filtres UI.

---

## 4. Export binaire (xlsx / pdf)

| `format` | Content-Type | Extension fichier (serveur) |
|----------|--------------|-----------------------------|
| `xlsx` (défaut) | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` | `.xlsx` |
| `pdf` | `application/pdf` | `.pdf` |

**Noms typiques :**

- Élèves : `FeuilleAppel_{Classe}_{yyyyMMdd}.xlsx|.pdf`
- Agents : `FeuilleAppel_Agents_{Ecole}[_{Fonction}]_{yyyyMMdd}.xlsx|.pdf`

**Front obligatoire :**

- Vue / Axios : `responseType: 'blob'` (ou `fetch` → `res.blob()`).
- Flutter / Dio : `ResponseType.bytes` puis écriture fichier local.

Ne pas parser le corps comme JSON quand `format=xlsx` ou `format=pdf`.

---

## 5. UX recommandée

```
┌─────────────────────────────────────────────┐
│  Feuille d'appel          [ date ]          │
│  [ Élèves ]  [ Agents ]                     │
├─────────────────────────────────────────────┤
│  Élèves : [ Classe ▼ ]                      │
│  Agents : [ Fonction (optionnel) ]          │
│  Totaux : Effectif · Présents · Absents · Retards │
│  [ Excel ]  [ PDF ]                         │
├─────────────────────────────────────────────┤
│  #  Matricule  Nom  …  Statut  Arrivée      │
│  1  E001       …    ● Present  07:45        │
└─────────────────────────────────────────────┘
```

- Pastilles : vert `Present`, orange `Retard`, rouge `Absent`.
- Recharger la liste au changement de date / classe / fonction.
- Export sans recharger la grille (même query `date` / filtres).

---

## 6. Vue 3 — types et client API

### Types — `src/types/feuilleAppel.ts`

```typescript
export type FeuilleAppelStatutJour = 'Present' | 'Absent' | 'Retard';

export interface FeuilleAppelLigneDto {
  idEleve: number;
  matricule?: string | null;
  nomComplet: string;
  genre?: string | null;
  statutJour: FeuilleAppelStatutJour;
  idPresence?: number | null;
  isPresent?: boolean | null;
  heureArrivee?: string | null;
  heureDepart?: string | null;
  observation?: string | null;
}

export interface FeuilleAppelClasseDto {
  idClasse: number;
  nomClasse: string;
  idEcole: number;
  nomEcole?: string | null;
  idAnneeScolaire: number;
  date: string;
  effectif: number;
  nbPresents: number;
  nbAbsents: number;
  nbRetards: number;
  lignes: FeuilleAppelLigneDto[];
}

export interface FeuilleAppelAgentLigneDto {
  idAgent: number;
  matricule?: string | null;
  nomComplet: string;
  fonction?: string | null;
  genre?: string | null;
  statutJour: FeuilleAppelStatutJour;
  idPresence?: number | null;
  isPresent?: boolean | null;
  heureArrivee?: string | null;
  heureDepart?: string | null;
  observation?: string | null;
}

export interface FeuilleAppelAgentsDto {
  idEcole: number;
  nomEcole?: string | null;
  fonctionFiltre?: string | null;
  date: string;
  effectif: number;
  nbPresents: number;
  nbAbsents: number;
  nbRetards: number;
  lignes: FeuilleAppelAgentLigneDto[];
}

export type FeuilleAppelExportFormat = 'xlsx' | 'pdf';
```

### Client — `src/api/feuilleAppel.js`

Réutiliser le même `apiFetch` / `buildQuery` que le guide Controleur.

```javascript
/** @returns {Promise<import('../types/feuilleAppel').FeuilleAppelClasseDto>} */
export function fetchFeuilleAppelEleves(idClasse, { date, idAnneeScolaire } = {}) {
  const qs = buildQuery({ date, idAnneeScolaire });
  const suffix = qs ? `?${qs}` : '';
  return apiFetch(`/api/Presence/eleves/classe/${idClasse}/feuille-appel${suffix}`);
}

/** @returns {Promise<import('../types/feuilleAppel').FeuilleAppelAgentsDto>} */
export function fetchFeuilleAppelAgents(idEcole, { date, fonction } = {}) {
  const qs = buildQuery({ date, fonction });
  const suffix = qs ? `?${qs}` : '';
  return apiFetch(`/api/Presence/agents/ecole/${idEcole}/feuille-appel${suffix}`);
}

/**
 * @param {'eleves'|'agents'} kind
 * @param {number} id — idClasse ou idEcole
 * @param {{ date?: string, idAnneeScolaire?: number, fonction?: string, format?: 'xlsx'|'pdf' }} opts
 * @returns {Promise<Blob>}
 */
export async function downloadFeuilleAppelExport(kind, id, opts = {}) {
  const { date, idAnneeScolaire, fonction, format = 'xlsx' } = opts;
  const auth = (await import('@/stores/auth')).useAuthStore();
  const base = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || '';

  const path =
    kind === 'eleves'
      ? `/api/Presence/eleves/classe/${id}/feuille-appel/export`
      : `/api/Presence/agents/ecole/${id}/feuille-appel/export`;

  const qs = buildQuery(
    kind === 'eleves'
      ? { date, idAnneeScolaire, format }
      : { date, fonction, format }
  );

  const res = await fetch(`${base}${path}?${qs}`, {
    headers: { Authorization: `Bearer ${auth.token}` }
  });

  if (!res.ok) {
    let message = `Export échoué (${res.status})`;
    try {
      const err = await res.json();
      if (err?.message) message = err.message;
    } catch { /* corps binaire ou vide */ }
    throw new Error(message);
  }

  return res.blob();
}

/** Déclenche le téléchargement navigateur */
export function triggerBlobDownload(blob, fileName) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();
  URL.revokeObjectURL(url);
}
```

---

## 7. Vue 3 — composable et téléchargement

`src/composables/useFeuilleAppel.js`

```javascript
import { ref, watch } from 'vue';
import {
  fetchFeuilleAppelEleves,
  fetchFeuilleAppelAgents,
  downloadFeuilleAppelExport,
  triggerBlobDownload
} from '@/api/feuilleAppel';
import { formatDateDuJour } from '@/utils/controleEntree';

export function useFeuilleAppel({ idEcole }) {
  const tab = ref('eleves'); // 'eleves' | 'agents'
  const date = ref(formatDateDuJour());
  const idClasse = ref(null);
  const fonction = ref('');
  const loading = ref(false);
  const error = ref(null);
  const feuilleEleves = ref(null);
  const feuilleAgents = ref(null);

  async function load() {
    loading.value = true;
    error.value = null;
    try {
      if (tab.value === 'eleves') {
        if (!idClasse.value) {
          feuilleEleves.value = null;
          return;
        }
        feuilleEleves.value = await fetchFeuilleAppelEleves(idClasse.value, {
          date: date.value
        });
      } else {
        feuilleAgents.value = await fetchFeuilleAppelAgents(idEcole, {
          date: date.value,
          fonction: fonction.value || undefined
        });
      }
    } catch (e) {
      error.value = e.message ?? 'Erreur chargement feuille';
    } finally {
      loading.value = false;
    }
  }

  async function exportFile(format) {
    const kind = tab.value;
    const id = kind === 'eleves' ? idClasse.value : idEcole;
    if (!id) return;

    const blob = await downloadFeuilleAppelExport(kind, id, {
      date: date.value,
      fonction: fonction.value || undefined,
      format
    });

    const ext = format === 'pdf' ? 'pdf' : 'xlsx';
    const stamp = date.value.replace(/-/g, '');
    const name =
      kind === 'eleves'
        ? `FeuilleAppel_classe${id}_${stamp}.${ext}`
        : `FeuilleAppel_Agents_ecole${id}_${stamp}.${ext}`;

    triggerBlobDownload(blob, name);
  }

  watch([tab, date, idClasse, fonction], load, { immediate: true });

  return {
    tab,
    date,
    idClasse,
    fonction,
    loading,
    error,
    feuilleEleves,
    feuilleAgents,
    load,
    exportFile
  };
}
```

Exemple boutons :

```vue
<button type="button" :disabled="loading" @click="exportFile('xlsx')">Excel</button>
<button type="button" :disabled="loading" @click="exportFile('pdf')">PDF</button>
```

---

## 8. Flutter — modèles et service

### Modèles — `lib/features/feuille_appel/models/feuille_appel_models.dart`

```dart
class FeuilleAppelClasseDto {
  FeuilleAppelClasseDto({
    required this.idClasse,
    required this.nomClasse,
    required this.idEcole,
    this.nomEcole,
    required this.idAnneeScolaire,
    required this.date,
    required this.effectif,
    required this.nbPresents,
    required this.nbAbsents,
    required this.nbRetards,
    required this.lignes,
  });

  final int idClasse;
  final String nomClasse;
  final int idEcole;
  final String? nomEcole;
  final int idAnneeScolaire;
  final DateTime date;
  final int effectif;
  final int nbPresents;
  final int nbAbsents;
  final int nbRetards;
  final List<FeuilleAppelLigneDto> lignes;

  factory FeuilleAppelClasseDto.fromJson(Map<String, dynamic> json) {
    return FeuilleAppelClasseDto(
      idClasse: json['idClasse'] as int,
      nomClasse: json['nomClasse'] as String? ?? '',
      idEcole: json['idEcole'] as int,
      nomEcole: json['nomEcole'] as String?,
      idAnneeScolaire: json['idAnneeScolaire'] as int,
      date: DateTime.parse(json['date'] as String),
      effectif: json['effectif'] as int? ?? 0,
      nbPresents: json['nbPresents'] as int? ?? 0,
      nbAbsents: json['nbAbsents'] as int? ?? 0,
      nbRetards: json['nbRetards'] as int? ?? 0,
      lignes: (json['lignes'] as List<dynamic>? ?? [])
          .map((e) => FeuilleAppelLigneDto.fromJson(Map<String, dynamic>.from(e as Map)))
          .toList(),
    );
  }
}

class FeuilleAppelLigneDto {
  FeuilleAppelLigneDto({
    required this.idEleve,
    this.matricule,
    required this.nomComplet,
    this.genre,
    required this.statutJour,
    this.idPresence,
    this.isPresent,
    this.heureArrivee,
    this.heureDepart,
    this.observation,
  });

  final int idEleve;
  final String? matricule;
  final String nomComplet;
  final String? genre;
  final String statutJour;
  final int? idPresence;
  final bool? isPresent;
  final String? heureArrivee;
  final String? heureDepart;
  final String? observation;

  factory FeuilleAppelLigneDto.fromJson(Map<String, dynamic> json) {
    return FeuilleAppelLigneDto(
      idEleve: json['idEleve'] as int,
      matricule: json['matricule'] as String?,
      nomComplet: json['nomComplet'] as String? ?? '',
      genre: json['genre'] as String?,
      statutJour: json['statutJour'] as String? ?? 'Absent',
      idPresence: json['idPresence'] as int?,
      isPresent: json['isPresent'] as bool?,
      heureArrivee: json['heureArrivee']?.toString(),
      heureDepart: json['heureDepart']?.toString(),
      observation: json['observation'] as String?,
    );
  }
}

class FeuilleAppelAgentsDto {
  FeuilleAppelAgentsDto({
    required this.idEcole,
    this.nomEcole,
    this.fonctionFiltre,
    required this.date,
    required this.effectif,
    required this.nbPresents,
    required this.nbAbsents,
    required this.nbRetards,
    required this.lignes,
  });

  final int idEcole;
  final String? nomEcole;
  final String? fonctionFiltre;
  final DateTime date;
  final int effectif;
  final int nbPresents;
  final int nbAbsents;
  final int nbRetards;
  final List<FeuilleAppelAgentLigneDto> lignes;

  factory FeuilleAppelAgentsDto.fromJson(Map<String, dynamic> json) {
    return FeuilleAppelAgentsDto(
      idEcole: json['idEcole'] as int,
      nomEcole: json['nomEcole'] as String?,
      fonctionFiltre: json['fonctionFiltre'] as String?,
      date: DateTime.parse(json['date'] as String),
      effectif: json['effectif'] as int? ?? 0,
      nbPresents: json['nbPresents'] as int? ?? 0,
      nbAbsents: json['nbAbsents'] as int? ?? 0,
      nbRetards: json['nbRetards'] as int? ?? 0,
      lignes: (json['lignes'] as List<dynamic>? ?? [])
          .map((e) => FeuilleAppelAgentLigneDto.fromJson(Map<String, dynamic>.from(e as Map)))
          .toList(),
    );
  }
}

class FeuilleAppelAgentLigneDto {
  FeuilleAppelAgentLigneDto({
    required this.idAgent,
    this.matricule,
    required this.nomComplet,
    this.fonction,
    this.genre,
    required this.statutJour,
    this.idPresence,
    this.isPresent,
    this.heureArrivee,
    this.heureDepart,
    this.observation,
  });

  final int idAgent;
  final String? matricule;
  final String nomComplet;
  final String? fonction;
  final String? genre;
  final String statutJour;
  final int? idPresence;
  final bool? isPresent;
  final String? heureArrivee;
  final String? heureDepart;
  final String? observation;

  factory FeuilleAppelAgentLigneDto.fromJson(Map<String, dynamic> json) {
    return FeuilleAppelAgentLigneDto(
      idAgent: json['idAgent'] as int,
      matricule: json['matricule'] as String?,
      nomComplet: json['nomComplet'] as String? ?? '',
      fonction: json['fonction'] as String?,
      genre: json['genre'] as String?,
      statutJour: json['statutJour'] as String? ?? 'Absent',
      idPresence: json['idPresence'] as int?,
      isPresent: json['isPresent'] as bool?,
      heureArrivee: json['heureArrivee']?.toString(),
      heureDepart: json['heureDepart']?.toString(),
      observation: json['observation'] as String?,
    );
  }
}
```

### API — `lib/features/feuille_appel/api/feuille_appel_api.dart`

```dart
import 'dart:typed_data';
import 'package:dio/dio.dart';
import '../models/feuille_appel_models.dart';

class FeuilleAppelApi {
  FeuilleAppelApi(this._dio);

  final Dio _dio;

  Future<FeuilleAppelClasseDto> fetchEleves(
    int idClasse, {
    String? date,
    int? idAnneeScolaire,
  }) async {
    final res = await _dio.get(
      '/api/Presence/eleves/classe/$idClasse/feuille-appel',
      queryParameters: {
        if (date != null) 'date': date,
        if (idAnneeScolaire != null) 'idAnneeScolaire': idAnneeScolaire,
      },
    );
    return FeuilleAppelClasseDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<FeuilleAppelAgentsDto> fetchAgents(
    int idEcole, {
    String? date,
    String? fonction,
  }) async {
    final res = await _dio.get(
      '/api/Presence/agents/ecole/$idEcole/feuille-appel',
      queryParameters: {
        if (date != null) 'date': date,
        if (fonction != null && fonction.isNotEmpty) 'fonction': fonction,
      },
    );
    return FeuilleAppelAgentsDto.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<Uint8List> exportEleves(
    int idClasse, {
    String? date,
    int? idAnneeScolaire,
    String format = 'xlsx',
  }) async {
    final res = await _dio.get<List<int>>(
      '/api/Presence/eleves/classe/$idClasse/feuille-appel/export',
      queryParameters: {
        if (date != null) 'date': date,
        if (idAnneeScolaire != null) 'idAnneeScolaire': idAnneeScolaire,
        'format': format,
      },
      options: Options(responseType: ResponseType.bytes),
    );
    return Uint8List.fromList(res.data ?? []);
  }

  Future<Uint8List> exportAgents(
    int idEcole, {
    String? date,
    String? fonction,
    String format = 'xlsx',
  }) async {
    final res = await _dio.get<List<int>>(
      '/api/Presence/agents/ecole/$idEcole/feuille-appel/export',
      queryParameters: {
        if (date != null) 'date': date,
        if (fonction != null && fonction.isNotEmpty) 'fonction': fonction,
        'format': format,
      },
      options: Options(responseType: ResponseType.bytes),
    );
    return Uint8List.fromList(res.data ?? []);
  }
}
```

---

## 9. Flutter — Provider et ouverture fichier

Dépendances typiques : `path_provider`, `open_filex` (ou `share_plus`).

```dart
import 'dart:io';
import 'package:path_provider/path_provider.dart';
import 'package:open_filex/open_filex.dart';

Future<void> saveAndOpenExport(Uint8List bytes, String fileName) async {
  final dir = await getTemporaryDirectory();
  final file = File('${dir.path}/$fileName');
  await file.writeAsBytes(bytes, flush: true);
  await OpenFilex.open(file.path);
}
```

Exemple Provider (Provider / Riverpod selon le projet) :

```dart
Future<void> exportCurrent({required String format}) async {
  final stamp = date.replaceAll('-', '');
  if (tab == FeuilleTab.eleves) {
    final bytes = await api.exportEleves(idClasse!, date: date, format: format);
    await saveAndOpenExport(bytes, 'FeuilleAppel_classe$idClasse\_$stamp.$format');
  } else {
    final bytes = await api.exportAgents(
      idEcole,
      date: date,
      fonction: fonction.isEmpty ? null : fonction,
      format: format,
    );
    await saveAndOpenExport(bytes, 'FeuilleAppel_Agents_ecole$idEcole\_$stamp.$format');
  }
}
```

Couleur pastille `statutJour` :

```dart
Color statutColor(String s) {
  switch (s) {
    case 'Present':
      return Colors.green;
    case 'Retard':
      return Colors.orange;
    default:
      return Colors.red;
  }
}
```

---

## 10. Gestion des erreurs

| Code | Contexte | Action UX |
|------|----------|-----------|
| **401** | JWT expiré | Logout → login |
| **403** | `idEcole` JWT ≠ école de la ressource | Message « Accès refusé pour cette école » |
| **404** | Classe / école introuvable | Message API `message` |
| **400** | `format` invalide | Afficher `message` (formats acceptés : `xlsx`, `pdf`) |
| **500** | Erreur serveur / PDF | Toast + retry |

Exemple message 400 :

```json
{ "message": "Format 'docx' non supporté. Formats acceptés : xlsx, pdf." }
```

**Note :** `format=pdf` est supporté (élèves et agents). Ne plus afficher l'ancien message « PDF phase 2 ».

---

## 11. Checklist d'intégration

### Vue 3

- [ ] Types `feuilleAppel.ts`
- [ ] `fetchFeuilleAppelEleves` / `fetchFeuilleAppelAgents`
- [ ] Export blob `xlsx` + `pdf` + `triggerBlobDownload`
- [ ] Onglets Élèves / Agents, date, sélecteur classe, filtre fonction
- [ ] Pastilles `Present` / `Absent` / `Retard`
- [ ] Gestion 401 / 403 / 404 / 400

### Flutter

- [ ] Feature `feuille_appel/` (modèles + `FeuilleAppelApi`)
- [ ] Écran onglets + totaux
- [ ] Export bytes → fichier temporaire → ouverture
- [ ] Claim `idEcole` passé correctement pour les routes agents

### Vérifications manuelles (Swagger / app)

- [ ] JSON élèves classe connue
- [ ] JSON agents école JWT
- [ ] Export Excel ouvre correctement
- [ ] Export PDF ouvre correctement
- [ ] 403 si autre `idEcole` (token restreint)

---

**Guide parent contrôle d'entrée :** [DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md)
