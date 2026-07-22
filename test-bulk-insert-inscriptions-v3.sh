#!/bin/bash

# Script de test pour le bulk insert d'inscriptions V3 (avec idClasse, idAnneeScolaire, typeInscription en paramètres)
# Usage: ./test-bulk-insert-inscriptions-v3.sh

set -e

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

# IDs de test (selon les informations fournies précédemment)
ID_ECOLE=13
ID_CLASSE=43
ID_UTILISATEUR=20  # idDirection
TYPE_INSCRIPTION="Inscription"

echo "🧪 Test du bulk insert d'inscriptions V3"
echo "========================================"
echo ""
echo "📋 Configuration :"
echo "  - URL: ${BASE_URL}"
echo "  - École ID: ${ID_ECOLE}"
echo "  - Classe ID: ${ID_CLASSE}"
echo "  - Utilisateur ID: ${ID_UTILISATEUR}"
echo "  - Type: ${TYPE_INSCRIPTION}"
echo ""

# 1. Authentification
echo "🔐 1. Authentification..."
TOKEN_RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d "{\"emailOuTelephone\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}")

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.accessToken // .access_token // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo "❌ Erreur d'authentification"
    echo "$TOKEN_RESPONSE" | jq '.'
    exit 1
fi

echo "✅ Authentification réussie"
echo ""

# 2. Récupérer l'ID de l'année scolaire active pour l'école
echo "📅 2. Récupération de l'année scolaire active..."
ANNEE_SCOLAIRE_RESPONSE=$(curl -k -s "${BASE_URL}/api/AnneeScolaire/ecole/${ID_ECOLE}/actuelle" \
    -H "Authorization: Bearer ${TOKEN}")

ID_ANNEE_SCOLAIRE=$(echo "$ANNEE_SCOLAIRE_RESPONSE" | jq -r '.idAnneeScolaire // empty')

if [ -z "$ID_ANNEE_SCOLAIRE" ] || [ "$ID_ANNEE_SCOLAIRE" = "null" ]; then
    echo "⚠️  Aucune année scolaire active trouvée, tentative de récupération de la première année..."
    ANNEE_SCOLAIRE_RESPONSE=$(curl -k -s "${BASE_URL}/api/AnneeScolaire/ecole/${ID_ECOLE}" \
        -H "Authorization: Bearer ${TOKEN}")
    ID_ANNEE_SCOLAIRE=$(echo "$ANNEE_SCOLAIRE_RESPONSE" | jq -r '.[0].idAnneeScolaire // empty')
fi

if [ -z "$ID_ANNEE_SCOLAIRE" ] || [ "$ID_ANNEE_SCOLAIRE" = "null" ]; then
    echo "❌ Impossible de récupérer une année scolaire pour l'école ${ID_ECOLE}"
    echo "$ANNEE_SCOLAIRE_RESPONSE" | jq '.'
    exit 1
fi

echo "✅ Année scolaire trouvée : ID ${ID_ANNEE_SCOLAIRE}"
echo ""

# 3. Télécharger le template Excel
echo "📥 3. Téléchargement du template Excel..."
TEMPLATE_FILE="Template_Inscriptions_$(date +%Y%m%d).xlsx"

curl -k -s "${BASE_URL}/api/Inscription/template-excel" \
    -H "Authorization: Bearer ${TOKEN}" \
    -o "${TEMPLATE_FILE}"

if [ ! -f "${TEMPLATE_FILE}" ] || [ ! -s "${TEMPLATE_FILE}" ]; then
    echo "❌ Erreur lors du téléchargement du template"
    exit 1
fi

echo "✅ Template téléchargé : ${TEMPLATE_FILE}"
echo ""

# 4. Créer un fichier Excel de test à partir du template
echo "📝 4. Création du fichier Excel de test..."
TEST_FILE="test_inscriptions_v3_$(date +%Y%m%d_%H%M%S).xlsx"

