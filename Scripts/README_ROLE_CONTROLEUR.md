# Migration rôle Controleur (contrôle à l'entrée)

Script : [`MIGRATION_ROLE_CONTROLEUR.sql`](MIGRATION_ROLE_CONTROLEUR.sql)

## Contexte

Le rôle JWT **Controleur** (sans accent) est distinct de **Enseignant**, **Caissier** et **Financier** :

| Rôle | Présence | Frais / paiements |
|------|----------|-------------------|
| **Enseignant** | Pointage + notes (pédagogie) | Aucun |
| **Controleur** | Pointage + feuille d'appel (create/read) | Lecture seule (solde, barèmes) |
| **Caissier** | — | Encaissement guichet |
| **Financier** | — | CRUD frais + comptabilité |

Les agents créés avec la fonction `controlleur` / `contrôleur` / `contrôleur des frais` sont mappés sur le rôle `Controleur`.

## Déploiement recommandé

1. **Déployer l'API** avec les changements RBAC et **redémarrer** l'application.
   - `PermissionSeeder.EnsureControleurPermissionsAsync` crée le rôle et assigne les permissions au démarrage.

2. **Production (`knb_db`)** — exécuter le script SQL si des contrôleurs existants ont encore le rôle `Enseignant` ou `Personnel` :
   - Décommenter et exécuter la **prévisualisation** (section 2)
   - Décommenter la **section 3** (attribution Controleur)
   - Optionnel : section 4 pour retirer `Enseignant` aux contrôleurs dédiés

3. **Reconnexion obligatoire** : les utilisateurs contrôleur doivent se déconnecter/reconnecter pour obtenir le claim JWT `Controleur`.

## Rollback

- Réactiver le rôle `Enseignant` ou `Personnel` sur les `UserRoles` concernés (`Statut = 1`)
- Désactiver le rôle `Controleur` (`Statut = 0`) si besoin
- Redéployer une version antérieure de l'API si nécessaire

## Vérifications post-migration

Exécuter [`VERIFY_ROLE_CONTROLEUR.sql`](VERIFY_ROLE_CONTROLEUR.sql) sur `knb_db`.

### SQL (phpMyAdmin)

- Rôle `Controleur` présent avec permissions (Presence.Create/Read, Frais.Read, Paiement.Read)
- Aucune permission `Paiement.Create`, `Frais.Create`, `Presence.Update` sur le rôle
- Agents `controlleur` / `contrôleur` migrés (section 5 du script verify)

### API (Swagger ou front)

- Token JWT contient le rôle `Controleur`
- `GET /api/Eleve/reinscription?matricule=` → **200**
- `GET /api/VuePaiementsFraisParEcole/eleve-matricule/{matricule}` → **200**
- `POST /api/Presence` → **200** ou 400 métier (pas 403)
- `GET /api/Presence/dashboard/ecole/{id}` → **200**
- `PUT /api/Presence/{id}` → **403**
- `POST /api/Paiement` → **403**
- `GET /api/Ecole/{id}/paiement-mobile/wallet/mouvements` → **403**

### Reconnexion

Les utilisateurs contrôleur **doivent se déconnecter et se reconnecter** après migration pour obtenir le claim JWT `Controleur`.

## Checklist déploiement prod

1. Pousser et déployer la version API (RBAC Controleur + durcissement Presence)
2. Redémarrer l'application (seeder `EnsureControleurPermissionsAsync`)
3. Exécuter [`MIGRATION_ROLE_CONTROLEUR.sql`](MIGRATION_ROLE_CONTROLEUR.sql) sections 2–3 si besoin
4. Exécuter [`VERIFY_ROLE_CONTROLEUR.sql`](VERIFY_ROLE_CONTROLEUR.sql)
5. Demander aux contrôleurs de se reconnecter
6. Déployer le front contrôle entrée :
   - Doc API : [`DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md`](../DOCUMENTATION_FRONTEND_ROLE_CONTROLEUR.md)
   - Intégration Vue / Flutter : [`DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md`](../DOCUMENTATION_INTEGRATION_FRONTEND_CONTROLEUR_VUE_FLUTTER.md)

## Multi-rôles

Ne pas cumuler **Controleur + Caissier** sur le même compte sauf cas exceptionnel documenté (union des permissions). Un enseignant qui pointe sa classe ne doit **pas** être migré automatiquement ; seuls les agents avec fonction `controlleur` / `contrôleur`.
