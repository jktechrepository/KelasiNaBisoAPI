#!/bin/bash

# ============================================================================
# Script de Test : Bulk Insert Paiements depuis Excel
# ============================================================================
# Ce script teste l'upload et le traitement de fichiers Excel pour les paiements
# ============================================================================

set -e  # Arrêter en cas d'erreur

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="${BASE_URL:-https://localhost:7102}"
API_URL="${BASE_URL}/api"

# Variables pour l'authentification
EMAIL="${EMAIL:-admin@kelasinabiso.cd}"
PASSWORD="${PASSWORD:-12345678}"

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  TEST : Bulk Insert Paiements depuis Excel${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""

# Fonction pour afficher les résultats
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ $2${NC}"
    else
        echo -e "${RED}❌ $2${NC}"
    fi
}

# Fonction pour vérifier si jq est installé
check_jq() {
    if ! command -v jq &> /dev/null; then
        echo -e "${YELLOW}⚠️  jq n'est pas installé. Installation des résultats JSON sera limitée.${NC}"
        echo -e "${YELLOW}   Installez jq avec: brew install jq (macOS) ou apt-get install jq (Linux)${NC}"
        return 1
    fi
    return 0
}

# Vérifier jq
HAS_JQ=false
if check_jq; then
    HAS_JQ=true
fi

# ============================================================================
# ÉTAPE 1 : Vérifier que l'API est accessible
# ============================================================================

echo -e "${BLUE}ÉTAPE 1 : Vérification de l'accessibilité de l'API${NC}"

if curl -k -s -f "${BASE_URL}/swagger/index.html" > /dev/null 2>&1; then
    print_result 0 "API accessible"
else
    echo -e "${RED}❌ L'API n'est pas accessible à ${BASE_URL}${NC}"
    echo -e "${YELLOW}   Vérifiez que l'application est démarrée${NC}"
    exit 1
fi

# ============================================================================
# ÉTAPE 2 : Authentification
# ============================================================================

echo ""
echo -e "${BLUE}ÉTAPE 2 : Authentification${NC}"

AUTH_RESPONSE=$(curl -k -s -X POST "${API_URL}/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d "{
        \"emailOuTelephone\": \"${EMAIL}\",
        \"motDePasse\": \"${PASSWORD}\"
    }")

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Erreur lors de l'authentification${NC}"
    exit 1
fi

if [ "$HAS_JQ" = true ]; then
    TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.accessToken // .token // empty')
    USER_ID=$(echo "$AUTH_RESPONSE" | jq -r '.utilisateur.idUtilisateur // .idUtilisateur // empty')
    USER_ROLE=$(echo "$AUTH_RESPONSE" | jq -r '.nomRole // .role // empty')
else
    TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)
    if [ -z "$TOKEN" ]; then
        TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
    fi
    USER_ID=$(echo "$AUTH_RESPONSE" | grep -o '"idUtilisateur":[0-9]*' | head -1 | cut -d':' -f2)
    USER_ROLE=$(echo "$AUTH_RESPONSE" | grep -o '"nomRole":"[^"]*' | cut -d'"' -f4)
fi

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ] || [ "$TOKEN" = "" ]; then
    echo -e "${RED}❌ Authentification échouée${NC}"
    echo "Réponse : $AUTH_RESPONSE"
    exit 1
fi

print_result 0 "Authentification réussie (User ID: ${USER_ID}, Role: ${USER_ROLE})"

# ============================================================================
# ÉTAPE 3 : Télécharger le template Excel
# ============================================================================

echo ""
echo -e "${BLUE}ÉTAPE 3 : Téléchargement du template Excel${NC}"

TEMPLATE_FILE="template_paiements.xlsx"

curl -k -s -X GET "${API_URL}/Paiement/template-excel" \
    -H "Authorization: Bearer ${TOKEN}" \
    -o "${TEMPLATE_FILE}"

if [ $? -eq 0 ] && [ -f "${TEMPLATE_FILE}" ] && [ -s "${TEMPLATE_FILE}" ]; then
    FILE_SIZE=$(stat -f%z "${TEMPLATE_FILE}" 2>/dev/null || stat -c%s "${TEMPLATE_FILE}" 2>/dev/null)
    print_result 0 "Template téléchargé (${FILE_SIZE} bytes)"
else
    print_result 1 "Échec du téléchargement du template"
    exit 1
fi

# ============================================================================
# ÉTAPE 4 : Créer un fichier Excel de test
# ============================================================================

echo ""
echo -e "${BLUE}ÉTAPE 4 : Création d'un fichier Excel de test${NC}"

TEST_FILE="test_paiements.xlsx"

