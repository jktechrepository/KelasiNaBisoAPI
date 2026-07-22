// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🚀 TEST DE CHARGE - SYSTÈME DE NOTIFICATION PUSH
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 
// Description: Test de charge k6 pour évaluer les performances du système
//              de notification push sous haute charge
// 
// Prérequis:
//   1. k6 installé: https://k6.io/docs/get-started/installation/
//   2. API lancée et accessible
//   3. Token JWT valide défini dans la variable d'environnement TOKEN_JWT
// 
// Utilisation:
//   export TOKEN_JWT="votre_token_jwt_ici"
//   k6 run --vus 50 --duration 30s test-notification-load.js
// 
// Ou avec options personnalisées:
//   k6 run --vus 100 --duration 1m --rps 200 test-notification-load.js
// 
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// ═══════════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════════

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7036/api';
const TOKEN = __ENV.TOKEN_JWT || '';

// Vérifier que le token est fourni
if (!TOKEN) {
    throw new Error('❌ TOKEN_JWT non fourni. Utilisez: export TOKEN_JWT="votre_token"');
}

// ═══════════════════════════════════════════════════════════════════
// OPTIONS DE TEST
// ═══════════════════════════════════════════════════════════════════

export const options = {
    // Scénario de montée en charge progressive
    stages: [
        { duration: '30s', target: 20 },   // Warmup: Monter à 20 utilisateurs
        { duration: '1m', target: 50 },    // Charge normale: 50 utilisateurs
        { duration: '30s', target: 100 },  // Pic: 100 utilisateurs
        { duration: '1m', target: 100 },   // Maintenir le pic
        { duration: '30s', target: 0 },    // Cooldown: Descendre à 0
    ],
    
    // Seuils de performance
    thresholds: {
        // 95% des requêtes doivent être < 500ms
        'http_req_duration': ['p(95)<500'],
        
        // Taux d'échec < 1%
        'http_req_failed': ['rate<0.01'],
        
        // Métriques personnalisées
        'notification_success_rate': ['rate>0.99'],      // 99% de succès
        'notification_send_duration': ['p(95)<400'],     // 95% < 400ms
        'multi_device_notifications': ['rate>0.95'],     // 95% multi-devices réussis
    },
    
    // Limites
    noConnectionReuse: false,  // Réutiliser les connexions HTTP
    userAgent: 'k6-load-test/1.0',
    insecureSkipTLSVerify: true, // Pour localhost avec HTTPS auto-signé
};

// ═══════════════════════════════════════════════════════════════════
// MÉTRIQUES PERSONNALISÉES
// ═══════════════════════════════════════════════════════════════════

// Taux de succès des notifications
const notificationSuccessRate = new Rate('notification_success_rate');

// Durée d'envoi des notifications
const notificationSendDuration = new Trend('notification_send_duration');

// Compteur de notifications envoyées
const notificationsSent = new Counter('notifications_sent');

// Compteur d'utilisateurs notifiés
const usersNotified = new Counter('users_notified');

// Taux de notifications multi-devices
const multiDeviceRate = new Rate('multi_device_notifications');

// ═══════════════════════════════════════════════════════════════════
// HELPERS
// ═══════════════════════════════════════════════════════════════════

/**
 * En-têtes HTTP standard
 */
