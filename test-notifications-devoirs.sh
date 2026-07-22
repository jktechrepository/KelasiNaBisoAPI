#!/bin/bash

# ============================================================================
# SCRIPT DE TEST : NOTIFICATIONS MULTI-CANAL POUR DEVOIRS
# ============================================================================
# Teste l'envoi de notifications (Push + SMS + Email) aux parents
# lors de la création d'un devoir
# ============================================================================

set -e

# Configuration
BASE_URL="${BASE_URL:-https://localhost:7102}"
API_URL="${BASE_URL}/api"

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  TEST : NOTIFICATIONS MULTI-CANAL POUR DEVOIRS${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# ÉTAPE 1 : AUTHENTIFICATION (Enseignant)
# ============================================================================

echo -e "${YELLOW}📝 Étape 1 : Authentification...${NC}"

# Utiliser un compte Admin pour créer le devoir (peut publier pour toutes les classes)
EMAIL_ENSEIGNANT="${EMAIL_ENSEIGNANT:-jk2@kelasinabiso.cd}"
MOT_DE_PASSE="${MOT_DE_PASSE:-12345678}"

AUTH_RESPONSE=$(curl -k -s -X POST "${API_URL}/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"${EMAIL_ENSEIGNANT}\",
    \"motDePasse\": \"${MOT_DE_PASSE}\"
  }")

TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.accessToken // .token // empty')
USER_ID=$(echo "$AUTH_RESPONSE" | jq -r '.idUtilisateur // empty')
AGENT_ID=$(echo "$AUTH_RESPONSE" | jq -r '.idAgent // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo -e "${RED}❌ Échec de l'authentification${NC}"
  echo "Réponse : $AUTH_RESPONSE"
  exit 1
fi

echo -e "${GREEN}✅ Authentification réussie${NC}"
echo "   Token : ${TOKEN:0:20}..."
echo "   User ID : $USER_ID"
echo "   Agent ID : $AGENT_ID"
echo ""

# ============================================================================
# ÉTAPE 2 : RÉCUPÉRER LES CLASSES DISPONIBLES
# ============================================================================

echo -e "${YELLOW}📚 Étape 2 : Récupération des classes...${NC}"

CLASSES_RESPONSE=$(curl -k -s -X GET "${API_URL}/Classe" \
  -H "Authorization: Bearer ${TOKEN}")

# Essayer de trouver une classe avec des élèves
CLASSE_ID=""
CLASSE_NOM=""

# Parcourir les classes pour trouver une avec des élèves
if echo "$CLASSES_RESPONSE" | jq empty 2>/dev/null; then
  NOMBRE_CLASSES=$(echo "$CLASSES_RESPONSE" | jq '. | length // 0')
  
  for i in $(seq 0 $((NOMBRE_CLASSES - 1))); do
    TEST_CLASSE_ID=$(echo "$CLASSES_RESPONSE" | jq -r ".[$i].idClasse // empty")
    TEST_CLASSE_NOM=$(echo "$CLASSES_RESPONSE" | jq -r ".[$i].nomClasse // empty")
    
    if [ -n "$TEST_CLASSE_ID" ] && [ "$TEST_CLASSE_ID" != "null" ]; then
      CLASSE_ID="$TEST_CLASSE_ID"
      CLASSE_NOM="$TEST_CLASSE_NOM"
      break
    fi
  done
fi

# Si aucune classe trouvée, utiliser une classe par défaut
if [ -z "$CLASSE_ID" ] || [ "$CLASSE_ID" == "null" ]; then
  echo -e "${YELLOW}⚠️ Aucune classe trouvée via l'API, utilisation d'une classe par défaut${NC}"
  CLASSE_ID="${CLASSE_ID_DEFAULT:-80}"  # Utiliser la classe 80 par défaut (5ème Primaire)
  CLASSE_NOM="Classe ID $CLASSE_ID"
fi

echo -e "${GREEN}✅ Classe sélectionnée${NC}"
echo "   ID : $CLASSE_ID"
echo "   Nom : $CLASSE_NOM"
echo ""

# ============================================================================
# ÉTAPE 3 : RÉCUPÉRER LES ÉLÈVES DE LA CLASSE (pour vérifier les parents)
# ============================================================================

echo -e "${YELLOW}👨‍👩‍👧‍👦 Étape 3 : Vérification des parents de la classe...${NC}"

ELEVES_RESPONSE=$(curl -k -s -X GET "${API_URL}/Eleve?IdClasse=${CLASSE_ID}" \
  -H "Authorization: Bearer ${TOKEN}")

# Vérifier si la réponse est valide
if echo "$ELEVES_RESPONSE" | jq empty 2>/dev/null; then
  NOMBRE_ELEVES=$(echo "$ELEVES_RESPONSE" | jq '. | length // 0')
  echo -e "${GREEN}✅ ${NOMBRE_ELEVES} élève(s) trouvé(s) dans la classe${NC}"
  
  # Afficher quelques élèves si disponibles
  if [ "$NOMBRE_ELEVES" -gt 0 ]; then
    echo "$ELEVES_RESPONSE" | jq -r '.[:3] | .[] | "   - \(.nomComplet // "Élève") (ID: \(.idEleve))"' 2>/dev/null || true
    if [ "$NOMBRE_ELEVES" -gt 3 ]; then
      echo "   ... et $((NOMBRE_ELEVES - 3)) autre(s)"
    fi
  fi
else
  echo -e "${YELLOW}⚠️ Impossible de récupérer la liste des élèves (continuer quand même)${NC}"
  NOMBRE_ELEVES=0
fi
echo ""

# ============================================================================
# ÉTAPE 4 : CRÉER UN DEVOIR AVEC CONTENU TEXTUEL
# ============================================================================

echo -e "${YELLOW}📝 Étape 4 : Création d'un devoir avec contenu textuel...${NC}"

# Créer un fichier PDF de test minimal
cat > /tmp/test-devoir-notification.pdf << 'EOFPDF'
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
  # macOS
  DATE_LIMITE=$(date -u -v+7d +%Y-%m-%dT%H:%M:%S 2>/dev/null || date -u -v+7d +%Y-%m-%dT%H:%M:%S)
else
  # Linux
  DATE_LIMITE=$(date -u -d '+7 days' +%Y-%m-%dT%H:%M:%S)
fi

# Créer le devoir avec fichier ET contenu
DEVOIR_RESPONSE=$(curl -k -s -X POST "${API_URL}/DevoirADomicile" \
  -H "Authorization: Bearer ${TOKEN}" \
  -F "Titre=Test Notifications Multi-Canal - $(date +%H:%M:%S)" \
  -F "Description=Ce devoir teste l'envoi de notifications Push, SMS et Email aux parents" \
  -F "Contenu=Exercices à faire :\n1. Résoudre les équations\n2. Faire les exercices de la page 45\n3. Préparer la leçon suivante" \
  -F "IdClasse=${CLASSE_ID}" \
  -F "DateLimite=${DATE_LIMITE}" \
  -F "fichier=@/tmp/test-devoir-notification.pdf")

# Vérifier si la réponse est valide JSON
if echo "$DEVOIR_RESPONSE" | jq empty 2>/dev/null; then
  DEVOIR_ID=$(echo "$DEVOIR_RESPONSE" | jq -r '.idDevoirADomicile // empty')
  
  if [ -z "$DEVOIR_ID" ] || [ "$DEVOIR_ID" == "null" ]; then
    echo -e "${RED}❌ Échec de la création du devoir${NC}"
    echo "Réponse : $DEVOIR_RESPONSE"
    exit 1
  fi
else
  echo -e "${RED}❌ Réponse invalide de l'API${NC}"
  echo "Réponse brute : $DEVOIR_RESPONSE"
  exit 1
fi

echo -e "${GREEN}✅ Devoir créé avec succès${NC}"
echo "   ID : $DEVOIR_ID"
echo "   Titre : $(echo "$DEVOIR_RESPONSE" | jq -r '.titre')"
echo "   Classe : $(echo "$DEVOIR_RESPONSE" | jq -r '.nomClasse')"
echo ""

# ============================================================================
# ÉTAPE 5 : ATTENDRE QUE LES NOTIFICATIONS SOIENT ENVOYÉES
# ============================================================================

echo -e "${YELLOW}⏳ Étape 5 : Attente de l'envoi des notifications (5 secondes)...${NC}"
sleep 5
echo ""

# ============================================================================
# ÉTAPE 6 : VÉRIFIER LES LOGS (si possible)
# ============================================================================

echo -e "${YELLOW}📋 Étape 6 : Résumé du test${NC}"
echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ TEST TERMINÉ${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo "📊 Résultats attendus :"
echo "   ✅ Devoir créé : ID $DEVOIR_ID"
echo "   ✅ Notifications Push envoyées aux parents"
echo "   ✅ Notifications SMS envoyées (si numéros disponibles)"
echo "   ✅ Notifications Email envoyées (si emails disponibles)"
echo ""
echo "📝 Pour vérifier les notifications :"
echo "   1. Vérifier les logs de l'application pour voir :"
echo "      - '✅ Push envoyé au parent X pour devoir $DEVOIR_ID'"
echo "      - '✅ SMS envoyé au parent X pour devoir $DEVOIR_ID'"
echo "      - '✅ Email envoyé au parent X pour devoir $DEVOIR_ID'"
echo "   2. Vérifier l'application mobile des parents"
echo "   3. Vérifier les téléphones des parents (SMS)"
echo "   4. Vérifier les emails des parents"
echo ""
echo "🔍 Pour voir les détails du devoir créé :"
echo "   curl -X GET \"${API_URL}/DevoirADomicile/${DEVOIR_ID}\" \\"
echo "     -H \"Authorization: Bearer ${TOKEN}\" | jq"
echo ""

# Nettoyer
rm -f /tmp/test-devoir-notification.pdf

echo -e "${GREEN}✅ Script terminé${NC}"

