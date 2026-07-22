#!/bin/bash

# Script de test complet pour le système PaiementCrashed
# Teste toutes les fonctionnalités : consultation, modification, réinjection

BASE_URL="https://localhost:7102"
EMAIL="jk@kelasinabiso.cd"
PASSWORD="Root@Kansa_owner3"

echo "🧪 Test Complet du Système PaiementCrashed"
echo "=========================================="
echo ""

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# 1. Authentification
echo -e "${BLUE}📝 Étape 1: Authentification...${NC}"
AUTH_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{\"emailOuTelephone\": \"${EMAIL}\", \"motDePasse\": \"${PASSWORD}\"}" \
  -k)

TOKEN=$(echo "$AUTH_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('accessToken', ''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
    echo -e "${RED}❌ Erreur d'authentification${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Authentification réussie${NC}"
echo ""

# 2. Lister tous les paiements échoués
echo -e "${BLUE}📝 Étape 2: Liste des paiements échoués non résolus...${NC}"
CRASHED_LIST=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/ecole?estResolu=false" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

CRASHED_COUNT=$(echo "$CRASHED_LIST" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de paiements échoués: $CRASHED_COUNT"

if [ "$CRASHED_COUNT" -eq 0 ]; then
    echo -e "${YELLOW}⚠️  Aucun paiement échoué trouvé. Créez d'abord un fichier Excel avec des erreurs.${NC}"
    exit 0
fi

# Afficher le premier paiement échoué
FIRST_ID=$(echo "$CRASHED_LIST" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idPaiementCrashed', ''))
" 2>/dev/null)

if [ -z "$FIRST_ID" ]; then
    echo -e "${RED}❌ Impossible de récupérer l'ID du premier paiement échoué${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Premier paiement échoué trouvé: ID $FIRST_ID${NC}"
echo ""

# 3. Récupérer les détails d'un paiement échoué
echo -e "${BLUE}📝 Étape 3: Détails du paiement échoué ID $FIRST_ID...${NC}"
GET_BY_ID=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

echo "$GET_BY_ID" | python3 -m json.tool 2>/dev/null | head -30
echo ""

# 4. Récupérer des élèves et frais réels pour la correction
echo -e "${BLUE}📝 Étape 4: Récupération des élèves et frais pour correction...${NC}"
ELEVES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Eleve/ecole/all" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

FRAIS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Frais" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

# Extraire le premier ID d'élève et de frais valides
ID_ELEVE_VALIDE=$(echo "$ELEVES_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idEleve', ''))
" 2>/dev/null)

ID_FRAIS_VALIDE=$(echo "$FRAIS_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idFrais', ''))
" 2>/dev/null)

if [ -z "$ID_ELEVE_VALIDE" ] || [ -z "$ID_FRAIS_VALIDE" ]; then
    echo -e "${YELLOW}⚠️  Impossible de récupérer des élèves ou frais valides. Test de modification ignoré.${NC}"
else
    echo -e "${GREEN}✅ ID Élève valide: $ID_ELEVE_VALIDE${NC}"
    echo -e "${GREEN}✅ ID Frais valide: $ID_FRAIS_VALIDE${NC}"
    echo ""
    
    # 5. Modifier un paiement échoué
    echo -e "${BLUE}📝 Étape 5: Modification du paiement échoué ID $FIRST_ID...${NC}"
    UPDATE_RESPONSE=$(curl -s -X PUT "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"idEleve\": ${ID_ELEVE_VALIDE},
        \"idFrais\": ${ID_FRAIS_VALIDE},
        \"montant\": 150,
        \"commentaire\": \"Paiement corrigé via test automatique\"
      }" \
      -k)
    
    echo "$UPDATE_RESPONSE" | python3 -m json.tool 2>/dev/null | head -25
    echo ""
    
    # 6. Tenter la réinjection
    echo -e "${BLUE}📝 Étape 6: Réinjection du paiement échoué ID $FIRST_ID...${NC}"
    REINJECT_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/PaiementCrashed/reinject" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"ids\": [${FIRST_ID}],
        \"forcerReinjection\": false
      }" \
      -k)
    
    echo "$REINJECT_RESPONSE" | python3 -m json.tool 2>/dev/null
    
    REINJECT_REUSSIS=$(echo "$REINJECT_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print(data.get('reussis', 0))
" 2>/dev/null || echo "0")
    
    if [ "$REINJECT_REUSSIS" -gt 0 ]; then
        echo -e "${GREEN}✅ Réinjection réussie !${NC}"
    else
        echo -e "${YELLOW}⚠️  Réinjection échouée (vérifiez les erreurs ci-dessus)${NC}"
    fi
    echo ""
fi

# 7. Test de modification en masse
echo -e "${BLUE}📝 Étape 7: Test de modification en masse...${NC}"
# Récupérer les 3 premiers IDs
IDS=$(echo "$CRASHED_LIST" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list):
    ids = [str(item.get('idPaiementCrashed', '')) for item in data[:3] if item.get('idPaiementCrashed')]
    print(','.join(ids))
" 2>/dev/null)

if [ ! -z "$IDS" ]; then
    BULK_UPDATE_RESPONSE=$(curl -s -X PUT "${BASE_URL}/api/PaiementCrashed/bulk-update" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"ids\": [${IDS}],
        \"devise\": \"USD\",
        \"modePaiement\": \"Cash\"
      }" \
      -k)
    
    echo "$BULK_UPDATE_RESPONSE" | python3 -m json.tool 2>/dev/null
    echo ""
else
    echo -e "${YELLOW}⚠️  Pas assez de paiements échoués pour tester la modification en masse${NC}"
    echo ""
fi

# 8. Vérifier les paiements résolus
echo -e "${BLUE}📝 Étape 8: Vérification des paiements résolus...${NC}"
RESOLVED_LIST=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/ecole?estResolu=true" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

RESOLVED_COUNT=$(echo "$RESOLVED_LIST" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de paiements résolus: $RESOLVED_COUNT"
echo ""

# Résumé
echo "=========================================="
echo -e "${GREEN}✅ Tests terminés${NC}"
echo ""
echo "📋 Résumé:"
echo "  - Paiements échoués non résolus: $CRASHED_COUNT"
echo "  - Paiements résolus: $RESOLVED_COUNT"
echo ""
echo "🔗 Routes testées:"
echo "  ✅ GET /api/PaiementCrashed/ecole?estResolu=false"
echo "  ✅ GET /api/PaiementCrashed/{id}"
echo "  ✅ PUT /api/PaiementCrashed/{id}"
echo "  ✅ PUT /api/PaiementCrashed/bulk-update"
echo "  ✅ POST /api/PaiementCrashed/reinject"
echo ""