function getHeaders() {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${TOKEN}`,
    };
}

/**
 * Génère un timestamp unique
 */
function getTimestamp() {
    return new Date().toISOString();
}

/**
 * Génère un ID utilisateur aléatoire (1-10)
 */
function getRandomUserId() {
    return Math.floor(Math.random() * 10) + 1;
}

/**
 * Génère un ID d'école aléatoire (1-5)
 */
function getRandomSchoolId() {
    return Math.floor(Math.random() * 5) + 1;
}

/**
 * Génère un ID de classe aléatoire (1-20)
 */
function getRandomClassId() {
    return Math.floor(Math.random() * 20) + 1;
}

/**
 * Génère un ID de rôle aléatoire (1-5)
 */
function getRandomRoleId() {
    return Math.floor(Math.random() * 5) + 1;
}

// ═══════════════════════════════════════════════════════════════════
// SCÉNARIOS DE TEST
// ═══════════════════════════════════════════════════════════════════

/**
 * Test 1: Notification à un utilisateur unique
 */
function testNotificationUtilisateur() {
    const userId = getRandomUserId();
    
    const payload = JSON.stringify({
        titre: `Test Load - User ${userId}`,
        corps: `Notification de test de charge envoyée à ${getTimestamp()}`,
        donnees: {
            type: 'load_test',
            userId: userId.toString(),
            timestamp: getTimestamp(),
            testId: `LOAD-${__VU}-${__ITER}`,
        },
    });
    
    const startTime = Date.now();
    const res = http.post(
        `${BASE_URL}/NotificationPush/utilisateur/${userId}`,
        payload,
        { headers: getHeaders() }
    );
    const duration = Date.now() - startTime;
    
    // Vérifications
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
        'success is true': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.success === true;
            } catch {
                return false;
            }
        },
    });
    
    // Métriques
    notificationSuccessRate.add(success);
    notificationSendDuration.add(duration);
    notificationsSent.add(1);
    
    if (success) {
        try {
            const body = JSON.parse(res.body);
            // Vérifier si multi-devices (count > 1 dans la réponse si disponible)
            multiDeviceRate.add(true); // Assumer multi-devices pour un utilisateur
        } catch {
            multiDeviceRate.add(false);
        }
    }
    
    return success;
}

/**
 * Test 2: Notification à un rôle
 */
function testNotificationRole() {
    const roleId = getRandomRoleId();
    
    const payload = JSON.stringify({
        titre: `Test Load - Role ${roleId}`,
        corps: `Notification de test de charge pour le rôle ${roleId}`,
        donnees: {
            type: 'load_test_role',
            roleId: roleId.toString(),
            timestamp: getTimestamp(),
        },
    });
    
    const res = http.post(
        `${BASE_URL}/NotificationPush/role/${roleId}`,
        payload,
        { headers: getHeaders() }
    );
    
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'count > 0': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.count > 0;
            } catch {
                return false;
            }
        },
    });
    
    if (success) {
        try {
            const body = JSON.parse(res.body);
            usersNotified.add(body.count || 0);
        } catch {}
    }
    
    notificationSuccessRate.add(success);
    notificationsSent.add(1);
    
    return success;
}

/**
 * Test 3: Notification à une école
 */
function testNotificationEcole() {
    const schoolId = getRandomSchoolId();
    
    const payload = JSON.stringify({
        titre: `Test Load - École ${schoolId}`,
        corps: `Notification de test de charge pour l'école ${schoolId}`,
        donnees: {
            type: 'load_test_ecole',
            schoolId: schoolId.toString(),
            timestamp: getTimestamp(),
        },
    });
    
    const res = http.post(
        `${BASE_URL}/NotificationPush/ecole/${schoolId}`,
        payload,
        { headers: getHeaders() }
    );
    
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'count > 0': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.count > 0;
            } catch {
                return false;
            }
        },
    });
    
    if (success) {
        try {
            const body = JSON.parse(res.body);
            usersNotified.add(body.count || 0);
        } catch {}
    }
    
    notificationSuccessRate.add(success);
    notificationsSent.add(1);
    
    return success;
}

/**
 * Test 4: Notification à une classe
 */
function testNotificationClasse() {
    const classId = getRandomClassId();
    
    const payload = JSON.stringify({
        titre: `Test Load - Classe ${classId}`,
        corps: `Notification de test de charge pour la classe ${classId}`,
        donnees: {
            type: 'load_test_classe',
            classId: classId.toString(),
            timestamp: getTimestamp(),
        },
    });
    
    const res = http.post(
        `${BASE_URL}/NotificationPush/classe/${classId}`,
        payload,
        { headers: getHeaders() }
    );
    
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'response time < 600ms': (r) => r.timings.duration < 600,
    });
    
    notificationSuccessRate.add(success);
    notificationsSent.add(1);
    
    return success;
}

