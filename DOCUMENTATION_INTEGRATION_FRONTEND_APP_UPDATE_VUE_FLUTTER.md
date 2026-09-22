# Intégration Flutter — Contrôle des mises à jour app (AppUpdate)

Guide pour le check de version au démarrage (soft / importante / force) et les push FCM associés.

**Base URL :** `https://localhost:7102` / `https://dev-knb.asdc-rdc.org`  
**JSON :** camelCase

---

## 1. Principe

| Niveau | `updateLevel` | UX recommandée |
|--------|---------------|----------------|
| Aucun | `none` | Rien |
| Soft (A) | `soft` | Bandeau / dialog dismissible + lien store |
| Importante (B) | `important` | Dialog fort + CTA store (app utilisable) |
| Force (C) | `force` | Écran bloquant + CTA store uniquement |

Le **GET policy** est la source de vérité (fonctionne avant login).  
Le **POST notify** (Super-Admin) envoie un FCM complémentaire (`type=app_update`).

---

## 2. Endpoints

| Méthode | Route | Auth |
|---------|-------|------|
| `GET` | `/api/AppUpdate/policy?platform=Android\|iOS&appVersion=1.2.3` | **Anonyme** |
| `GET` | `/api/AppUpdate/policies` | Super-Admin |
| `PUT` | `/api/AppUpdate/policies/{platform}` | Super-Admin |
| `POST` | `/api/AppUpdate/notify` | Super-Admin |

`platform` : `Android` ou `iOS` (casse tolérée).  
`appVersion` : semver `major.minor.patch`.

### Exemple check

```http
GET /api/AppUpdate/policy?platform=Android&appVersion=2.1.0
```

```json
{
  "platform": "Android",
  "appVersion": "2.1.0",
  "updateLevel": "important",
  "forceUpdate": false,
  "message": "Mettez à jour l'application.",
  "storeUrl": "https://play.google.com/store/apps/details?id=...",
  "minSupportedVersion": "2.0.0",
  "latestVersion": "3.0.0",
  "recommendFromVersion": "2.5.0"
}
```

### Règles serveur

1. `appVersion < minSupportedVersion` → `force`
2. sinon `appVersion < recommendFromVersion` → `important`
3. sinon `appVersion < latestVersion` → `soft`
4. sinon → `none`

Si policy absente ou `isActive=false` → `none`.

### Enregistrement device (optionnel)

`POST /api/UserDevice/register` accepte `appVersion` pour mesurer l’adoption.

---

## 3. Flutter — flux démarrage

```dart
import 'dart:io';
import 'package:package_info_plus/package_info_plus.dart';

Future<AppUpdateCheck> checkAppUpdate(Dio dio) async {
  final info = await PackageInfo.fromPlatform();
  final platform = Platform.isIOS ? 'iOS' : 'Android';
  final res = await dio.get(
    '/api/AppUpdate/policy',
    queryParameters: {
      'platform': platform,
      'appVersion': info.version, // ex. 1.2.3
    },
    options: Options(extra: {'skipAuth': true}),
  );
  return AppUpdateCheck.fromJson(res.data as Map<String, dynamic>);
}
```

Au cold start (et idéalement au resume) :

1. Appeler `checkAppUpdate`
2. Brancher UI selon `updateLevel` / `forceUpdate`
3. Ouvrir `storeUrl` via `url_launcher`

Sur notif FCM avec `data.type == app_update` : re-fetch policy puis même UI.

---

## 4. Admin (Super-Admin)

```http
PUT /api/AppUpdate/policies/Android
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "minSupportedVersion": "2.0.0",
  "recommendFromVersion": "2.5.0",
  "latestVersion": "3.0.0",
  "message": "Une mise à jour importante est disponible.",
  "storeUrl": "https://play.google.com/store/apps/details?id=...",
  "isActive": true
}
```

```http
POST /api/AppUpdate/notify
{
  "titre": "Mise à jour disponible",
  "corps": "Une version importante de Kelasi Na Biso est prête.",
  "platform": "Android",
  "updateLevel": "important"
}
```

---

## 5. Migration BDD production

Appliquer [`docs/sql/20260922_AddMobileAppVersionPolicies.sql`](docs/sql/20260922_AddMobileAppVersionPolicies.sql)  
(table `MobileAppVersionPolicies` + colonne `UserDevices.AppVersion`).

Les seeds Android/iOS sont créés **inactifs** (`IsActive=0`) : activer via PUT après avoir renseigné versions + storeUrl.

---

## 6. Checklist

- [ ] Check anonyme au démarrage (avant login)
- [ ] UI force = non contournable
- [ ] Liens store corrects Android / iOS
- [ ] Super-Admin active la policy après publication store
- [ ] FCM `type=app_update` → re-check policy
- [ ] Script SQL appliqué en prod