# Copier le template
cp "${TEMPLATE_FILE}" "${TEST_FILE}"

# Utiliser Python pour remplir le fichier Excel
python3 << EOF
import openpyxl
from datetime import datetime, date
import sys

try:
    # Charger le workbook
    wb = openpyxl.load_workbook("${TEST_FILE}")
    ws = wb.active
    
    # Vérifier les en-têtes
    headers = [cell.value for cell in ws[1]]
    print(f"📋 Colonnes trouvées : {', '.join([str(h) for h in headers if h])}")
    
    # Vérifier que les colonnes supprimées ne sont plus présentes
    removed_columns = ["Type", "NomClasse", "LibelleAnneeScolaire"]
    found_removed = [col for col in removed_columns if col in headers]
    if found_removed:
        print(f"⚠️  ATTENTION : Colonnes supposées supprimées trouvées : {', '.join(found_removed)}")
    else:
        print("✅ Colonnes Type, NomClasse, LibelleAnneeScolaire correctement supprimées")
    
    # Ajouter des données de test (ligne 2)
    today = date.today()
    
    # Trouver les indices des colonnes
    col_indices = {}
    for idx, header in enumerate(headers, start=1):
        if header:
            col_indices[str(header)] = idx
    
    # Remplir les données
    if "DateInscription" in col_indices:
        ws.cell(row=2, column=col_indices["DateInscription"], value=today)
    
    if "NomEleve" in col_indices:
        ws.cell(row=2, column=col_indices["NomEleve"], value="TEST")
    if "PostnomEleve" in col_indices:
        ws.cell(row=2, column=col_indices["PostnomEleve"], value="BULK")
    if "PrenomEleve" in col_indices:
        ws.cell(row=2, column=col_indices["PrenomEleve"], value="InsertV3")
    if "GenreEleve" in col_indices:
        ws.cell(row=2, column=col_indices["GenreEleve"], value="M")
    if "DateNaissanceEleve" in col_indices:
        ws.cell(row=2, column=col_indices["DateNaissanceEleve"], value=date(2010, 5, 15))
    if "LieuNaissanceEleve" in col_indices:
        ws.cell(row=2, column=col_indices["LieuNaissanceEleve"], value="Kinshasa")
    if "NationaliteEleve" in col_indices:
        ws.cell(row=2, column=col_indices["NationaliteEleve"], value="Congolaise")
    if "NomCompletTuteur" in col_indices:
        ws.cell(row=2, column=col_indices["NomCompletTuteur"], value="TEST Parent")
    if "GenreTuteur" in col_indices:
        ws.cell(row=2, column=col_indices["GenreTuteur"], value="M")
    
    # Ajouter une deuxième ligne de test
    if "DateInscription" in col_indices:
        ws.cell(row=3, column=col_indices["DateInscription"], value=today)
    if "NomEleve" in col_indices:
        ws.cell(row=3, column=col_indices["NomEleve"], value="DEMO")
    if "PostnomEleve" in col_indices:
        ws.cell(row=3, column=col_indices["PostnomEleve"], value="INSERT")
    if "PrenomEleve" in col_indices:
        ws.cell(row=3, column=col_indices["PrenomEleve"], value="TestV3")
    if "GenreEleve" in col_indices:
        ws.cell(row=3, column=col_indices["GenreEleve"], value="F")
    if "DateNaissanceEleve" in col_indices:
        ws.cell(row=3, column=col_indices["DateNaissanceEleve"], value=date(2011, 8, 20))
    if "LieuNaissanceEleve" in col_indices:
        ws.cell(row=3, column=col_indices["LieuNaissanceEleve"], value="Lubumbashi")
    if "NationaliteEleve" in col_indices:
        ws.cell(row=3, column=col_indices["NationaliteEleve"], value="Congolaise")
    if "NomCompletTuteur" in col_indices:
        ws.cell(row=3, column=col_indices["NomCompletTuteur"], value="DEMO Parent")
    if "GenreTuteur" in col_indices:
        ws.cell(row=3, column=col_indices["GenreTuteur"], value="F")
    
    # Sauvegarder
    wb.save("${TEST_FILE}")
    print(f"✅ Fichier Excel de test créé : ${TEST_FILE}")
    print(f"   - 2 lignes de données ajoutées")
    