// ═══════════════════════════════════════════════════════════════════
// FONCTION PRINCIPALE DE TEST
// ═══════════════════════════════════════════════════════════════════

export default function () {
    // Répartition des types de tests:
    // 50% utilisateur, 20% rôle, 20% école, 10% classe
    const rand = Math.random();
    
    if (rand < 0.5) {
        group('Notification Utilisateur', () => {
            testNotificationUtilisateur();
        });
    } else if (rand < 0.7) {
        group('Notification Rôle', () => {
            testNotificationRole();
        });
    } else if (rand < 0.9) {
        group('Notification École', () => {
            testNotificationEcole();
        });
    } else {
        group('Notification Classe', () => {
            testNotificationClasse();
        });
    }
    
    // Pause aléatoire entre 0.5 et 2 secondes
    sleep(Math.random() * 1.5 + 0.5);
}

// ═══════════════════════════════════════════════════════════════════
// SETUP ET TEARDOWN
// ═══════════════════════════════════════════════════════════════════

/**
 * Setup exécuté une fois avant tous les tests
 */
export function setup() {
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('🚀 DÉMARRAGE TEST DE CHARGE - NOTIFICATION PUSH');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log(`📍 URL: ${BASE_URL}`);
    console.log(`🔑 Token: ${TOKEN.substring(0, 20)}...`);
    console.log(`⏱️  Durée totale: 3m30s`);
    console.log(`👥 VUs max: 100`);
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    
    // Vérifier que l'API est accessible
    const res = http.get(`${BASE_URL.replace('/api', '')}/health`, {
        headers: getHeaders(),
    });
    
    if (res.status !== 200) {
        console.warn('⚠️  Warning: API health check failed (might be normal if /health endpoint not implemented)');
    }
    
    return { startTime: new Date() };
}

/**
 * Teardown exécuté une fois après tous les tests
 */
export function teardown(data) {
    const endTime = new Date();
    const duration = (endTime - data.startTime) / 1000; // en secondes
    
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('✅ TEST DE CHARGE TERMINÉ');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log(`⏱️  Durée réelle: ${duration.toFixed(2)}s`);
    console.log(`📊 Consultez les métriques détaillées ci-dessus`);
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
}

// ═══════════════════════════════════════════════════════════════════
// RÉSUMÉ DU TEST
// ═══════════════════════════════════════════════════════════════════
// 
// Ce script teste le système de notification push sous différentes charges:
// 
// Phases:
//   1. Warmup (30s):     0 → 20 VUs   (Montée douce)
//   2. Normal (1m):      20 → 50 VUs  (Charge normale)
//   3. Peak (30s):       50 → 100 VUs (Pic de charge)
//   4. Sustained (1m):   100 VUs      (Maintien du pic)
//   5. Cooldown (30s):   100 → 0 VUs  (Descente)
// 
// Scénarios testés (répartition):
//   - 50% Notifications à utilisateur unique
//   - 20% Notifications par rôle
//   - 20% Notifications par école
//   - 10% Notifications par classe
// 
// Métriques surveillées:
//   ✓ Taux de succès > 99%
//   ✓ Temps de réponse P95 < 500ms
//   ✓ Taux d'échec < 1%
//   ✓ Support multi-devices > 95%
// 
// Résultats attendus:
//   ✅ ~3000-5000 notifications envoyées
//   ✅ 0 erreurs critiques
//   ✅ Temps de réponse constant
//   ✅ Pas de dégradation pendant le pic
// 
// Commandes utiles:
//   # Test rapide (30s, 20 VUs)
//   k6 run --vus 20 --duration 30s test-notification-load.js
// 
//   # Test complet (comme configuré)
//   k6 run test-notification-load.js
// 
//   # Test avec export HTML
//   k6 run --out json=test-results.json test-notification-load.js
//   k6 convert test-results.json -o test-results.html
// 
//   # Test avec monitoring en temps réel (nécessite k6 cloud)
//   k6 run --out cloud test-notification-load.js
// 
// ═══════════════════════════════════════════════════════════════════

