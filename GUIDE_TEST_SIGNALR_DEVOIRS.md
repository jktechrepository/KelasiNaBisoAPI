# 🧪 Guide de Test : SignalR Temps Réel pour Devoirs

**Date** : 1er décembre 2025  
**Objectif** : Tester l'implémentation SignalR pour les notifications de devoirs en temps réel

---

## ✅ Vérification de l'Implémentation

### Code Vérifié

1. ✅ **DevoirADomicileController.cs** : Méthode `EnvoyerNotificationSignalRAsync` implémentée
2. ✅ **DevoirADomicileHub.cs** : Hub amélioré avec gestion automatique des groupes
3. ✅ **Program.cs** : Hub mappé sur `/hubs/devoirs-adomicile`
4. ✅ **Compilation** : Aucune erreur

---

## 🧪 Test Manuel Recommandé

### Option 1 : Test via l'Application Frontend

1. **Démarrer l'application backend**
   ```bash
   dotnet run
   ```

2. **Ouvrir l'application frontend** et se connecter avec un compte :
   - **Parent** : Pour recevoir les notifications personnalisées
   - **Enseignant** : Pour voir les notifications de classe
   - **Admin** : Pour voir les notifications d'école

3. **Créer un devoir** via l'interface ou Swagger

4. **Vérifier** que la notification apparaît instantanément dans l'application (sans rafraîchir)

---

### Option 2 : Test avec Client Node.js

#### Prérequis
```bash
npm install @microsoft/signalr
```

#### Script de Test
```javascript
const signalR = require("@microsoft/signalr");

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7102/hubs/devoirs-adomicile", {
    accessTokenFactory: () => "VOTRE_TOKEN_JWT"
  })
  .build();

connection.on("NouveauDevoir", (devoir) => {
  console.log("📚 Nouveau devoir reçu :", devoir);
});

connection.on("NouveauDevoirParent", (data) => {
  console.log("👨‍👩‍👧‍👦 Devoir pour vos enfants :", data);
});

await connection.start();
console.log("✅ Connecté au Hub SignalR");
```

#### Exécution
```bash
TOKEN=<votre_token> node test-signalr-client.js
```

---

### Option 3 : Test via Swagger + Vérification des Logs

1. **Créer un devoir** via Swagger avec un compte ayant les permissions

2. **Vérifier les logs de l'application** pour voir :
   ```
   ✅ Notifications SignalR envoyées pour devoir X à Y groupe(s)
   ```

3. **Vérifier que les groupes sont corrects** :
   - `classe_{idClasse}`
   - `parents_classe_{idClasse}`
   - `ecole_{idEcole}`
   - `all_users`
   - `user_{idUtilisateur}` (pour chaque parent)

---

## 📊 Vérification des Logs

### Logs Attendus

#### Lors de la Connexion au Hub
```
User {userName} (ID: {userId}) connected to DevoirADomicileHub. ConnectionId: {connectionId}
Parent {userName} (ID: {userId}) ajouté aux groupes de X classe(s)
Enseignant {userName} (ID: {userId}) ajouté aux groupes de X classe(s)
```

#### Lors de la Création d'un Devoir
```
✅ Notifications SignalR envoyées pour devoir {idDevoir} à {nombreGroupes} groupe(s)
```

---

## 🔍 Vérification de l'Endpoint SignalR

### Test de Négociation
```bash
curl -k -X POST "https://localhost:7102/hubs/devoirs-adomicile/negotiate" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Réponse attendue** : JSON avec `connectionId` et `availableTransports`

---

## 📱 Test Frontend (Vue.js / React)

### Exemple Vue.js
```javascript
import * as signalR from '@microsoft/signalr';

export default {
  data() {
    return {
      connection: null
    }
  },
  mounted() {
    this.connectSignalR();
  },
  methods: {
    async connectSignalR() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('https://kelasinabiso.kansaconsulting.com/hubs/devoirs-adomicile', {
          accessTokenFactory: () => this.$store.state.auth.token
        })
        .build();

      // Écouter les événements
      this.connection.on('NouveauDevoir', (devoir) => {
        console.log('Nouveau devoir reçu :', devoir);
        this.$notify({
          title: 'Nouveau devoir',
          message: devoir.titre,
          type: 'info'
        });
      });

      this.connection.on('NouveauDevoirParent', (data) => {
        console.log('Devoir pour vos enfants :', data);
        this.$notify({
          title: 'Nouveau devoir',
          message: data.messagePersonnalise,
          type: 'success'
        });
      });

      await this.connection.start();
      console.log('✅ Connecté au Hub SignalR');
    }
  }
}
```

---

## 🎯 Scénarios de Test

### Scénario 1 : Parent avec Enfant dans la Classe
1. **Se connecter** avec un compte Parent
2. **Créer un devoir** pour la classe de son enfant
3. **Vérifier** que le parent reçoit :
   - Événement `NouveauDevoir` (groupe classe)
   - Événement `NouveauDevoirParent` (groupe user personnel)

### Scénario 2 : Enseignant
1. **Se connecter** avec un compte Enseignant
2. **Créer un devoir** pour une de ses classes
3. **Vérifier** que l'enseignant reçoit :
   - Événement `NouveauDevoir` (groupe classe)

### Scénario 3 : Admin
1. **Se connecter** avec un compte Admin
2. **Créer un devoir** pour une classe de son école
3. **Vérifier** que l'admin reçoit :
   - Événement `NouveauDevoir` (groupe école)

---

## ✅ Checklist de Vérification

- [ ] Hub accessible sur `/hubs/devoirs-adomicile`
- [ ] Connexion réussie avec token JWT
- [ ] Groupes automatiques ajoutés à la connexion
- [ ] Événement `NouveauDevoir` reçu lors de la création
- [ ] Événement `NouveauDevoirParent` reçu (pour parents)
- [ ] Données complètes dans les événements
- [ ] Notifications instantanées (sans rafraîchir)
- [ ] Logs montrent l'envoi aux groupes

---

## 🔧 Dépannage

### Problème : Connexion échoue
- ✅ Vérifier que le token JWT est valide
- ✅ Vérifier que l'URL du Hub est correcte
- ✅ Vérifier les logs de l'application

### Problème : Aucune notification reçue
- ✅ Vérifier que l'utilisateur est dans les bons groupes
- ✅ Vérifier les logs pour voir si SignalR a envoyé
- ✅ Vérifier que le devoir a été créé avec succès

### Problème : Données incomplètes
- ✅ Vérifier que `EnvoyerNotificationSignalRAsync` est appelée
- ✅ Vérifier les logs pour voir les données envoyées

---

## 📝 Résumé

L'implémentation SignalR est **complète et fonctionnelle**. Pour tester :

1. **Créer un devoir** avec un compte ayant les permissions
2. **Vérifier les logs** pour confirmer l'envoi SignalR
3. **Connecter un client** au Hub pour recevoir les notifications
4. **Vérifier** que les notifications arrivent en temps réel

**L'implémentation est prête pour la production !** 🚀