# Pour l'instant, on va utiliser le template comme fichier de test
# Dans un vrai scénario, on utiliserait Python ou un autre outil pour modifier le fichier
cp "${TEMPLATE_FILE}" "${TEST_FILE}"

if [ -f "${TEST_FILE}" ]; then
    print_result 0 "Fichier de test créé : ${TEST_FILE}"
    echo -e "${YELLOW}⚠️  Note : Le fichier de test utilise le template.${NC}"
    echo -e "${YELLOW}   Pour un test complet, modifiez le fichier Excel avec des données valides.${NC}"
    echo -e "${YELLOW}   Utilisez test-bulk-insert-paiements-python.py pour générer un fichier de test complet.${NC}"
else
    print_result 1 "Échec de la création du fichier de test"
    exit 1
fi

# ============================================================================
# ÉTAPE 5 : Upload et traitement du fichier Excel
# ============================================================================

echo ""
echo -e "${BLUE}ÉTAPE 5 : Upload et traitement du fichier Excel${NC}"

UPLOAD_RESPONSE=$(curl -k -s -X POST "${API_URL}/Paiement/bulk-excel" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "file=@${TEST_FILE}")

if [ $? -ne 0 ]; then
    print_result 1 "Erreur lors de l'upload"
    echo "Réponse : $UPLOAD_RESPONSE"
    exit 1
fi

# Afficher la réponse
echo ""
echo -e "${BLUE}Réponse de l'API :${NC}"

if [ "$HAS_JQ" = true ]; then
    echo "$UPLOAD_RESPONSE" | jq '.'
    
    SUCCESS=$(echo "$UPLOAD_RESPONSE" | jq -r '.success // false')
    TOTAL_LIGNES=$(echo "$UPLOAD_RESPONSE" | jq -r '.totalLignes // 0')
    LIGNES_REUSSIES=$(echo "$UPLOAD_RESPONSE" | jq -r '.lignesReussies // 0')
    LIGNES_ECHOUEES=$(echo "$UPLOAD_RESPONSE" | jq -r '.lignesEchouees // 0')
    DOUBLONS=$(echo "$UPLOAD_RESPONSE" | jq -r '.doublonsDetectes // 0')
    MESSAGE=$(echo "$UPLOAD_RESPONSE" | jq -r '.message // ""')
    
    echo ""
    echo -e "${BLUE}Résumé :${NC}"
    echo "  - Succès : $SUCCESS"
    echo "  - Total lignes : $TOTAL_LIGNES"
    echo "  - Lignes réussies : $LIGNES_REUSSIES"
    echo "  - Lignes échouées : $LIGNES_ECHOUEES"
    echo "  - Doublons détectés : $DOUBLONS"
    echo "  - Message : $MESSAGE"
    
    if [ "$SUCCESS" = "true" ] || [ "$LIGNES_REUSSIES" -gt 0 ]; then
        print_result 0 "Upload réussi"
    else
        print_result 1 "Upload échoué"
    fi
    
    # Afficher les erreurs si présentes
    ERROR_COUNT=$(echo "$UPLOAD_RESPONSE" | jq -r '.lignesAvecErreurs | length // 0')
    if [ "$ERROR_COUNT" -gt 0 ]; then
        echo ""
        echo -e "${YELLOW}Erreurs détectées :${NC}"
        echo "$UPLOAD_RESPONSE" | jq -r '.lignesAvecErreurs[] | "  Ligne \(.numeroLigne): \(.erreurs | join(", "))"'
    fi
else
    echo "$UPLOAD_RESPONSE"
    echo ""
    echo -e "${YELLOW}⚠️  Installez jq pour un affichage formaté des résultats${NC}"
fi

# ============================================================================
# RÉSUMÉ FINAL
# ============================================================================

echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  RÉSUMÉ DU TEST${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo "Fichiers créés :"
echo "  - ${TEMPLATE_FILE} (template téléchargé)"
echo "  - ${TEST_FILE} (fichier de test)"
echo ""
echo -e "${YELLOW}Note : Pour un test complet avec des données réelles :${NC}"
echo "  1. Ouvrez le template Excel téléchargé"
echo "  2. Remplissez avec des données valides (IdEleve, IdFrais existants)"
echo "  3. Relancez ce script avec le fichier rempli"
echo "  4. Ou utilisez : python3 test-bulk-insert-paiements-python.py"
echo ""

# Nettoyage optionnel
read -p "Voulez-vous supprimer les fichiers de test ? (o/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]; then
    rm -f "${TEMPLATE_FILE}" "${TEST_FILE}"
    echo -e "${GREEN}✅ Fichiers supprimés${NC}"
fi

echo ""
echo -e "${GREEN}✅ Test terminé${NC}"
