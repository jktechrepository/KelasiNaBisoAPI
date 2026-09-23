# Intégration Vue / Flutter — Tuteur / Parent (création + affectation élève)

Guide pour créer un tuteur avec compte Parent, puis l’affecter à un élève existant.

**Base URL :** `https://localhost:7102` / `https://dev-knb.asdc-rdc.org`  
**JSON :** camelCase  
**Auth :** Bearer JWT  

**Rôles communs** (création + affectation) : `Admin`, `Super-Admin`, `Directeur`

---

## 1. Flux recommandé

```mermaid
flowchart LR
  create[POST api/Tuteur]
  creds[Afficher username MDP]
  affect[PUT api/Eleve id tuteur]
  create --> creds --> affect
```

1. `POST /api/Tuteur?idEcole=` → récupérer `tuteur.idTuteur` et `compteUtilisateur`
2. Si `motDePasseParDefaut` non vide → afficher / envoyer les identifiants (toast, SMS)
3. `PUT /api/Eleve/{idEleve}/tuteur` avec `{ "idTuteur": … }` pour lier le parent à l’élève

Un élève n’a **qu’un seul** tuteur : l’affectation **remplace** le précédent.

---

## 2. Créer un tuteur + compte Parent

### Endpoint

```http
POST /api/Tuteur?idEcole={idEcole}
Authorization: Bearer …
Content-Type: application/json
```

| Paramètre | Obligatoire | Description |
|-----------|-------------|-------------|
| `idEcole` (query) | Oui | École du compte `Utilisateur` Parent + contrôle `ForbidIfWrongSchool` |

Le modèle `Tuteur` n’a **pas** d’`IdEcole` (parent multi-écoles via les enfants). L’école sert uniquement au rattachement du compte utilisateur.

### Body

| Champ | Obligatoire | Notes |
|-------|-------------|--------|
| `nomComplet` | Oui | Max 200 |
| `genre` | Oui | `M` ou `F` |
| `telephone` | Oui | Format téléphone |
| `email` | Non | Unicité si fourni |
| `photoTuteurUrl` | Non | |
| `pieceIdentiteTuteur` | Non | |
| `nomCompletRepresentant` | Non | |
| `telephoneRepresentant` | Non | |

```json
{
  "nomComplet": "Marie Dupont",
  "genre": "F",
  "telephone": "+243810000001",
  "email": "marie@exemple.cd",
  "photoTuteurUrl": null,
  "pieceIdentiteTuteur": null,
  "nomCompletRepresentant": null,
  "telephoneRepresentant": null
}
```

### Réponse `201`

```json
{
  "tuteur": {
    "idTuteur": 12,
    "nomComplet": "Marie Dupont",
    "genre": "F",
    "telephone": "+243810000001",
    "email": "marie@exemple.cd",
    "statut": true
  },
  "compteUtilisateur": {
    "idUtilisateur": 45,
    "idTuteur": 12,
    "defaultUsername": "MarieDupont456",
    "motDePasseParDefaut": "123456",
    "email": "marie@exemple.cd",
    "telephone": "+243810000001",
    "nomComplet": "Marie Dupont",
    "role": "Parent"
  }
}
```

| Situation | `motDePasseParDefaut` | Comportement |
|-----------|----------------------|--------------|
| Nouveau compte | `"123456"` | Afficher / SMS ; `DoitChangerMotDePasse = true` côté serveur |
| Compte déjà existant (même téléphone / email) | `""` | Rôle Parent ajouté / `IdTuteur` lié ; **ne pas** réafficher un MDP |

Même logique que la création de compte Parent à l’inscription (`ITuteurCompteService`).

### Erreurs typiques

| Code | Cas |
|------|-----|
| `400` | Validation body, email déjà utilisé par un autre tuteur, `idEcole` manquant |
| `403` | École différente du claim JWT (hors Super-Admin / IT-Support) |
| `404` | École introuvable |

---

## 3. Affecter un tuteur à un élève

Préférer cet endpoint au `PUT /api/Eleve/{id}` (profil complet) pour un simple changement de parent.

### Endpoint

```http
PUT /api/Eleve/{idEleve}/tuteur
Authorization: Bearer …
Content-Type: application/json
```

**Scope :** école courante de l’élève (`GetEcoleCouranteAsync` + `ForbidIfWrongSchool`).

### Body

```json
{ "idTuteur": 12 }
```

`idTuteur` obligatoire et **> 0** (réaffectation uniquement — pas de détachement en V1).

### Réponse `200`

```json
{
  "idEleve": 5,
  "idTuteur": 12,
  "idTuteurPrecedent": 3,
  "nomCompletEleve": "Jean Mukendi",
  "nomCompletTuteur": "Marie Dupont"
}
```

### Erreurs typiques

| Code | Cas |
|------|-----|
| `400` | Body invalide / `idTuteur` ≤ 0 |
| `403` | Élève hors école de l’utilisateur |
| `404` | Élève introuvable ou inactif ; tuteur introuvable ou inactif |

---

## 4. UX front (Vue / Flutter)

1. **Écran « Nouveau parent »** : formulaire → `POST /api/Tuteur?idEcole=`
2. **Credentials** : si `motDePasseParDefaut` non vide, dialog / snackbar (username + MDP) + option copier / SMS
3. **Écran « Affecter à un élève »** : recherche élève (paged école) → `PUT /api/Eleve/{id}/tuteur`
4. **Confirmation** : indiquer le remplacement si `idTuteurPrecedent` est non null

Pas besoin de renvoyer tout le profil élève pour ce cas d’usage.

---

## 5. Hors scope V1

- Détachement (`IdTuteur = null`)
- Multi-tuteurs (père + mère, etc.)
- Historique des affectations
- Correction automatique du tuteur à la réinscription via `IdTuteurExistant`
