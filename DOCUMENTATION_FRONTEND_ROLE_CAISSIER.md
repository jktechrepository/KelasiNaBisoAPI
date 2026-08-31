# Documentation Frontend — Rôle Caissier (écran guichet)

Guide complet pour implémenter l'**écran guichet** réservé au rôle JWT **`Caissier`** (et consultation par Directeur / Financier / Admin).

**Intégration Vue 3 / Flutter :** [DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md](DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md)  
**Complément Moko :** [DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md](DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md)  
**Base URL dev :** `https://dev-knb.asdc-rdc.org`  
**Auth :** `Authorization: Bearer {jwt_token}`

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Parcours utilisateur](#2-parcours-utilisateur)
3. [Écran accueil — Dashboard caissier](#3-écran-accueil--dashboard-caissier)
4. [Recherche élève](#4-recherche-élève)
5. [Affichage frais dus / solde](#5-affichage-frais-dus--solde)
6. [Encaissement espèces / chèque](#6-encaissement-espèces--chèque)
7. [Encaissement Mobile Money (Moko)](#7-encaissement-mobile-money-moko)
8. [Journal de caisse paginé](#8-journal-de-caisse-paginé)
9. [Clôture de caisse](#9-clôture-de-caisse)
10. [Supervision école (Directeur / Financier)](#10-supervision-école-directeur--financier)
11. [Permissions et erreurs](#11-permissions-et-erreurs)
12. [Checklist implémentation](#12-checklist-implémentation)

---

## 1. Vue d'ensemble

| Rôle | Écran guichet | Wallet Moko | Config Moko | CRUD frais |
|------|---------------|-------------|-------------|------------|
| **Caissier** | ✅ | ❌ (403) | Lecture overview seulement | ❌ |
| **Financier** | ✅ + supervision | ✅ | ✅ | ✅ |
| **Directeur / Admin** | ✅ + supervision | ✅ | ✅ | Partiel |

Le Caissier encaisse au guichet pour n'importe quel élève de **son école** (`idEcole` JWT). Il ne gère pas la trésorerie ni la configuration Moko.

---

## 2. Parcours utilisateur

```
Login (JWT Caissier)
    │
    ▼
GET /api/Dashboard/caissier?idEcole={jwt.idEcole}
    │
    ├── Recherche matricule ──► GET /api/Eleve/reinscription?matricule=
    │                              GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
    │
    ├── Encaissement Cash/Chèque ──► POST /api/Paiement
    │
    └── Encaissement Mobile Money ──► POST /api/MokoAfrika/payin/frais-scolaire
                                          └── polling POST /api/MokoAfrika/status/{ref}/check
```

Fin de journée :

```
GET /api/Dashboard/caissier/cloture?idEcole={id}&date={yyyy-MM-dd}
    └── Export PDF / impression côté front (pas d'endpoint PDF API v1)
```

---

## 3. Écran accueil — Dashboard caissier

### Route

```
GET /api/Dashboard/caissier?idEcole={idEcole}&idAnneeScolaire={optionnel}&date={optionnel}&scope=moi
```

| Paramètre | Défaut | Description |
|-----------|--------|-------------|
| `idEcole` | requis | Doit correspondre au JWT |
| `idAnneeScolaire` | année courante | Filtre frais / inscriptions |
| `date` | aujourd'hui | Journée de caisse |
| `scope` | `moi` | `moi` = encaissements du caissier connecté |

**Ne pas utiliser** `GET /api/Dashboard/global` pour le guichet (KPI direction, présence, wallet).

### Blocs réponse

| Bloc | Usage UI |
|------|----------|
| `resume.nombrePaiements` / `montantTotal` | KPI du jour |
| `resume.payInsReussis` / `payInsEnAttente` / `payInsEchoues` | Badges Moko |
| `repartitionParMode.modes` | Camembert Cash / Chèque / MM |
| `derniersPaiements` | 15 dernières lignes |
| `moko.estConfigure` | Bandeau « Configurer Moko » si `false` |
| `moko.payInsEnAttente` | Liste refs à poller |
| `scope` | `"moi"` ou `"ecole"` |

### Rafraîchissement

- Au login et après chaque encaissement réussi
- Polling Moko : toutes les 5–10 s sur les refs `payInsEnAttente` jusqu'à statut final
- Option : bouton « Actualiser »

---

## 4. Recherche élève

### Par matricule (recommandé guichet)

```
GET /api/Eleve/reinscription?matricule={matricule}&idEcole={jwt.idEcole}
```

Retourne l'identité élève, classe, inscription — suffisant pour pré-sélectionner l'élève.

### Par nom (fallback)

```
GET /api/Eleve/reinscription?nomComplet={nom}&idEcole={jwt.idEcole}
```

Pagination si plusieurs homonymes.

### UX suggérée

1. Champ « Matricule » + touche Entrée
2. Carte élève (photo, nom, classe)
3. Bouton « Encaisser » → écran paiement

---

## 5. Affichage frais dus / solde

```
GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}
```

Chaque ligne = un frais avec montant dû, payé, reste. Filtrer côté front les lignes avec `reste > 0` pour la liste « À payer ».

Alternative si `idEleve` connu :

```
GET /api/VuePaiementsFraisParEcole/eleve/{idEleve}
```

Afficher pour chaque frais :

- Libellé, montant total, déjà payé, **solde restant**
- Sélection du frais avant encaissement

---

## 6. Encaissement espèces / chèque

```
POST /api/Paiement
```

**Permission :** `Paiement.Create` (Caissier ✅)

### Corps minimal

```json
{
  "idEleve": 123,
  "idFrais": 45,
  "montant": 50.00,
  "modePaiement": "Cash",
  "statutPaiement": "Confirme",
  "datePaiement": "2026-08-31T10:30:00"
}
```

Modes autorisés sur ce endpoint : **`Cash`**, **`Chèque`**, **`Virement`**.

### Interdit — Mobile Money / Carte

Si le front envoie `modePaiement: "Mobile Money"` :

```json
HTTP 400
{
  "message": "Les paiements Mobile Money et Carte doivent passer par POST /api/MokoAfrika/payin/frais-scolaire...",
  "code": "MOKO_PAYIN_REQUIRED"
}
```

→ Rediriger vers le flux Moko (section 7).

### Après succès

1. Toast « Paiement enregistré »
2. Rafraîchir dashboard caissier
3. Rafraîchir solde élève (`VuePaiementsFraisParEcole`)
4. Option v2 : générer un ticket PDF côté front (données du paiement retourné)

---

## 7. Encaissement Mobile Money (Moko)

Voir [DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md §6](DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md) pour le détail.

### Séquence guichet

1. `GET /api/Ecole/{idEcole}/paiement-mobile` — vérifier `estConfigure`
2. `GET /api/MokoAfrika/fees/estimate?amount=&operator=` — frais affichés au caissier
3. `POST /api/MokoAfrika/payin/frais-scolaire` — saisir **téléphone du payeur**
4. Afficher « En attente USSD » + référence
5. `POST /api/MokoAfrika/status/{reference}/check` toutes les 5–10 s
6. Succès → rafraîchir dashboard + solde élève

Le Caissier **ne peut pas** accéder à `GET .../wallet/mouvements` (403).

---

## 8. Journal de caisse paginé

Pour un historique plus riche que les 15 lignes du dashboard :

```
GET /api/Paiement/paged?idEcole={id}&idAnneeScolaire={id}&idUtilisateur={optionnel}&page=1&pageSize=20
```

| Paramètre | Usage |
|-----------|-------|
| `idUtilisateur` | Filtrer « ma caisse » (`jwt.idUtilisateur`) ; omettre pour toute l'école (supervision) |
| `sortBy` / `sortOrder` | Tri par date |
| `search` | Recherche texte si supportée |

Plage de dates :

```
GET /api/Paiement/date-range/paged?idEcole={id}&dateDebut=&dateFin=&page=1&pageSize=50
```

---

## 9. Clôture de caisse

```
GET /api/Dashboard/caissier/cloture?idEcole={id}&date={yyyy-MM-dd}&scope=moi
```

Retourne le même résumé que le dashboard + **`tousLesPaiements`** (liste complète du jour) + `genereLe`.

### Impression / export

L'API ne génère pas de PDF en v1. Le front doit :

1. Appeler `cloture`
2. Rendre un template HTML (logo école, totaux par mode, tableau paiements)
3. Imprimer ou exporter PDF via la librairie front (jsPDF, print CSS, etc.)

Champs utiles pour le ticket :

- `ecole.nomEcole`, `periode.libelle`
- `resume.montantTotal`, `repartitionParMode`
- `tousLesPaiements[]` : élève, frais, montant, mode, heure

---

## 10. Supervision école (Directeur / Financier)

Utilisateurs avec rôle **Directeur**, **Financier**, **Admin** ou **Super-Admin** :

```
GET /api/Dashboard/caissier?idEcole={id}&scope=ecole
GET /api/Dashboard/caissier/cloture?idEcole={id}&scope=ecole&date=
```

- `scope=ecole` : **tous** les encaissements de l'école pour la journée (tous caissiers)
- Un **Caissier** qui passe `scope=ecole` est ignoré → reste sur `scope=moi`

Journal supervision :

```
GET /api/Paiement/paged?idEcole={id}&dateDebut=&dateFin=...
```

(sans `idUtilisateur` = toute l'école)

---

## 11. Permissions et erreurs

| Code HTTP | Cas |
|-----------|-----|
| **401** | Token absent / expiré → login |
| **403** | Mauvais rôle ou mauvaise école (`idEcole` ≠ JWT) |
| **400** `MOKO_PAYIN_REQUIRED` | MM sur `POST /api/Paiement` |
| **503** `MOKO_MIGRATION_REQUIRED` | Migration Moko absente — bandeau admin |

Endpoints **interdits** au Caissier (403 attendu) :

- `GET /api/Ecole/{id}/paiement-mobile/wallet/mouvements`
- `GET /api/Ecole/{id}/paiement-mobile/payouts`
- `POST /api/Frais` (création frais)

---

## 12. Checklist implémentation

- [ ] Route `/guichet` protégée rôle `Caissier` (+ redirect si autre rôle)
- [ ] Dashboard accueil `GET /api/Dashboard/caissier`
- [ ] Recherche matricule + affichage frais dus
- [ ] Formulaire Cash/Chèque → `POST /api/Paiement`
- [ ] Flux Moko PayIn + polling (ne jamais POST MM sur `/api/Paiement`)
- [ ] Liste PayIns en attente depuis dashboard
- [ ] Journal paginé `GET /api/Paiement/paged?idUtilisateur=`
- [ ] Clôture jour + impression HTML/PDF front
- [ ] (Option) Vue supervision `scope=ecole` pour Directeur/Financier
- [ ] Gestion erreurs 403 wallet / 503 migration

---

*Dernière mise à jour : 31 août 2026*
