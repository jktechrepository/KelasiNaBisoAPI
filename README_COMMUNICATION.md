# README - Module de communication

## Vue d’ensemble
Le module de communication offre messagerie (privé/groupe), campagnes multi-canal (Push Firebase, Email, SMS, In-App) et notifications temps réel via SignalR. Authentification par JWT Bearer.

## Composants clés
- **Messagerie** : `Message`, `GroupeMessage`, envoi auto Push + fallback SMS.
- **Campagnes** : `CommunicationCampaign`, `CommunicationSegment`, canaux configurables.
- **Temps réel** : SignalR hubs `/hubs/notifications` et `/hubs/dashboard`.
- **Services** : `IFirebaseNotificationService`, `IEmailService`, `ISmsNotificationService` (Twilio), dispatch asynchrone.

## Configuration
- URL API locale : `https://localhost:7102` ou `http://localhost:5002`.
- Hubs SignalR : `.../hubs/notifications`, `.../hubs/dashboard`.
- AppSettings (extrait) :
  - `Firebase:*` pour les notifications Push.
  - `Twilio:Enabled` pour activer/désactiver les SMS (actuellement à `false` en dev/prod pour économiser).
  - SMTP/Email : paramètres `EmailSettings`.
  - Rate limiting (si activé) : section `IpRateLimiting` (désactivable en tests).

## Canaux et priorités
1. Push Firebase (prioritaire, notifications + data).
2. Email (contenu riche, pièces jointes).
3. SMS Twilio (fallback, gouverné par `Twilio:Enabled`).
4. In-App (SignalR + stockage DB) pour la rémanence et la consultation historique.

## Endpoints principaux
- Messagerie : `GET/POST/PUT/DELETE /api/Message`, `GET /api/Message/groupe/{id}`, `PUT /api/Message/toggle-statut/{id}`.
- Groupes : `GET/POST/PUT/DELETE /api/GroupeMessage`, `PUT /api/GroupeMessage/toggle-statut/{id}`.
- Campagnes : `GET /api/Communication`, `GET /api/Communication/{id}`, `POST /api/Communication`, `PUT /api/Communication/{id}`, `POST /api/Communication/{id}/envoyer`, `POST /api/Communication/{id}/annuler`, `POST /api/Communication/{id}/destinataires/recharger`, `GET /api/Communication/{id}/destinataires`, `GET /api/Communication/{id}/historique`.

## Flux de notification (exemple message privé)
1. Sauvegarde du message.
2. Envoi Push Firebase (titre `💬 Message de {expéditeur}`, corps tronqué).
3. Fallback SMS via Twilio si Push échoue et `Twilio:Enabled = true`.
4. Diffusion temps réel via SignalR au destinataire/groupe.

## Flux de notification (campagne)
1. Création/validation (statut `En attente`).
2. Matérialisation des destinataires depuis les segments.
3. Dispatch parallèle vers canaux actifs (Push/Email/SMS/In-App).
4. Suivi des statuts par destinataire + historique.
5. Rappels auto si `rappelAuto=true` et non expiré.

## Exemples frontend (Vue 3 + Axios)
- Service API : ajoute automatiquement le Bearer token.
- SignalR : connexion avec `HubConnectionBuilder` et écoute de `ReceiveNotification`.
- Messagerie : `messageService.sendMessage` puis rafraîchissement de la conversation.
- Campagnes : `communicationService.createCampaign`, `dispatchCampaign`, pagination via `getCampaigns`.

## Bonnes pratiques
- Toujours paginer les listes (messages, campagnes).
- Nettoyer le Markdown côté frontend pour éviter le XSS.
- Journaliser les envois (Push/Email/SMS) pour le suivi et le support.
- Surveiller `Twilio:Enabled` avant d’appeler le SMS service pour éviter les coûts inutiles.

## Rôles et permissions (synthèse)
| Action | Rôles typiques |
| --- | --- |
| Messagerie privée | Tout utilisateur authentifié |
| Création groupe de message | Admin / Directeur / Sous-Directeur |
| Lecture messages d’un groupe | Membres du groupe |
| Campagnes - lecture liste | Super-Admin / Admin / Directeur / Sous-Directeur |
| Campagnes - création/édition | Super-Admin / Admin / Directeur / Sous-Directeur |
| Campagnes - envoi/annulation | Super-Admin / Admin / Directeur / Sous-Directeur |

