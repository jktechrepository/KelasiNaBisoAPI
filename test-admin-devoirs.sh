#!/bin/bash

# Script de test pour les permissions Admin - Devoirs à Domicile
# Compte Admin: jk2@kelasinabiso.cd / 12345678

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🧪 Test des permissions Admin pour les Devoirs à Domicile"
echo "=================================================="
echo ""

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Fonction pour afficher les résultats
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# Étape 1 : Authentification
echo "📝 Étape 1 : Authentification"
echo "----------------------------"
AUTH_RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"$EMAIL\",
    \"motDePasse\": \"$PASSWORD\"
  }")

echo "Réponse d'authentification :"
echo "$AUTH_RESPONSE" | jq '.' 2>/dev/null || echo "$AUTH_RESPONSE"
echo ""

# Extraire le token
TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.accessToken // .token // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    print_error "Échec de l'authentification. Impossible d'obtenir le token."
    exit 1
fi

print_success "Authentification réussie !"
print_info "Token obtenu : ${TOKEN:0:50}..."
echo ""

# Étape 2 : Vérifier les informations de l'utilisateur
echo "📝 Étape 2 : Informations de l'utilisateur"
echo "----------------------------------------"
USER_INFO=$(curl -k -s -X GET "$BASE_URL/api/Utilisateur/me" \
  -H "Authorization: Bearer $TOKEN")

echo "$USER_INFO" | jq '.' 2>/dev/null || echo "$USER_INFO"
echo ""

ROLE=$(echo "$USER_INFO" | jq -r '.role // .utilisateur.role // empty' 2>/dev/null)
ECOLE_ID=$(echo "$USER_INFO" | jq -r '.idEcole // .utilisateur.idEcole // empty' 2>/dev/null)
AGENT_ID=$(echo "$USER_INFO" | jq -r '.idAgent // .utilisateur.idAgent // empty' 2>/dev/null)

print_info "Rôle : $ROLE"
print_info "ID École : $ECOLE_ID"
print_info "ID Agent : $AGENT_ID"
echo ""

# Étape 3 : Vérifier les permissions - Voir mes devoirs (devrait voir tous les devoirs de l'école)
echo "📝 Étape 3 : Voir mes devoirs (Admin devrait voir tous les devoirs de son école)"
echo "------------------------------------------------------------------------------"
MES_DEVOIRS=$(curl -k -s -X GET "$BASE_URL/api/DevoirADomicile/mes-devoirs" \
  -H "Authorization: Bearer $TOKEN")

echo "$MES_DEVOIRS" | jq '.' 2>/dev/null || echo "$MES_DEVOIRS"
echo ""

DEVOIRS_COUNT=$(echo "$MES_DEVOIRS" | jq 'length' 2>/dev/null || echo "0")
print_info "Nombre de devoirs récupérés : $DEVOIRS_COUNT"
echo ""

# Étape 4 : Vérifier les permissions - Voir les devoirs d'une classe
echo "📝 Étape 4 : Voir les devoirs d'une classe (nécessite un ID de classe)"
echo "---------------------------------------------------------------------"
print_info "Pour tester cette étape, vous devez fournir un ID de classe valide."
print_info "Exemple : curl -k -X GET \"$BASE_URL/api/DevoirADomicile/classe/1\" -H \"Authorization: Bearer $TOKEN\""
echo ""

# Étape 5 : Test de publication (nécessite un fichier PDF)
echo "📝 Étape 5 : Test de publication d'un devoir"
echo "-------------------------------------------"
print_info "Pour tester la publication, vous devez :"
print_info "1. Avoir un fichier PDF de test"
print_info "2. Connaître un ID de classe valide"
print_info "3. Utiliser l'endpoint : POST /api/DevoirADomicile"
print_info "4. Format : multipart/form-data avec fichier et données"
echo ""

# Étape 6 : Vérifier la configuration S3
echo "📝 Étape 6 : Vérification de la configuration S3"
echo "-----------------------------------------------"
print_info "Vérifiez les logs de l'application au démarrage pour voir :"
print_info "✅ Stockage AWS S3 configuré et activé"
print_info "OU"
print_info "⚠️  Credentials AWS S3 non configurés. Utilisation du stockage local."
echo ""

print_success "Tests terminés !"
print_info "Token valide pour les prochaines requêtes : $TOKEN"

