# Migration rôle Caissier (guichet)

Script : [`MIGRATION_ROLE_CAISSIER.sql`](MIGRATION_ROLE_CAISSIER.sql)

## Contexte

Le rôle JWT **Caissier** est distinct de **Financier** :

| Rôle | Périmètre |
|------|-----------|
| **Caissier** | Encaissement guichet, PayIn Moko, lecture élèves/frais/paiements |
| **Financier** | Comptabilité complète, gestion des frais, trésorerie Moko (wallet, payouts) |

Les agents créés avec la fonction `caissier` / `caissière` sont désormais mappés sur le rôle `Caissier` (plus `Financier`).

## Déploiement recommandé

1. **Déployer l'API** avec les changements RBAC et **redémarrer** l'application.
   - `PermissionSeeder.EnsureCaissierPermissionsAsync` crée le rôle et assigne les permissions au démarrage.

2. **Production (`knb_db`)** — exécuter le script SQL si des caissiers existants ont encore le rôle `Financier` :
   - Décommenter et exécuter la **prévisualisation** (section 2)
   - Décommenter la **section 3** (attribution Caissier)
   - Optionnel : section 4 pour retirer `Financier` aux caissiers

3. **Reconnexion obligatoire** : les utilisateurs caissier doivent se déconnecter/reconnecter pour obtenir le claim JWT `Caissier`.

## Rollback

- Réactiver le rôle `Financier` sur les `UserRoles` concernés (`Statut = 1`)
- Désactiver le rôle `Caissier` (`Statut = 0`) si besoin
- Redéployer une version antérieure de l'API si nécessaire

## Vérifications post-migration

Exécuter [`VERIFY_ROLE_CAISSIER.sql`](VERIFY_ROLE_CAISSIER.sql) sur `knb_db`.

### SQL (phpMyAdmin)

- Rôle `Caissier` présent avec permissions (Paiement.Create, Frais.Read, pas Frais.Create)
- Aucun agent `caissier` resté sur `Financier` seul (section 4 du script)

### API (Swagger ou front)

- Token JWT contient le rôle `Caissier`
- `GET /api/Dashboard/caissier?idEcole={id}` → **200**
- `POST /api/MokoAfrika/payin/frais-scolaire` → 200 ou 400 métier (pas 403)
- `GET /api/Ecole/{id}/paiement-mobile` → 200
- `GET /api/Ecole/{id}/paiement-mobile/wallet/mouvements` → **403**
- `POST /api/Paiement` avec `modePaiement: "Mobile Money"` → **400** (utiliser Moko PayIn)

### Reconnexion

Les utilisateurs caissier **doivent se déconnecter et se reconnecter** après migration pour obtenir le claim JWT `Caissier`.

## Checklist déploiement prod

1. Pousser et déployer la version API (RBAC Caissier + Dashboard caissier + blocage MM)
2. Redémarrer l'application (seeder `EnsureCaissierPermissionsAsync`)
3. Exécuter [`MIGRATION_ROLE_CAISSIER.sql`](MIGRATION_ROLE_CAISSIER.sql) sections 2–3 si besoin
4. Exécuter [`VERIFY_ROLE_CAISSIER.sql`](VERIFY_ROLE_CAISSIER.sql)
5. Demander aux caissiers de se reconnecter
6. Déployer le front guichet :
   - Doc API : [`DOCUMENTATION_FRONTEND_ROLE_CAISSIER.md`](../DOCUMENTATION_FRONTEND_ROLE_CAISSIER.md)
   - Intégration Vue / Flutter : [`DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md`](../DOCUMENTATION_INTEGRATION_FRONTEND_CAISSIER_VUE_FLUTTER.md)

## Multi-rôles

Si un utilisateur cumule **Caissier + Financier**, le JWT contient les deux claims et le périmètre est l'union des deux rôles. Éviter cette combinaison sauf cas exceptionnel documenté.
