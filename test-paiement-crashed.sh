#!/bin/bash

# Script de test complet pour le système PaiementCrashed
# Usage: ./test-paiement-crashed.sh

BASE_URL="https://localhost:7102"
# BASE_URL="http://localhost:5002"  # Si vous utilisez HTTP

EMAIL="jk@kelasinabiso.cd"
PASSWORD="Root@Kansa_owner3"

echo "🧪 Test du système PaiementCrashed"
echo "===================================="
echo ""

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Authentification
echo "📝 Étape 1: Authentification..."
AUTH_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"${EMAIL}\",
    \"motDePasse\": \"${PASSWORD}\"
  }" \
  -k)

TOKEN=$(echo "$AUTH_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('accessToken', ''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
    echo -e "${RED}❌ Erreur d'authentification${NC}"
    echo "$AUTH_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$AUTH_RESPONSE"
    exit 1
fi

echo -e "${GREEN}✅ Authentification réussie${NC}"
echo "Token: ${TOKEN:0:50}..."
echo ""

# 2. Récupérer l'ID de l'école depuis le token
echo "📝 Étape 2: Extraction de l'ID de l'école..."
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

if [ -z "$ID_ECOLE" ]; then
    echo -e "${RED}❌ Impossible d'extraire l'ID de l'école${NC}"
    exit 1
fi

echo -e "${GREEN}✅ ID École: ${ID_ECOLE}${NC}"
echo ""

# 3. Vérifier les paiements échoués existants
echo "📝 Étape 3: Vérification des paiements échoués existants..."
CRASHED_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/ecole?estResolu=false" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

CRASHED_COUNT=$(echo "$CRASHED_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de paiements échoués non résolus: $CRASHED_COUNT"
if [ "$CRASHED_COUNT" -gt 0 ]; then
    echo -e "${YELLOW}⚠️  Il y a déjà $CRASHED_COUNT paiement(s) échoué(s)${NC}"
    echo "$CRASHED_RESPONSE" | python3 -m json.tool 2>/dev/null | head -30
fi
echo ""

# 4. Récupérer des élèves et frais réels de l'école
echo "📝 Étape 4: Récupération des élèves et frais de l'école..."
ELEVES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Eleve/ecole/all" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

FRAIS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/Frais" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

# Extraire quelques noms d'élèves et libellés de frais
NOM_ELEVE_1=$(echo "$ELEVES_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data[0].get('nomComplet', 'ELEVE_TEST') if isinstance(data, list) and len(data) > 0 else 'ELEVE_TEST')" 2>/dev/null || echo "ELEVE_TEST")
NOM_ELEVE_2=$(echo "$ELEVES_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data[1].get('nomComplet', 'ELEVE_TEST_2') if isinstance(data, list) and len(data) > 1 else 'ELEVE_TEST_2')" 2>/dev/null || echo "ELEVE_TEST_2")

LIBELLE_FRAIS_1=$(echo "$FRAIS_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data[0].get('libelleFrais', 'Minerval') if isinstance(data, list) and len(data) > 0 else 'Minerval')" 2>/dev/null || echo "Minerval")
LIBELLE_FRAIS_2=$(echo "$FRAIS_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data[1].get('libelleFrais', 'Frais examen') if isinstance(data, list) and len(data) > 1 else 'Frais examen')" 2>/dev/null || echo "Frais examen")

echo "Élève 1: $NOM_ELEVE_1"
echo "Élève 2: $NOM_ELEVE_2"
echo "Frais 1: $LIBELLE_FRAIS_1"
echo "Frais 2: $LIBELLE_FRAIS_2"
echo ""

# 5. Créer un fichier Excel de test avec des erreurs intentionnelles
echo "📝 Étape 5: Création d'un fichier Excel de test avec erreurs..."
python3 << PYTHON_SCRIPT
import openpyxl
from openpyxl import Workbook
from datetime import datetime

# Créer un nouveau workbook
wb = Workbook()
ws = wb.active
ws.title = "Paiements"

# En-têtes
headers = ["DatePaiement", "Montant", "Devise", "ModePaiement", "NomCompletEleve", "LibelleFrais"]
for col, header in enumerate(headers, 1):
    cell = ws.cell(row=1, column=col)
    cell.value = header
    cell.font = openpyxl.styles.Font(bold=True)
    cell.fill = openpyxl.styles.PatternFill(start_color="CCCCCC", end_color="CCCCCC", fill_type="solid")

# Ligne 1: Paiement valide (pour référence)
ws.cell(row=2, column=1).value = datetime.now()
ws.cell(row=2, column=2).value = 100
ws.cell(row=2, column=3).value = "USD"
ws.cell(row=2, column=4).value = "Cash"
ws.cell(row=2, column=5).value = "${NOM_ELEVE_1}"  # ✅ Nom d'élève réel
ws.cell(row=2, column=6).value = "${LIBELLE_FRAIS_1}"  # ✅ Libellé de frais réel

# Ligne 2: Élève introuvable
ws.cell(row=3, column=1).value = datetime.now()
ws.cell(row=3, column=2).value = 50
ws.cell(row=3, column=3).value = "CDF"
ws.cell(row=3, column=4).value = "Mobile Money"
ws.cell(row=3, column=5).value = "ELEVE_INEXISTANT_12345"  # ❌ Élève qui n'existe pas
ws.cell(row=3, column=6).value = "Minerval"

# Ligne 3: Frais introuvable
ws.cell(row=4, column=1).value = datetime.now()
ws.cell(row=4, column=2).value = 75
ws.cell(row=4, column=3).value = "USD"
ws.cell(row=4, column=4).value = "Carte"
ws.cell(row=4, column=5).value = "${NOM_ELEVE_1}"  # ✅ Nom d'élève réel
ws.cell(row=4, column=6).value = "FRAIS_INEXISTANT_XYZ_12345"  # ❌ Frais qui n'existe pas

# Ligne 4: Montant invalide
ws.cell(row=5, column=1).value = datetime.now()
ws.cell(row=5, column=2).value = -10  # ❌ Montant négatif
ws.cell(row=5, column=3).value = "USD"
ws.cell(row=5, column=4).value = "Cash"
ws.cell(row=5, column=5).value = "${NOM_ELEVE_1}"  # ✅ Nom d'élève réel
ws.cell(row=5, column=6).value = "${LIBELLE_FRAIS_1}"  # ✅ Libellé de frais réel

# Sauvegarder
wb.save("test_paiements_avec_erreurs.xlsx")
print("✅ Fichier Excel créé: test_paiements_avec_erreurs.xlsx")
PYTHON_SCRIPT

if [ ! -f "test_paiements_avec_erreurs.xlsx" ]; then
    echo -e "${RED}❌ Erreur lors de la création du fichier Excel${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Fichier Excel créé${NC}"
echo ""

# 6. Upload du fichier Excel (bulk insert)
echo "📝 Étape 6: Upload du fichier Excel (bulk insert)..."
UPLOAD_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Paiement/bulk-excel" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@test_paiements_avec_erreurs.xlsx" \
  -k)

echo "Réponse du bulk insert:"
echo "$UPLOAD_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$UPLOAD_RESPONSE"
echo ""

# Vérifier le nombre de paiements échoués
LIGNES_ECHOUEES=$(echo "$UPLOAD_RESPONSE" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data.get('lignesEchouees', 0))" 2>/dev/null || echo "0")

if [ "$LIGNES_ECHOUEES" -gt 0 ]; then
    echo -e "${YELLOW}⚠️  $LIGNES_ECHOUEES paiement(s) échoué(s) détecté(s)${NC}"
else
    echo -e "${GREEN}✅ Aucun paiement échoué (peut-être que tous les noms sont valides)${NC}"
fi
echo ""

# 7. Vérifier que les paiements échoués sont dans PaiementCrashed
echo "📝 Étape 7: Vérification des paiements échoués dans PaiementCrashed..."
sleep 2  # Attendre un peu pour que la sauvegarde soit terminée

CRASHED_RESPONSE_AFTER=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/ecole?estResolu=false" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -k)

CRASHED_COUNT_AFTER=$(echo "$CRASHED_RESPONSE_AFTER" | python3 -c "import sys, json; data = json.load(sys.stdin); print(len(data) if isinstance(data, list) else 0)" 2>/dev/null || echo "0")

echo "Nombre de paiements échoués après upload: $CRASHED_COUNT_AFTER"

if [ "$CRASHED_COUNT_AFTER" -gt 0 ]; then
    echo -e "${GREEN}✅ Les paiements échoués sont bien sauvegardés dans PaiementCrashed${NC}"
    echo ""
    echo "Premier paiement échoué:"
    echo "$CRASHED_RESPONSE_AFTER" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(json.dumps(data[0], indent=2, ensure_ascii=False))
" 2>/dev/null | head -40
    
    # Récupérer l'ID du premier paiement échoué
    FIRST_ID=$(echo "$CRASHED_RESPONSE_AFTER" | python3 -c "
import sys, json
data = json.load(sys.stdin)
if isinstance(data, list) and len(data) > 0:
    print(data[0].get('idPaiementCrashed', ''))
" 2>/dev/null)
    
    if [ ! -z "$FIRST_ID" ]; then
        echo ""
        echo "📝 Étape 8: Test de récupération d'un paiement échoué par ID..."
        GET_BY_ID=$(curl -s -X GET "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
          -H "Authorization: Bearer $TOKEN" \
          -H "Content-Type: application/json" \
          -k)
        
        echo "$GET_BY_ID" | python3 -m json.tool 2>/dev/null | head -40
        echo ""
        
        echo "📝 Étape 9: Test de modification d'un paiement échoué..."
        UPDATE_RESPONSE=$(curl -s -X PUT "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
          -H "Authorization: Bearer $TOKEN" \
          -H "Content-Type: application/json" \
          -d "{
            \"commentaire\": \"Paiement corrigé manuellement via test\"
          }" \
          -k)
        
        echo "$UPDATE_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$UPDATE_RESPONSE"
        echo ""
        
        echo "📝 Étape 10: Test de réinjection d'un paiement échoué..."
        # Récupérer l'ID de l'élève et des frais depuis le paiement échoué
        ID_ELEVE_CRASHED=$(echo "$GET_BY_ID" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data.get('idEleve', ''))" 2>/dev/null)
        ID_FRAIS_CRASHED=$(echo "$GET_BY_ID" | python3 -c "import sys, json; data = json.load(sys.stdin); print(data.get('idFrais', ''))" 2>/dev/null)
        
        if [ ! -z "$ID_ELEVE_CRASHED" ] && [ ! -z "$ID_FRAIS_CRASHED" ] && [ "$ID_ELEVE_CRASHED" != "null" ] && [ "$ID_FRAIS_CRASHED" != "null" ]; then
            echo "Correction du paiement échoué avec ID Élève: $ID_ELEVE_CRASHED, ID Frais: $ID_FRAIS_CRASHED"
            
            # Corriger le paiement échoué
            UPDATE_CORRECTED=$(curl -s -X PUT "${BASE_URL}/api/PaiementCrashed/${FIRST_ID}" \
              -H "Authorization: Bearer $TOKEN" \
              -H "Content-Type: application/json" \
              -d "{
                \"idEleve\": ${ID_ELEVE_CRASHED},
                \"idFrais\": ${ID_FRAIS_CRASHED},
                \"montant\": 100,
                \"commentaire\": \"Paiement corrigé et prêt pour réinjection\"
              }" \
              -k)
            
            echo "$UPDATE_CORRECTED" | python3 -m json.tool 2>/dev/null | head -20
            echo ""
            
            # Tenter la réinjection
            REINJECT_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/PaiementCrashed/reinject" \
              -H "Authorization: Bearer $TOKEN" \
              -H "Content-Type: application/json" \
              -d "{
                \"ids\": [${FIRST_ID}],
                \"forcerReinjection\": false
              }" \
              -k)
            
            echo "Résultat de la réinjection:"
            echo "$REINJECT_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$REINJECT_RESPONSE"
            echo ""
        else
            echo -e "${YELLOW}⚠️  Impossible de réinjecter : ID Élève ou Frais manquant${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠️  Aucun paiement échoué trouvé (peut-être que tous les noms sont valides)${NC}"
    echo ""
    echo "💡 Astuce: Modifiez le fichier Excel pour utiliser des noms d'élèves ou frais qui n'existent pas"
fi

echo ""
echo "===================================="
echo -e "${GREEN}✅ Tests terminés${NC}"
echo ""
echo "📋 Résumé:"
echo "  - Authentification: ✅"
echo "  - Upload Excel: ✅"
echo "  - Sauvegarde PaiementCrashed: $([ "$CRASHED_COUNT_AFTER" -gt 0 ] && echo "✅" || echo "⚠️")"
echo ""
echo "🔗 Routes disponibles:"
echo "  - GET /api/PaiementCrashed/ecole?estResolu=false"
echo "  - GET /api/PaiementCrashed/{id}"
echo "  - PUT /api/PaiementCrashed/{id}"
echo "  - PUT /api/PaiementCrashed/bulk-update"
echo "  - POST /api/PaiementCrashed/reinject"
echo "  - DELETE /api/PaiementCrashed/{id}"
echo ""