except Exception as e:
    print(f"❌ Erreur lors de la création du fichier Excel : {e}")
    sys.exit(1)
EOF

if [ $? -ne 0 ]; then
    echo "❌ Erreur lors de la création du fichier Excel de test"
    exit 1
fi

echo ""

# 5. Envoyer le fichier pour traitement
echo "📤 5. Envoi du fichier pour traitement..."
echo "   Endpoint: POST /api/Inscription/bulk-excel"
echo "   Paramètres:"
echo "     - idEcole: ${ID_ECOLE}"
echo "     - idUtilisateur: ${ID_UTILISATEUR}"
echo "     - idClasse: ${ID_CLASSE}"
echo "     - idAnneeScolaire: ${ID_ANNEE_SCOLAIRE}"
echo "     - typeInscription: ${TYPE_INSCRIPTION}"
echo ""

RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/Inscription/bulk-excel?idEcole=${ID_ECOLE}&idUtilisateur=${ID_UTILISATEUR}&idClasse=${ID_CLASSE}&idAnneeScolaire=${ID_ANNEE_SCOLAIRE}&typeInscription=${TYPE_INSCRIPTION}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "file=@${TEST_FILE}")

# 6. Afficher les résultats
echo "📊 6. Résultats du traitement :"
echo ""

SUCCESS=$(echo "$RESPONSE" | jq -r '.success // false')
LIGNES_REUSSIES=$(echo "$RESPONSE" | jq -r '.lignesReussies // 0')
LIGNES_ECHOUEES=$(echo "$RESPONSE" | jq -r '.lignesEchouees // 0')
TOTAL_LIGNES=$(echo "$RESPONSE" | jq -r '.totalLignes // 0')
MESSAGE=$(echo "$RESPONSE" | jq -r '.message // ""')

if [ "$SUCCESS" = "true" ]; then
    echo "✅ SUCCÈS !"
else
    echo "⚠️  Traitement terminé avec des erreurs"
fi

echo ""
echo "📈 Statistiques :"
echo "   - Total de lignes : ${TOTAL_LIGNES}"
echo "   - Lignes réussies : ${LIGNES_REUSSIES}"
echo "   - Lignes échouées : ${LIGNES_ECHOUEES}"
echo ""

if [ -n "$MESSAGE" ]; then
    echo "💬 Message :"
    echo "   ${MESSAGE}"
    echo ""
fi

# Afficher les erreurs s'il y en a
ERRORS=$(echo "$RESPONSE" | jq -r '.lignesAvecErreurs // [] | length')
if [ "$ERRORS" -gt 0 ]; then
    echo "❌ Erreurs détaillées :"
    echo "$RESPONSE" | jq '.lignesAvecErreurs[] | {ligne: .numeroLigne, erreurs: .erreurs}'
    echo ""
fi

# 7. Résumé
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ "$SUCCESS" = "true" ] && [ "$LIGNES_ECHOUEES" -eq 0 ]; then
    echo "✅ TEST RÉUSSI : Toutes les inscriptions ont été créées avec succès !"
    exit 0
elif [ "$LIGNES_REUSSIES" -gt 0 ]; then
    echo "⚠️  TEST PARTIELLEMENT RÉUSSI : ${LIGNES_REUSSIES} inscription(s) créée(s), ${LIGNES_ECHOUEES} échouée(s)"
    exit 0
else
    echo "❌ TEST ÉCHOUÉ : Aucune inscription n'a été créée"
    echo ""
    echo "Réponse complète :"
    echo "$RESPONSE" | jq '.'
    exit 1
fi

