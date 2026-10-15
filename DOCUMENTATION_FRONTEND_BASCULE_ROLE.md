# Documentation Frontend — Bascule de rôle (session)

Guide d’intégration **Vue 3** (web) et **Flutter** (mobile parent / enseignant / multi-rôles).

**Base URL dev :** `https://localhost:7102` ou `https://dev-knb.asdc-rdc.org`  
**Swagger :** `/swagger/index.html` — tag **Utilisateur**  
**Auth :** `Authorization: Bearer {accessToken}`

Références :
- [GUIDE_FRONTEND_PERMISSIONS.md](GUIDE_FRONTEND_PERMISSIONS.md) — menus à partir de `permissions`
- Swagger : `POST /api/Utilisateur/{id}/roles/{roleId}/bascule`

---

## Table des matières

1. [Vue d’ensemble](#1-vue-densemble)
2. [Endpoint](#2-endpoint)
3. [Login vs bascule vs refresh](#3-login-vs-bascule-vs-refresh)
4. [Réponse JSON](#4-réponse-json)
5. [Erreurs](#5-erreurs)
6. [Recommandations UI](#6-recommandations-ui)
7. [Vue 3 — types, service, store](#7-vue-3--types-service-store)
8. [Flutter — Dio et persistance](#8-flutter--dio-et-persistance)
9. [Piège refresh-token](#9-piège-refresh-token)
10. [Checklist](#10-checklist)

---

## 1. Vue d’ensemble

Un utilisateur peut avoir **plusieurs rôles actifs** (ex. Enseignant + Parent). Le **login** renvoie :

- `roles` : tous les rôles actifs ;
- `primaryRole` : rôle principal **en base** (`IsPrimary`) ;
- `permissions` : **union** de tous les rôles ;
- JWT : **tous** les rôles en `ClaimTypes.Role` (`User.IsInRole("Parent")` reste vrai même en mode enseignant).

La **bascule** change uniquement la **session** : nouveau access token + nouveau refresh token, `nomRole` et `permissions` **du rôle choisi**. Elle **ne modifie pas** `primaryRole` / `IsPrimary` en base.

| Besoin | Endpoint |
|--------|----------|
| Changer le contexte UI (menus, `IsInRole`) | `POST .../roles/{roleId}/bascule` |
| Changer le rôle principal persisté | `PUT .../roles/{roleId}/primary` — **Admin / Super-Admin uniquement**, à ne pas utiliser pour le switcher |

Les attributs `[Permission("X")]` côté API restent l’**union des rôles en base**. Les menus / écrans doivent suivre `permissions` + `nomRole` de la **dernière** réponse auth (login ou bascule).

---

## 2. Endpoint

| Méthode | Route | Body |
|---------|-------|------|
| POST | `/api/Utilisateur/{id}/roles/{roleId}/bascule` | Aucun |

| Path | Type | Règle |
|------|------|--------|
| `id` | int | `idUtilisateur` du JWT (`NameIdentifier` / `sub`). Un autre `id` → **403**, sauf Super-Admin. |
| `roleId` | int | `idRole` d’un rôle **déjà attribué et actif** (`roles[]` du login). |

```http
POST /api/Utilisateur/223/roles/3/bascule
Authorization: Bearer {accessToken}
```

- **POST**, pas GET.
- Pas de query, pas de JSON body.
- Source de la liste pour le sélecteur : `roles` de la dernière auth. **Ne pas** appeler `GET /api/Utilisateur/{id}/roles` (Admin / Super-Admin seulement).

---

## 3. Login vs bascule vs refresh

| | Login `POST /authentifier` | Bascule | `POST /refresh-token` |
|--|---------------------------|---------|------------------------|
| JWT `ClaimTypes.Role` | Tous les rôles | **Un seul** (rôle choisi) | Tous les rôles (pas de rôle de session) |
| `permissions` | Union de tous les rôles | **Uniquement** le rôle choisi | Union de tous les rôles |
| `nomRole` | Souvent le principal | Nom du rôle basculé | Principal |
| `roles` | Liste complète | Liste complète | Liste complète |
| `primaryRole` | Base | **Inchangé** (base) | Base |
| `refreshToken` | Nouveau | **Nouveau** (remplacer le stocké) | Rotation (nouveau) |
| `IdAgent` / `IdTuteur` | Inchangés | Inchangés | Inchangés |

Après bascule, **remplacer** `accessToken` **et** `refreshToken` (ne pas garder l’ancien refresh en ignorant le nouveau, et ne pas écraser par `null`).

Claims JWT utiles après bascule :

- `role` : un seul nom (contexte `IsInRole`) ;
- `idRole` / `primaryRole` : rôle **de session** (pas forcément `IsPrimary` en base) ;
- `roles` / `roleIds` (JSON) : **tous** les rôles pour le switcher ;
- `IdAgent`, `IdTuteur`, `idEcole` : inchangés.

---

## 4. Réponse JSON

Même forme que l’authentification (`AuthentificationResponse`, camelCase). Exemple (tronqué) :

```json
{
  "success": true,
  "message": "Rôle de session basculé",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "…",
  "tokenType": "Bearer",
  "expiresIn": 86400,
  "expiresAt": "2026-10-10T14:12:33.821927Z",
  "doitChangerMotDePasse": false,
  "utilisateur": {
    "idUtilisateur": 223,
    "idAgent": 45,
    "idTuteur": 12,
    "idEcole": 1
  },
  "nomRole": "Parent",
  "nomEcole": "École Exemple",
  "acceptNotification": true,
  "permissions": [
    "Eleve.ReadChildren",
    "Paiement.Read",
    "HoraireDeCours.Read"
  ],
  "roles": [
    { "idRole": 2, "nom": "Enseignant", "niveau": 4, "statut": true },
    { "idRole": 3, "nom": "Parent", "niveau": 5, "statut": true }
  ],
  "primaryRole": {
    "idRole": 2,
    "nom": "Enseignant",
    "niveau": 4,
    "statut": true
  }
}
```

| Champ | Usage front |
|-------|-------------|
| `accessToken` | Header Bearer de tous les appels suivants |
| `refreshToken` | Stockage persistant (remplace l’ancien) |
| `nomRole` | Libellé du mode actif (header, switcher) |
| `permissions` | Afficher / masquer menus (pas l’union du login) |
| `roles` | Options du switcher (`idRole` + `nom`) |
| `primaryRole` | Information « rôle par défaut » ; **ne pas** s’en servir comme mode UI après une bascule |
| `utilisateur.idRole` | Rôle historique utilisateur ; le contexte UI = `nomRole` de la réponse |

---

## 5. Erreurs

| HTTP | Cas | Corps typique | Action UI |
|------|-----|----------------|-----------|
| 200 | OK | `AuthentificationResponse` | Remplacer le blob auth, reconstruire menus |
| 401 | JWT manquant / invalide | `{ "message": "Token JWT invalide" }` | Relogin |
| 403 | `{id}` ≠ utilisateur connecté | `{ "message": "Vous ne pouvez basculer que votre propre rôle." }` | Utiliser `utilisateur.idUtilisateur` du JWT |
| 404 | Utilisateur inconnu | `{ "message": "Utilisateur {id} introuvable" }` | Relogin |
| 404 | `roleId` inconnu | `{ "message": "Rôle {roleId} introuvable" }` | Rafraîchir la liste `roles` |
| 400 | Rôle non attribué / inactif | `{ "message": "L'utilisateur n'a pas ce rôle." }` | Ne proposer que `roles[]` |
| 500 | Erreur serveur | `{ "message": "Erreur lors de la bascule de rôle", … }` | Toast + retry |

---

## 6. Recommandations UI

1. Afficher le switcher seulement si `roles.length > 1`.
2. Marquer le rôle actif avec `nomRole` (session), pas `primaryRole.nom`.
3. Au tap : `POST` puis **remplacer** tout le stockage auth (tokens + permissions + nomRole).
4. Recalculer la navigation (drawer, bottom bar) à partir des **nouvelles** `permissions`.
5. Recharger les listes scopées (ex. horaires enseignant vs parent) : le JWT `IsInRole` a changé.
6. Ne pas appeler `PUT .../primary`.
7. Super-Admin : peut basculer un autre `{id}` ; les apps école n’en ont pas besoin.

---

## 7. Vue 3 — types, service, store

```ts
export type RoleDto = {
  idRole: number
  nom: string
  niveau?: number | null
  statut?: boolean | null
}

export type AuthResponse = {
  success: boolean
  message: string
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresIn: number
  expiresAt: string
  nomRole: string
  permissions: string[]
  roles: RoleDto[]
  primaryRole: RoleDto | null
  utilisateur: { idUtilisateur: number }
}

export async function basculeRole(
  baseUrl: string,
  accessToken: string,
  idUtilisateur: number,
  roleId: number
): Promise<AuthResponse> {
  const res = await fetch(
    `${baseUrl}/api/Utilisateur/${idUtilisateur}/roles/${roleId}/bascule`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        Accept: 'application/json',
      },
    }
  )
  const body = await res.json().catch(() => ({}))
  if (!res.ok) {
    throw new Error(body.message ?? `HTTP ${res.status}`)
  }
  return body as AuthResponse
}
```

Pinia (remplacer le blob, ne pas merger partiellement) :

```ts
async function switchRole(roleId: number) {
  const id = this.auth.utilisateur.idUtilisateur
  const next = await basculeRole(apiBase, this.auth.accessToken, id, roleId)
  this.auth = next
  localStorage.setItem('auth', JSON.stringify(next))
}
```

`can(permission: string)` : `auth.permissions.includes(permission)` après chaque bascule.

---

## 8. Flutter — Dio et persistance

```dart
Future<Map<String, dynamic>> basculeRole({
  required Dio dio,
  required int idUtilisateur,
  required int roleId,
}) async {
  final res = await dio.post(
    '/api/Utilisateur/$idUtilisateur/roles/$roleId/bascule',
  );
  if (res.statusCode != 200) {
    throw Exception(res.data?['message'] ?? 'Bascule échouée');
  }
  return Map<String, dynamic>.from(res.data as Map);
}
```

```dart
Future<void> applyAuth(Map<String, dynamic> auth) async {
  await secureStorage.write(key: 'accessToken', value: auth['accessToken'] as String);
  await secureStorage.write(key: 'refreshToken', value: auth['refreshToken'] as String);
  await prefs.setString('auth_json', jsonEncode(auth));
}
```

- Intercepteur Dio : toujours le **dernier** `accessToken`.
- Après succès : `applyAuth` **complet** (y compris `refreshToken`).
- Menus : `List<String>.from(auth['permissions'])` et `auth['nomRole']`.
- Switcher : `(auth['roles'] as List)` → `idRole` / `nom`.

---

## 9. Piège refresh-token

`POST /api/Utilisateur/refresh-token` génère un JWT **sans** rôle de session (tous les `ClaimTypes.Role` à nouveau, `permissions` = union).

Options front :

1. Après refresh, **rejouer** la bascule vers le `roleId` encore affiché (si toujours dans `roles`).
2. Ou conserver `nomRole` / `permissions` en local pour l’UI, en sachant que `IsInRole` côté API redevient multi-rôles jusqu’à une nouvelle bascule.

Ne pas traiter le refresh comme une bascule.

---

## 10. Checklist

- [ ] Switcher seulement si plus d’un rôle
- [ ] `id` = `utilisateur.idUtilisateur` du JWT
- [ ] `roleId` pris dans `roles[]` du login / dernière auth
- [ ] POST sans body
- [ ] Remplacer `accessToken` **et** `refreshToken`
- [ ] Menus = `permissions` de la bascule, pas l’union du login
- [ ] Label actif = `nomRole`, pas `primaryRole`
- [ ] Ne pas appeler `PUT .../primary` ni `GET .../roles` pour soi-même
- [ ] Prévoir le refresh-token (re-bascule ou perte du contexte JWT)

**Swagger :** `https://localhost:7102/swagger/index.html` — `POST /api/Utilisateur/{id}/roles/{roleId}/bascule`.
