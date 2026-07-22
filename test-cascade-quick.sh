#!/bin/bash

# ============================================================================
# Script de Test Rapide : Cascade Soft Delete Élève → Inscriptions
# ============================================================================

BASE_URL="http://localhost:5000"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🔐 ÉTAPE 1 : Authentification..."
LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailOuTelephone\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}")

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "❌ Erreur d'authentification"
  echo "Réponse: $LOGIN_RESPONSE"
  exit 1
fi

echo "✅ Authentification réussie"
echo "Token: ${TOKEN:0:50}..."
echo ""

# Demander l'ID de l'école
read -p "📚 Entrez l'ID de l'école: " ID_ECOLE

echo ""
echo "🔍 ÉTAPE 2 : Récupération des élèves de l'école ${ID_ECOLE}..."
ELEVES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Eleve/ecole/${ID_ECOLE}?pageNumber=1&pageSize=5" \
  -H "Authorization: Bearer ${TOKEN}")

echo "$ELEVES_RESPONSE" | jq '.' 2>/dev/null || echo "$ELEVES_RESPONSE"
echo ""

# Demander l'ID de l'élève
read -p "👤 Entrez l'ID de l'élève à tester: " ID_ELEVE

echo ""
echo "📊 ÉTAPE 3 : Vérification des inscriptions AVANT désactivation..."
INSCRIPTIONS_AVANT=$(curl -s -X GET "${BASE_URL}/api/Inscription/eleve/${ID_ELEVE}?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer ${TOKEN}")

echo "$INSCRIPTIONS_AVANT" | jq '.' 2>/dev/null || echo "$INSCRIPTIONS_AVANT"
echo ""

read -p "⏸️  Appuyez sur Entrée pour désactiver l'élève (cascade automatique)..."
echo ""

echo "🔄 ÉTAPE 4 : Désactivation de l'élève ${ID_ELEVE}..."
TOGGLE_RESPONSE=$(curl -s -X PUT "${BASE_URL}/api/Eleve/toggle-statut/${ID_ELEVE}" \
  -H "Authorization: Bearer ${TOKEN}")

echo "$TOGGLE_RESPONSE" | jq '.' 2>/dev/null || echo "$TOGGLE_RESPONSE"
echo ""

echo "✅ ÉTAPE 5 : Vérification des inscriptions APRÈS désactivation..."
INSCRIPTIONS_APRES=$(curl -s -X GET "${BASE_URL}/api/Inscription/eleve/${ID_ELEVE}?pageNumber=1&pageSize=10&includeInactive=true" \
  -H "Authorization: Bearer ${TOKEN}")

echo "$INSCRIPTIONS_APRES" | jq '.' 2>/dev/null || echo "$INSCRIPTIONS_APRES"
echo ""

echo "✅ Test terminé !"
echo ""
echo "📝 Vérifications :"
echo "   1. L'élève doit être désactivé (statut: false)"
echo "   2. Toutes les inscriptions actives doivent être désactivées"
echo "   3. Vérifiez les logs de l'application pour voir la cascade"
