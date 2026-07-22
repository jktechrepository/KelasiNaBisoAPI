#!/bin/bash

# Script pour tester la réinjection complète des paiements échoués
# 1. Vérifie/crée des élèves et frais de test
# 2. Corrige les paiements échoués
# 3. Teste la réinjection

BASE_URL="https://localhost:7102"
EMAIL="jk@kelasinabiso.cd"
PASSWORD="Root@Kansa_owner3"

echo "🧪 Test Complet de Réinjection des Paiements Échoués"
echo "====================================================="
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

# Extraire l'ID de l'école
ID_ECOLE=$(echo "$TOKEN" | cut -d'.' -f2 | python3 -c "
import base64
import json
import sys

payload = sys.stdin.read().strip()
padding = 4 - len(payload) % 4
if padding != 4:
    payload += '=' * padding

decoded = base64.urlsafe_b64decode(payload)
data = json.loads(decoded.decode('utf-8'))
print(data.get('idEcole', ''))
" 2>/dev/null)

echo -e "${GREEN}✅ ID École: ${ID_ECOLE}${NC}"
echo ""

# 2. Vérifier les élèves existants
echo -e "${BLUE}📝 Étape 2: Vérification des élèves...${NC}"
ELEVES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Eleve/ecole/all" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

ELEVES_COUNT=$(echo "$ELEVES_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre d'élèves dans l'école: $ELEVES_COUNT"

if [ "$ELEVES_COUNT" -eq 0 ]; then
    echo -e "${YELLOW}⚠️  Aucun élève trouvé. Création d'un élève de test...${NC}"
    
    # Récupérer les classes de l'école
    CLASSES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Classe/ecole/${ID_ECOLE}" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -k)
    
    ID_CLASSE=$(echo "$CLASSES_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idClasse', ''))
" 2>/dev/null)
    
    if [ -z "$ID_CLASSE" ]; then
        echo -e "${RED}❌ Aucune classe trouvée dans l'école. Impossible de créer un élève.${NC}"
        exit 1
    fi
    
    echo "Création d'un élève dans la classe $ID_CLASSE..."
    
    # Créer un élève de test avec tous les champs requis
    ELEVE_CREATE=$(curl -s -X POST "${BASE_URL}/api/Eleve" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"nom\": \"TEST\",
        \"postnom\": \"ELEVE\",
        \"prenom\": \"Paiement\",
        \"genre\": \"M\",
        \"dateNaissance\": \"2010-01-01T00:00:00\",
        \"lieuNaissance\": \"Kinshasa\",
        \"nationalite\": \"Congolaise\",
        \"idClasse\": ${ID_CLASSE},
        \"statut\": true
      }" \
      -k)
    
    # Afficher la réponse pour debug
    echo "Réponse création élève:"
    echo "$ELEVE_CREATE" | python3 -m json.tool 2>/dev/null || echo "$ELEVE_CREATE"
    echo ""
    
    ID_ELEVE_NEW=$(echo "$ELEVE_CREATE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data.get('idEleve', ''))" 2>/dev/null)
    
    if [ ! -z "$ID_ELEVE_NEW" ] && [ "$ID_ELEVE_NEW" != "null" ] && [ "$ID_ELEVE_NEW" != "None" ]; then
        echo -e "${GREEN}✅ Élève créé avec ID: $ID_ELEVE_NEW${NC}"
        ID_ELEVE_VALIDE=$ID_ELEVE_NEW
    else
        echo -e "${YELLOW}⚠️  Impossible de créer un élève. Utilisation d'un ID par défaut ou arrêt.${NC}"
        echo -e "${YELLOW}   Vous pouvez créer manuellement un élève et un frais, puis relancer le test.${NC}"
        exit 1
    fi
else
    # Utiliser le premier élève existant
    ID_ELEVE_VALIDE=$(echo "$ELEVES_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idEleve', ''))
" 2>/dev/null)
    
    echo -e "${GREEN}✅ Élève existant trouvé: ID $ID_ELEVE_VALIDE${NC}"
fi
echo ""

# 3. Vérifier les frais existants
echo -e "${BLUE}📝 Étape 3: Vérification des frais...${NC}"
FRAIS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Frais" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

FRAIS_COUNT=$(echo "$FRAIS_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de frais: $FRAIS_COUNT"

if [ "$FRAIS_COUNT" -eq 0 ]; then
    echo -e "${YELLOW}⚠️  Aucun frais trouvé. Création d'un frais de test...${NC}"
    
    # Récupérer les directions de l'école
    DIRECTIONS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Direction" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -k)
    
    ID_DIRECTION=$(echo "$DIRECTIONS_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list):
    for item in data:
        if item.get('idEcole') == ${ID_ECOLE}:
            print(item.get('idDirection', ''))
            break
" 2>/dev/null)
    
    if [ -z "$ID_DIRECTION" ]; then
        echo -e "${YELLOW}⚠️  Aucune direction trouvée. Création d'un frais sans direction...${NC}"
        ID_DIRECTION="null"
    fi
    
    # Créer un frais de test
    FRAIS_CREATE=$(curl -s -X POST "${BASE_URL}/api/Frais" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"libelleFrais\": \"Frais Test Paiement\",
        \"montantFrais\": 100,
        \"deviseFrais\": \"USD\",
        \"idEcole\": ${ID_ECOLE},
        \"statut\": true
      }" \
      -k)
    
    ID_FRAIS_NEW=$(echo "$FRAIS_CREATE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data.get('idFrais', ''))" 2>/dev/null)
    
    if [ ! -z "$ID_FRAIS_NEW" ]; then
        echo -e "${GREEN}✅ Frais créé avec ID: $ID_FRAIS_NEW${NC}"
        ID_FRAIS_VALIDE=$ID_FRAIS_NEW
    else
        echo -e "${RED}❌ Erreur lors de la création du frais${NC}"
        exit 1
    fi
else
    # Utiliser le premier frais existant
    ID_FRAIS_VALIDE=$(echo "$FRAIS_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idFrais', ''))
" 2>/dev/null)
    
    echo -e "${GREEN}✅ Frais existant trouvé: ID $ID_FRAIS_VALIDE${NC}"
fi
echo ""

# 4. Récupérer les paiements échoués
echo -e "${BLUE}📝 Étape 4: Récupération des paiements échoués...${NC}"
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

# Récupérer le premier paiement échoué
FIRST_ID=$(echo "$CRASHED_LIST" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idPaiementCrashed', ''))
" 2>/dev/null)

echo -e "${GREEN}✅ Premier paiement échoué: ID $FIRST_ID${NC}"
echo ""

# 5. Corriger le paiement échoué
echo -e "${BLUE}📝 Étape 5: Correction du paiement échoué ID $FIRST_ID...${NC}"
UPDATE_RESPONSE=$(curl -s -X PUT "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"idEleve\": ${ID_ELEVE_VALIDE},
    \"idFrais\": ${ID_FRAIS_VALIDE},
    \"montant\": 150,
    \"devise\": \"USD\",
    \"modePaiement\": \"Cash\",
    \"commentaire\": \"Paiement corrigé et prêt pour réinjection\"
  }" \
  -k)

echo "$UPDATE_RESPONSE" | python3 -m json.tool 2>/dev/null | head -25
echo ""

# 6. Tester la réinjection
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

REINJECT_ECHOUES=$(echo "$REINJECT_RESPONSE" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print(data.get('echoues', 0))
" 2>/dev/null || echo "0")

echo ""

if [ "$REINJECT_REUSSIS" -gt 0 ]; then
    echo -e "${GREEN}✅ Réinjection réussie !${NC}"
    echo -e "${GREEN}   - Réussis: $REINJECT_REUSSIS${NC}"
    echo -e "${GREEN}   - Échoués: $REINJECT_ECHOUES${NC}"
    
    # Vérifier que le paiement est marqué comme résolu
    sleep 1
    CRASHED_AFTER=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -k)
    
    EST_RESOLU=$(echo "$CRASHED_AFTER" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print(data.get('estResolu', False))
" 2>/dev/null || echo "false")
    
    ID_PAIEMENT_CREE=$(echo "$CRASHED_AFTER" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print(data.get('idPaiementCree', ''))
" 2>/dev/null || echo "")
    
    if [ "$EST_RESOLU" = "True" ] || [ "$EST_RESOLU" = "true" ]; then
        echo -e "${GREEN}✅ Paiement marqué comme résolu${NC}"
        if [ ! -z "$ID_PAIEMENT_CREE" ] && [ "$ID_PAIEMENT_CREE" != "null" ]; then
            echo -e "${GREEN}✅ Paiement créé avec ID: $ID_PAIEMENT_CREE${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠️  Réinjection échouée${NC}"
    echo "Vérifiez les erreurs ci-dessus"
fi
echo ""

# 7. Vérifier les paiements résolus
echo -e "${BLUE}📝 Étape 7: Vérification des paiements résolus...${NC}"
RESOLVED_LIST=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/ecole?estResolu=true" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

RESOLVED_COUNT=$(echo "$RESOLVED_LIST" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de paiements résolus: $RESOLVED_COUNT"
echo ""

# Résumé final
echo "====================================================="
echo -e "${GREEN}✅ Test de réinjection terminé${NC}"
echo ""
echo "📋 Résumé:"
echo "  - Élève utilisé: ID $ID_ELEVE_VALIDE"
echo "  - Frais utilisé: ID $ID_FRAIS_VALIDE"
echo "  - Paiements échoués: $CRASHED_COUNT"
echo "  - Réinjection réussie: $REINJECT_REUSSIS"
echo "  - Réinjection échouée: $REINJECT_ECHOUES"
echo "  - Paiements résolus: $RESOLVED_COUNT"
echo ""

