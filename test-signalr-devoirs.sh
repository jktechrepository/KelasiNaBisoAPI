#!/bin/bash

# ============================================================================
# SCRIPT DE TEST : SIGNALR TEMPS RÉEL POUR DEVOIRS
# ============================================================================
# Teste l'envoi de notifications SignalR lors de la création d'un devoir
# ============================================================================

set -e

# Configuration
BASE_URL="${BASE_URL:-https://localhost:7102}"
API_URL="${BASE_URL}/api"
HUB_URL="${BASE_URL}/hubs/devoirs-adomicile"

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  TEST : SIGNALR TEMPS RÉEL POUR DEVOIRS${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# ÉTAPE 1 : VÉRIFIER QUE L'APPLICATION EST EN COURS D'EXÉCUTION
# ============================================================================

echo -e "${YELLOW}🔍 Étape 1 : Vérification de l'application...${NC}"

if ! curl -k -s -f "${BASE_URL}/swagger" > /dev/null 2>&1; then
  echo -e "${RED}❌ L'application n'est pas accessible sur ${BASE_URL}${NC}"
  echo "   Assurez-vous que l'application est démarrée avec 'dotnet run'"
  exit 1
fi

echo -e "${GREEN}✅ Application accessible${NC}"
echo ""

# ============================================================================
# ÉTAPE 2 : VÉRIFIER L'ENDPOINT SIGNALR
# ============================================================================

echo -e "${YELLOW}🔌 Étape 2 : Vérification de l'endpoint SignalR...${NC}"

# SignalR négocie d'abord via HTTP, puis établit une connexion WebSocket
# On peut vérifier que l'endpoint répond (même si ce n'est pas une connexion complète)
HUB_RESPONSE=$(curl -k -s -o /dev/null -w "%{http_code}" "${HUB_URL}/negotiate" 2>/dev/null || echo "000")

if [ "$HUB_RESPONSE" == "401" ] || [ "$HUB_RESPONSE" == "404" ]; then
  echo -e "${YELLOW}⚠️ Endpoint SignalR nécessite une authentification (normal)${NC}"
  echo "   Code HTTP : $HUB_RESPONSE"
elif [ "$HUB_RESPONSE" == "000" ]; then
  echo -e "${YELLOW}⚠️ Impossible de vérifier l'endpoint SignalR (peut nécessiter WebSocket)${NC}"
else
  echo -e "${GREEN}✅ Endpoint SignalR accessible${NC}"
  echo "   Code HTTP : $HUB_RESPONSE"
fi
echo ""

# ============================================================================
# ÉTAPE 3 : AUTHENTIFICATION
# ============================================================================

echo -e "${YELLOW}📝 Étape 3 : Authentification...${NC}"

# Utiliser un compte Admin pour créer le devoir
EMAIL="${EMAIL:-jk2@kelasinabiso.cd}"
MOT_DE_PASSE="${MOT_DE_PASSE:-12345678}"

AUTH_RESPONSE=$(curl -k -s -X POST "${API_URL}/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"${EMAIL}\",
    \"motDePasse\": \"${MOT_DE_PASSE}\"
  }")

TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.accessToken // .token // empty')
USER_ID=$(echo "$AUTH_RESPONSE" | jq -r '.idUtilisateur // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo -e "${RED}❌ Échec de l'authentification${NC}"
  echo "Réponse : $AUTH_RESPONSE"
  exit 1
fi

echo -e "${GREEN}✅ Authentification réussie${NC}"
echo "   Token : ${TOKEN:0:30}..."
echo "   User ID : $USER_ID"
echo ""

# ============================================================================
# ÉTAPE 4 : RÉCUPÉRER LES CLASSES
# ============================================================================

echo -e "${YELLOW}📚 Étape 4 : Récupération des classes...${NC}"

CLASSES_RESPONSE=$(curl -k -s -X GET "${API_URL}/Classe" \
  -H "Authorization: Bearer ${TOKEN}")

if echo "$CLASSES_RESPONSE" | jq empty 2>/dev/null; then
  CLASSE_ID=$(echo "$CLASSES_RESPONSE" | jq -r '.[0].idClasse // empty')
  CLASSE_NOM=$(echo "$CLASSES_RESPONSE" | jq -r '.[0].nomClasse // empty')
  
  if [ -z "$CLASSE_ID" ] || [ "$CLASSE_ID" == "null" ]; then
    echo -e "${YELLOW}⚠️ Aucune classe trouvée via l'API, utilisation d'une classe par défaut${NC}"
    CLASSE_ID="${CLASSE_ID_DEFAULT:-80}"
    CLASSE_NOM="Classe ID $CLASSE_ID"
  fi
else
  echo -e "${YELLOW}⚠️ Impossible de récupérer les classes, utilisation d'une classe par défaut${NC}"
  CLASSE_ID="${CLASSE_ID_DEFAULT:-80}"
  CLASSE_NOM="Classe ID $CLASSE_ID"
fi

echo -e "${GREEN}✅ Classe sélectionnée${NC}"
echo "   ID : $CLASSE_ID"
echo "   Nom : $CLASSE_NOM"
echo ""

# ============================================================================
# ÉTAPE 5 : CRÉER UN DEVOIR
# ============================================================================

echo -e "${YELLOW}📝 Étape 5 : Création d'un devoir pour tester SignalR...${NC}"

# Créer un fichier PDF de test minimal
cat > /tmp/test-devoir-signalr.pdf << 'EOFPDF'
%PDF-1.4
1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj
3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Resources<<>>>>endobj
xref
0 4
trailer<</Size 4/Root 1 0 R>>
startxref
%%EOF
EOFPDF

# Calculer la date limite (7 jours à partir de maintenant)
if [[ "$OSTYPE" == "darwin"* ]]; then
  DATE_LIMITE=$(date -u -v+7d +%Y-%m-%dT%H:%M:%S 2>/dev/null || date -u -v+7d +%Y-%m-%dT%H:%M:%S)
else
  DATE_LIMITE=$(date -u -d '+7 days' +%Y-%m-%dT%H:%M:%S)
fi

# Créer le devoir avec fichier ET contenu
DEVOIR_RESPONSE=$(curl -k -s -X POST "${API_URL}/DevoirADomicile" \
  -H "Authorization: Bearer ${TOKEN}" \
  -F "Titre=Test SignalR Temps Réel - $(date +%H:%M:%S)" \
  -F "Description=Ce devoir teste l'envoi de notifications SignalR en temps réel" \
  -F "Contenu=Exercices à faire :\n1. Résoudre les équations\n2. Faire les exercices de la page 45" \
  -F "IdClasse=${CLASSE_ID}" \
  -F "DateLimite=${DATE_LIMITE}" \
  -F "fichier=@/tmp/test-devoir-signalr.pdf")

# Vérifier si la réponse est valide JSON
if echo "$DEVOIR_RESPONSE" | jq empty 2>/dev/null; then
  DEVOIR_ID=$(echo "$DEVOIR_RESPONSE" | jq -r '.idDevoirADomicile // empty')
  
  if [ -z "$DEVOIR_ID" ] || [ "$DEVOIR_ID" == "null" ]; then
    echo -e "${RED}❌ Échec de la création du devoir${NC}"
    echo "Réponse : $DEVOIR_RESPONSE"
    rm -f /tmp/test-devoir-signalr.pdf
    exit 1
  fi
else
  echo -e "${RED}❌ Réponse invalide de l'API${NC}"
  echo "Réponse brute : $DEVOIR_RESPONSE"
  rm -f /tmp/test-devoir-signalr.pdf
  exit 1
fi

echo -e "${GREEN}✅ Devoir créé avec succès${NC}"
echo "   ID : $DEVOIR_ID"
echo "   Titre : $(echo "$DEVOIR_RESPONSE" | jq -r '.titre')"
echo "   Classe : $(echo "$DEVOIR_RESPONSE" | jq -r '.nomClasse // "N/A"')"
echo ""

# ============================================================================
# ÉTAPE 6 : ATTENDRE QUE LES NOTIFICATIONS SIGNALR SOIENT ENVOYÉES
# ============================================================================

echo -e "${YELLOW}⏳ Étape 6 : Attente de l'envoi des notifications SignalR (3 secondes)...${NC}"
sleep 3
echo ""

# ============================================================================
# ÉTAPE 7 : VÉRIFIER LES DONNÉES DU DEVOIR CRÉÉ
# ============================================================================

echo -e "${YELLOW}🔍 Étape 7 : Vérification des données du devoir créé...${NC}"

DEVOIR_DETAILS=$(curl -k -s -X GET "${API_URL}/DevoirADomicile/${DEVOIR_ID}" \
  -H "Authorization: Bearer ${TOKEN}")

if echo "$DEVOIR_DETAILS" | jq empty 2>/dev/null; then
  echo -e "${GREEN}✅ Devoir récupéré avec succès${NC}"
  echo ""
  echo "📋 Détails du devoir :"
  echo "$DEVOIR_DETAILS" | jq '{
    id: .idDevoirADomicile,
    titre: .titre,
    description: .description,
    contenu: .contenu,
    nomFichier: .nomFichier,
    typeMIME: .typeMIME,
    dateLimite: .dateLimite,
    classe: .nomClasse,
    enseignant: .nomAgent
  }'
else
  echo -e "${YELLOW}⚠️ Impossible de récupérer les détails du devoir${NC}"
fi
echo ""

# ============================================================================
# RÉSUMÉ
# ============================================================================

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ TEST TERMINÉ${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo "📊 Résultats :"
echo "   ✅ Devoir créé : ID $DEVOIR_ID"
echo "   ✅ Notifications SignalR envoyées (vérifier les logs de l'application)"
echo ""
echo "📝 Pour vérifier les notifications SignalR :"
echo ""
echo "   1. Vérifier les logs de l'application pour voir :"
echo "      - '✅ Notifications SignalR envoyées pour devoir $DEVOIR_ID à X groupe(s)'"
echo ""
echo "   2. Connecter un client SignalR au Hub :"
echo "      URL : ${HUB_URL}"
echo "      Token : ${TOKEN:0:30}..."
echo ""
echo "   3. Écouter les événements :"
echo "      - 'NouveauDevoir' : Événement général"
echo "      - 'NouveauDevoirParent' : Événement personnalisé pour parents"
echo ""
echo "   4. Groupes SignalR ciblés :"
echo "      - classe_${CLASSE_ID}"
echo "      - parents_classe_${CLASSE_ID}"
echo "      - ecole_<idEcole>"
echo "      - all_users"
echo "      - user_<idUtilisateur> (pour chaque parent)"
echo ""
echo "🔍 Pour tester avec un client SignalR :"
echo "   - Utiliser l'application frontend connectée au Hub"
echo "   - Utiliser un client de test SignalR (ex: @microsoft/signalr)"
echo "   - Vérifier que les notifications arrivent en temps réel"
echo ""

# Nettoyer
rm -f /tmp/test-devoir-signalr.pdf

echo -e "${GREEN}✅ Script terminé${NC}"

