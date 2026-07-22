# 🔔 Guide : Comment vérifier que les notifications push ont été envoyées

## 📊 Résumé de votre test

D'après les logs, voici ce qui s'est passé lors de la création du devoir **ID 11** :

### ✅ Ce qui a fonctionné :
1. **Démarrage de l'envoi des notifications** : ✅
   ```
   📤 Démarrage de l'envoi des notifications pour devoir 11
   ```

2. **Recherche des parents** : ✅
   ```
   🔍 Recherche des parents pour devoir 11, classe 43
   ```

3. **Fin du processus** : ✅
   ```
   ✅ Envoi des notifications terminé pour devoir 11
   ```

### ❌ Problèmes détectés :

1. **Erreur lors de l'envoi aux parents** :
   ```
   [ERR] Erreur lors de l'envoi des notifications aux parents pour devoir 11
   ```

2. **Erreur SignalR** :
   ```
   [ERR] Erreur lors de l'envoi de la notification SignalR pour devoir 11
   ```

---

## 🔍 Comment vérifier les notifications

### Méthode 1 : Script automatique (Recommandé)

```bash
./verifier-notifications-devoir.sh 11
```

Ce script affiche :
- ✅ Les logs de notifications push
- ✅ Le nombre de parents trouvés
- ✅ Les erreurs éventuelles
- ✅ Les détails du devoir

### Méthode 2 : Consultation manuelle des logs

#### A. Fichiers de logs
```bash
# Voir les logs du jour
tail -100 logs/log-$(date +%Y%m%d).txt | grep -i "devoir 11\|notification\|push"

# Voir les erreurs
grep "devoir 11" logs/errors-*.txt
```

#### B. Console de l'application
Si vous avez lancé l'application avec `dotnet run`, les logs s'affichent directement dans la console.

#### C. Logs en temps réel
```bash
# Suivre les logs en temps réel
tail -f logs/log-$(date +%Y%m%d).txt | grep -i "notification\|push\|devoir"
```

---

## 📋 Interprétation des logs

### Logs de succès ✅

```
[INF] 📤 Démarrage de l'envoi des notifications pour devoir X
[INF] 🔍 Recherche des parents pour devoir X, classe Y
[INF] 👥 N parent(s) trouvé(s) pour la classe Y
[INF] 📲 Tentative d'envoi Push au parent Z
[INF] 📱 N token(s) FCM trouvé(s) pour l'utilisateur Z
[INF] ✅ Notification envoyée à l'utilisateur Z. Succès: N/N
[INF] ✅ Push envoyé avec succès au parent Z
[INF] ✅ Notifications multi-canal envoyées à N parent(s)
[INF] ✅ Envoi des notifications terminé pour devoir X
```

### Logs d'erreur ❌

#### Aucun parent trouvé
```
[INF] ⚠️ Aucun parent trouvé pour la classe X
```
**Solution** : Vérifier qu'il y a des élèves inscrits dans la classe avec des tuteurs associés.

#### Aucun token FCM
```
[WRN] ⚠️ Aucun token FCM actif trouvé pour l'utilisateur X
```
**Solution** : L'utilisateur n'a pas enregistré de token FCM (pas d'app mobile connectée).

#### Firebase non initialisé
```
[ERR] ❌ Firebase n'est pas initialisé. Impossible d'envoyer la notification...
```
**Solution** : Vérifier que le fichier `firebase-credentials.json` existe et que Firebase est initialisé au démarrage.

#### Erreur d'envoi
```
[ERR] ❌ Échec Push pour parent X - Détails: [message]
```
**Solution** : Vérifier les détails de l'erreur dans les logs complets.

---

## 🛠️ Diagnostic des problèmes

### Problème 1 : Aucun parent trouvé

**Vérification** :
```sql
-- Vérifier les élèves de la classe
SELECT e.*, t.* 
FROM Eleves e
JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
WHERE e.IdClasse = 43 AND e.Statut = 1;
```

**Solution** : S'assurer qu'il y a des élèves avec des tuteurs dans la classe.

### Problème 2 : Aucun token FCM

**Vérification** :
```sql
-- Vérifier les tokens FCM des parents
SELECT u.IdUtilisateur, u.NomUtilisateur, ud.TokenFCM, ud.Statut
FROM Utilisateurs u
JOIN UserDevices ud ON u.IdUtilisateur = ud.IdUtilisateur
WHERE u.IdTuteur IS NOT NULL
AND ud.Statut = 1;
```

**Solution** : Les parents doivent avoir l'application mobile installée et connectée.

### Problème 3 : Firebase non initialisé

**Vérification** :
- Vérifier que `firebase-credentials.json` existe à la racine du projet
- Vérifier les logs au démarrage de l'application pour voir si Firebase est initialisé

---

## 📱 Vérification côté application mobile

Pour vérifier que les notifications sont bien reçues :

1. **Vérifier les paramètres de notification** dans l'app mobile
2. **Vérifier que l'utilisateur est connecté** avec un compte parent
3. **Vérifier que l'app a les permissions** pour recevoir des notifications push

---

## 🔧 Commandes utiles

```bash
# Voir les logs du jour
cat logs/log-$(date +%Y%m%d).txt | grep -i "devoir 11"

# Compter les notifications envoyées
grep "✅ Push envoyé" logs/log-$(date +%Y%m%d).txt | wc -l

# Voir toutes les erreurs
grep "ERR" logs/log-$(date +%Y%m%d).txt | grep -i "notification\|push"

# Voir les logs en temps réel
tail -f logs/log-$(date +%Y%m%d).txt
```

---

## 📞 Support

Si les notifications ne sont pas envoyées, vérifier dans l'ordre :
1. ✅ Les logs montrent-ils le démarrage de l'envoi ?
2. ✅ Y a-t-il des parents trouvés dans la classe ?
3. ✅ Y a-t-il des tokens FCM actifs pour ces parents ?
4. ✅ Firebase est-il initialisé correctement ?
5. ✅ Y a-t-il des erreurs dans les logs ?

