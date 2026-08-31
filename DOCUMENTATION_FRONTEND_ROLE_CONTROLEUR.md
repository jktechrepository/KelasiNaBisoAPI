# Documentation Frontend — Rôle Controleur (contrôle à l'entrée)

Guide pour implémenter l'**écran contrôle à l'entrée** réservé au rôle JWT **`Controleur`** (vérification frais + pointage présence).

**Intégration Vue 3 / Flutter :** [DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md)  
**Grille tarifaire (année + classe) :** [DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md)

**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Auth :** `Authorization: Bearer {jwt_token}`

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Parcours utilisateur](#2-parcours-utilisateur)
3. [Recherche élève par matricule](#3-recherche-élève-par-matricule)
4. [Vérification des frais (lecture seule)](#4-vérification-des-frais-lecture-seule)
5. [Pointage présence](#5-pointage-présence)
6. [Dashboard présence du jour](#6-dashboard-présence-du-jour)
7. [Feuille d'appel et export](#7-feuille-dappel-et-export)
8. [Permissions et erreurs](#8-permissions-et-erreurs)
9. [Checklist implémentation](#9-checklist-implémentation)

---

## 1. Vue d'ensemble

| Rôle | Pointage présence | Frais (lecture) | Encaissement | Correction présence |
|------|-------------------|-----------------|--------------|---------------------|
| **Controleur** | ✅ Create + Read | ✅ | ❌ (403) | ❌ (403) |
| **Enseignant** | ✅ + Update | ❌ | ❌ | ✅ |
| **Caissier** | ❌ | ✅ Lecture | ✅ | ❌ |
| **Financier** | — | ✅ CRUD | ✅ | — |

Le Controleur vérifie qu'un élève est **à jour sur ses frais** et enregistre sa **présence** à l'entrée. Il ne touche ni à la comptabilité ni au guichet.

---

## 2. Parcours utilisateur

```
Login (JWT Controleur)
    │
    ▼
Recherche matricule ──► GET /api/Eleve/reinscription?matricule={matricule}
    │
    ▼
Solde / barèmes ──► GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
    │
    ▼
Pointage ──► POST /api/Presence
    │
    ▼
Dashboard jour ──► GET /api/Presence/dashboard/ecole/{idEcole}
```

Pas d'écran guichet, Moko PayIn ni wallet pour ce rôle.

---

## 3. Recherche élève par matricule

### Route

```
GET /api/Eleve/reinscription?matricule={matricule}
```

| Paramètre | Description |
|-----------|-------------|
| `matricule` | Matricule de l'élève (recherche rapide à l'entrée) |

**Réponse attendue :** 200 avec fiche élève (nom, classe, photo, etc.) ou 404 si introuvable.

**Permissions :** `Eleve.Read` / `Eleve.ReadAll`

---

## 4. Vérification des frais (lecture seule)

> **Barèmes et scoping année/classe :** [DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_FRAIS_VUE_FLUTTER.md)

### Solde par matricule

```
GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
```

Affiche les frais dus, payés et le solde restant pour l'année en cours.

### Liste des barèmes de l'école

```
GET /api/Frais/ecole/{idEcole}
```

Consultation des types de frais (sans création ni modification).

### Journal paiements (optionnel)

```
GET /api/Paiement/paged?idEcole={idEcole}&page=1&pageSize=20
```

Lecture seule — le Controleur ne peut pas `POST /api/Paiement`.

---

## 5. Pointage présence

### Créer un pointage

```
POST /api/Presence
```

**Corps exemple :**

```json
{
  "idEleve": 123,
  "isPresent": true,
  "dateDuJour": "2026-08-31",
  "heureArrivee": "07:45"
}
```

| Champ | Obligatoire | Description |
|-------|-------------|-------------|
| `idEleve` | oui | ID élève |
| `isPresent` | oui | `true` = présent |
| `dateDuJour` | oui | Date du jour (yyyy-MM-dd) |
| `heureArrivee` | oui | Heure d'arrivée (`HH:mm` ou `7h30`) |
| `observation` | non | Note optionnelle |

**Permission requise :** `Presence.Create`

### Interdit pour Controleur

```
PUT /api/Presence/{id}
```

Retourne **403** — correction réservée à Enseignant / Admin.

---

## 6. Dashboard présence du jour

```
GET /api/Presence/dashboard/ecole/{idEcole}?date={yyyy-MM-dd}
```

| Paramètre | Défaut | Description |
|-----------|--------|-------------|
| `idEcole` | path | Doit correspondre au JWT |
| `date` | aujourd'hui | Journée affichée |

Vue synthétique : effectifs présents / absents, statistiques par classe.

---

## 7. Feuille d'appel et export

### Feuille d'appel par classe

```
GET /api/Presence/eleves/classe/{idClasse}/feuille-appel?date={yyyy-MM-dd}
```

### Export Excel

```
GET /api/Presence/eleves/classe/{idClasse}/feuille-appel/export?date={yyyy-MM-dd}&format=xlsx
```

### Vues présence (optionnel)

```
GET /api/VuePointagePresenceParEcole/...
```

---

## 8. Permissions et erreurs

### Permissions JWT attendues (extrait)

| Permission | Usage |
|------------|-------|
| `Presence.Create`, `Presence.Read`, `Presence.ReadAll` | Pointage et listes |
| `Frais.Read`, `Frais.ReadAll` | Barèmes |
| `Paiement.Read`, `Paiement.ReadAll` | Vérifier paiements |
| `Eleve.Read`, `Eleve.ReadAll` | Recherche matricule |
| `Classe.Read`, `Inscription.Read` | Contexte |

### Absentes (403 garanti)

- `Paiement.Create` — réservé Caissier / Financier
- `Frais.Create`, `Frais.Update`, `Frais.Delete`
- `Presence.Update`, `Presence.Delete`
- Wallet Moko : `GET /api/Ecole/{id}/paiement-mobile/wallet/mouvements`

### Codes HTTP

| Code | Action front |
|------|--------------|
| 401 | Rediriger vers login |
| 403 | Masquer l'action / message « non autorisé » |
| 404 | Élève ou ressource introuvable |

---

## 9. Checklist implémentation

- [ ] Détecter le rôle `Controleur` dans le JWT après login
- [ ] Écran unique : champ matricule + résultat élève + solde frais
- [ ] Bouton « Marquer présent » → `POST /api/Presence`
- [ ] Indicateur visuel frais impayés (rouge) vs à jour (vert)
- [ ] Ne pas afficher guichet, Moko, CRUD frais
- [ ] Ne pas proposer correction présence (`PUT`)
- [ ] Dashboard présence du jour en accueil secondaire
- [ ] Gérer 403 sur endpoints interdits
- [ ] Reconnexion après migration prod (claim JWT `Controleur`)

---

**Migration prod :** [`Scripts/README_ROLE_CONTROLEUR.md`](Scripts/README_ROLE_CONTROLEUR.md)