## Schéma de séquence d’envoi (campagne ou message)
1) Événement (message privé ou campagne validée).
2) Récupération des destinataires + canaux actifs (Push/Email/SMS/In-App).
3) Dispatch parallèle :
   - Push Firebase (prioritaire).
   - Email via `IEmailService`.
   - SMS via `ISmsNotificationService` (Twilio, si `Enabled=true`).
   - In-App via SignalR (NotificationHub) et stockage en base.
4) Journalisation des statuts et erreurs par canal.
5) Retour d’état (pour campagne : suivi via endpoints `/destinataires` et `/historique`).

## Exemples d’appels (auth + payload)
- Headers pour toute requête :
  - `Authorization: Bearer <token_jwt>`
  - `Content-Type: application/json`

- Payload complet pour créer une campagne :
```json
{
  "idEcole": 1,
  "titre": "Réunion parents-professeurs",
  "contenuMarkdown": "Bonjour,\n\nNous vous invitons à une réunion...",
  "importance": "Important",
  "canaux": {
    "push": true,
    "email": true,
    "sms": false,
    "inApp": true
  },
  "rappelAuto": true,
  "sendImmediately": false,
  "planifiedAt": "2025-12-10T10:00:00",
  "expirationAt": "2025-12-15T23:59:59",
  "segments": [
    {
      "typeSegment": "Classe",
      "criteresJson": "{\"idClasse\": 5}"
    }
  ]
}
```

- cURL rapides :
```bash
# Envoyer un message privé
curl -X POST https://localhost:7102/api/Message \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "idExpediteur": 45,
    "idDestinateur": 67,
    "contenuMessage": "Bonjour, réunion à 15h."
  }'

# Créer puis envoyer une campagne
curl -X POST https://localhost:7102/api/Communication \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d @campaign.json

curl -X POST https://localhost:7102/api/Communication/123/envoyer \
  -H "Authorization: Bearer $TOKEN"
```

## Erreurs HTTP et stratégies de retry (synthèse)
- 400 Validation : corriger le payload (messages détaillés).
- 401/403 AuthZ : renouveler le token ou vérifier le rôle.
- 404 Ressource : vérifier l’ID ou le segment.
- 408/429 Timeout/Rate limit : retry avec backoff (ex. 1s, 2s, 4s).
- 500-504 Serveur : log + retry limité (max 3) sur canaux externes (Firebase/SMTP/Twilio).

## Modèle de log minimal par envoi
```json
{
  "channel": "push|email|sms|inapp",
  "target": "userId|email|phone",
  "campaignId": 123,
  "messageId": 456,
  "status": "sent|failed",
  "latencyMs": 230,
  "error": "exception message if any",
  "timestamp": "2025-12-11T12:34:56Z"
}
```

## Exemples Postman (prêts à coller)
- En-têtes globaux : `Authorization: Bearer {{token}}`, `Content-Type: application/json`
- Envoyer un message privé :
```http
POST {{baseUrl}}/api/Message
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "idExpediteur": 45,
  "idDestinateur": 67,
  "contenuMessage": "Bonjour, réunion à 15h."
}
```
- Créer une campagne :
```http
POST {{baseUrl}}/api/Communication
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "idEcole": 1,
  "titre": "Réunion parents-professeurs",
  "contenuMarkdown": "Bonjour,\\n\\nNous vous invitons à une réunion...",
  "importance": "Important",
  "canaux": { "push": true, "email": true, "sms": false, "inApp": true },
  "rappelAuto": true,
  "sendImmediately": false,
  "planifiedAt": "2025-12-10T10:00:00",
  "expirationAt": "2025-12-15T23:59:59",
  "segments": [ { "typeSegment": "Classe", "criteresJson": "{\\\"idClasse\\\": 5}" } ]
}
```
- Envoyer la campagne :
```http
POST {{baseUrl}}/api/Communication/123/envoyer
Authorization: Bearer {{token}}
Content-Type: application/json
```

## Documentation détaillée
Le guide complet se trouve dans `DOCUMENTATION_MODULE_COMMUNICATION.md` (architecture, DTO, payloads, exemples Vue, gestion des erreurs, bonnes pratiques).


