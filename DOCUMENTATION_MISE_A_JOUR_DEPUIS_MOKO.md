# Documentation des mises à jour depuis l'implémentation MOKO

Date: 2026-07-09  
Projet: `KelasiNaBisoAPI`

## 1) Test de non-régression exécuté avant documentation

Commande lancée:

```bash
dotnet test
```

### Résultat global

- Tests unitaires: **OK** (`30/30` réussis)
- Tests d'intégration: **KO** (`10` échecs, `1` succès, `7` ignorés)
- Conclusion: la non-régression n'est **pas validée** côté intégration.

### Principales causes observées

1. **Erreur d'hébergement de l'application de test**
   - `System.InvalidOperationException: The entry point exited without ever building an IHost.`
   - Impacte `ClasseControllerTests`.

2. **Erreur middleware RateLimit**
   - `System.NullReferenceException` dans `AspNetCoreRateLimit.IpAddressUtil.ParseIp`.
   - Effet de bord: plusieurs scénarios attendus en `401`/`200` passent en `500`.

3. **Avertissements techniques à traiter (hors blocage immédiat des tests unitaires)**
   - Framework cible `net6.0` en fin de support.
   - Vulnérabilité connue sur `AutoMapper 15.0.1` (NU1903).
   - Conflits de versions EF Core Relational (`6.0.7` vs `6.0.25`).

## 2) Synthèse des mises à jour depuis MOKO (état applicatif)

> Note: l'historique Git local disponible est très limité; cette synthèse est basée sur les composants MOKO présents dans le code et la documentation du projet.

### 2.1 Intégration paiement MOKO (backend)

Les capacités suivantes sont en place:

- Gestion du gateway MOKO (PayIn `debit`, PayOut `credit`, check statut)
- Orchestration métier des paiements
- Calcul des frais par opérateur
- Gestion callback/webhook MOKO
- Gestion des wallets écoles et mouvements
- File d'attente/retry de payout

Composants clés:

- `Services/MokoAfrika/MokoAfrikaService.cs`
- `Services/MokoAfrika/PaiementMokoOrchestrator.cs`
- `Services/MokoAfrika/MokoFeeCalculator.cs`
- `Services/MokoAfrika/MokoWalletService.cs`
- `Services/MokoAfrika/MokoCallbackHandler.cs`
- `Services/MokoAfrika/MokoPayoutWorker.cs`
- `Services/MokoAfrika/MokoAfrikaGatewayClient.cs`
- `Controllers/MokoAfrikaController.cs`
- `Controllers/EcolePaiementMobileController.cs`

### 2.2 Modèle de données / persistance

Entités MOKO ajoutées et reliées au contexte:

- `Models/TransactionMoko.cs`
- `Models/FilePayoutMoko.cs`
- `Models/EcoleWalletMouvement.cs`
- `Models/Enums/MokoEnums.cs`
- `Models/DTOs/MokoAfrika/MokoAfrikaDtos.cs`
- `Data/KelasiNaBisoDbContext.cs`

Migration associée:

- `Migrations/20260704123906_AddMokoAfrikaPaymentEntities.cs`
- `Migrations/KelasiNaBisoDbContextModelSnapshot.cs`

Scripts SQL de support:

- `Scripts/migration-moko-afrika-manual.sql`
- `Scripts/verification-moko-migration.sql`

### 2.3 Configuration et démarrage

Paramétrage centralisé MOKO:

- `Services/MokoAfrika/MokoSettings.cs`
- `appsettings.json`
- `Program.cs`

Points couverts:

- Clés marchand (merchant id, secret, code)
- Bascule test/production
- Callback URL
- Profil statique client pour payload MOKO

### 2.4 Exposition API et usages fonctionnels

Cas d'usage supportés:

- Paiement frais scolaires via MOKO
- Estimation des frais avant paiement
- Consultation statut transaction
- Configuration paiement mobile par école
- Gestion bénéficiaires payout
- Suivi transactions / wallets / payouts
- Relance payout (retry)

### 2.5 Documentation et exploitation

Documentation technique présente:

- `integration-moko-afrika-projet-tiers.md`
- `DOCUMENTATION_FRONTEND_PAIEMENT_MOKO.md`
- `DEPLOIEMENT_DEV_KNB.md`

Objectif couvert:

- Guide intégration tiers
- Guide frontend (flux parent, école, super-admin)
- Guide déploiement/migration en environnement dev

## 3) Recommandations immédiates

1. Corriger les tests d'intégration (`ClasseControllerTests`) avant toute validation de release.
2. Sécuriser et aligner les dépendances (`AutoMapper`, EF Core, packages net9 sur net6).
3. Planifier migration de runtime vers `net8.0` (ou supérieur) pour sortie du support `net6.0`.
4. Rejouer `dotnet test` après correctifs et archiver le nouveau rapport de non-régression.

---

Document généré automatiquement à partir de l'état actuel du dépôt.
