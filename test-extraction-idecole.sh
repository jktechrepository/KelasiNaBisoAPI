#!/bin/bash

# Script de test pour vérifier l'extraction de l'IdEcole depuis le token JWT
# Usage: ./test-extraction-idecole.sh

BASE_URL="http://localhost:5002"
# BASE_URL="https://localhost:7102"  # Si vous utilisez HTTPS

EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🔐 Test d'authentification et extraction IdEcole"
echo "=================================================="
echo ""

# 1. Authentification
echo "📝 Étape 1: Authentification..."
AUTH_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"${EMAIL}\",
    \"motDePasse\": \"${PASSWORD}\"
  }")

echo "Réponse d'authentification:"
echo "$AUTH_RESPONSE" | jq '.' 2>/dev/null || echo "$AUTH_RESPONSE"
echo ""

# Extraire le token
TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.token // .data.token // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo "❌ Erreur: Impossible d'extraire le token JWT"
    echo "Réponse complète:"
    echo "$AUTH_RESPONSE"
    exit 1
fi

echo "✅ Token JWT obtenu: ${TOKEN:0:50}..."
echo ""

# 2. Décoder le token JWT pour vérifier son contenu
echo "📝 Étape 2: Décodage du token JWT..."
# Le token JWT a 3 parties séparées par des points: header.payload.signature
PAYLOAD=$(echo "$TOKEN" | cut -d'.' -f2)

# Ajouter le padding si nécessaire (base64url)
PADDING_LENGTH=$((4 - ${#PAYLOAD} % 4))
if [ $PADDING_LENGTH -ne 4 ]; then
    PAYLOAD="${PAYLOAD}$(printf '%*s' $PADDING_LENGTH | tr ' ' '=')"
fi

# Décoder le payload (remplacer - par + et _ par / pour base64 standard)
PAYLOAD_DECODED=$(echo "$PAYLOAD" | tr '_-' '/+' | base64 -d 2>/dev/null)

if [ -z "$PAYLOAD_DECODED" ]; then
    echo "⚠️  Impossible de décoder le payload (peut nécessiter base64 -d sur macOS)"
    echo "Tentative avec Python..."
    PAYLOAD_DECODED=$(python3 -c "
import base64
import json
import sys

token = '${TOKEN}'
parts = token.split('.')
if len(parts) != 3:
    print('Token invalide')
    sys.exit(1)

payload = parts[1]
# Ajouter le padding
padding = 4 - len(payload) % 4
if padding != 4:
    payload += '=' * padding

# Décoder
decoded = base64.urlsafe_b64decode(payload)
print(decoded.decode('utf-8'))
" 2>/dev/null)
fi

echo "Payload décodé:"
echo "$PAYLOAD_DECODED" | jq '.' 2>/dev/null || echo "$PAYLOAD_DECODED"
echo ""

# Extraire l'IdEcole du payload
ID_ECOLE=$(echo "$PAYLOAD_DECODED" | jq -r '.idEcole // empty' 2>/dev/null)

if [ -z "$ID_ECOLE" ] || [ "$ID_ECOLE" = "null" ]; then
    echo "⚠️  ATTENTION: Le token ne contient pas d'IdEcole (ou la valeur est vide)"
    echo "Cela signifie que l'utilisateur n'a pas d'école associée en base de données."
    echo ""
    echo "Vérifiez en base de données:"
    echo "SELECT IdUtilisateur, IdEcole, NomUtilisateur FROM Utilisateurs WHERE Email = '${EMAIL}';"
    echo ""
else
    echo "✅ IdEcole trouvé dans le token: $ID_ECOLE"
    echo ""
fi

# 3. Tester l'extraction via l'API (endpoint qui utilise GetCurrentUserSchoolId)
echo "📝 Étape 3: Test d'extraction IdEcole via l'API..."
echo "Test avec GET /api/Eleve/ecole/all (extrait automatiquement l'IdEcole du token)"
echo ""

RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Eleve/ecole/all" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json")

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "${BASE_URL}/api/Eleve/ecole/all" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json")

echo "Code HTTP: $HTTP_CODE"
echo "Réponse:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ SUCCÈS: L'extraction de l'IdEcole fonctionne correctement!"
elif [ "$HTTP_CODE" = "401" ]; then
    echo "❌ ERREUR 401: Problème d'authentification ou d'extraction de l'IdEcole"
    echo "Message d'erreur:"
    echo "$RESPONSE" | jq -r '.message // .details // .' 2>/dev/null || echo "$RESPONSE"
else
    echo "⚠️  Code HTTP inattendu: $HTTP_CODE"
fi

echo ""
echo "=================================================="
echo "✅ Test terminé"

