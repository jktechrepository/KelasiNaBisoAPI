#!/bin/bash

# Script de test pour Bulk Insert Inscriptions V2
# Teste le nouveau format simplifié avec NomClasse et LibelleAnneeScolaire

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🧪 Test Bulk Insert Inscriptions V2 - Format Simplifié"
echo "======================================================"
echo ""

# 1. Authentification
echo "1️⃣ Authentification..."
TOKEN=$(curl -k -s -X POST "${BASE_URL}/api/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d "{\"emailOuTelephone\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}" \
    | jq -r '.accessToken')

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo "❌ Erreur d'authentification"
    exit 1
fi

echo "✅ Authentification réussie"
echo ""

# 2. Récupérer l'ID de l'école de l'utilisateur
echo "2️⃣ Récupération des informations utilisateur..."
USER_INFO=$(curl -k -s "${BASE_URL}/api/Utilisateur/me" \
    -H "Authorization: Bearer ${TOKEN}")

ID_ECOLE=$(echo "$USER_INFO" | jq -r '.idEcole // .ecoleId // 1')
ID_UTILISATEUR=$(echo "$USER_INFO" | jq -r '.idUtilisateur // .userId // 1')

echo "   ID École: ${ID_ECOLE}"
echo "   ID Utilisateur: ${ID_UTILISATEUR}"
echo ""

# 3. Télécharger le template
echo "3️⃣ Téléchargement du template Excel..."
TEMPLATE_RESPONSE=$(curl -k -s -w "\n%{http_code}" "${BASE_URL}/api/Inscription/template-excel" \
    -H "Authorization: Bearer ${TOKEN}" \
    -o "template_inscriptions_v2.xlsx")

HTTP_CODE=$(echo "$TEMPLATE_RESPONSE" | tail -1)
if [ "$HTTP_CODE" == "200" ]; then
    TEMPLATE_SIZE=$(stat -f%z template_inscriptions_v2.xlsx 2>/dev/null || stat -c%s template_inscriptions_v2.xlsx 2>/dev/null)
    echo "✅ Template téléchargé (${TEMPLATE_SIZE} bytes)"
else
    echo "❌ Erreur lors du téléchargement du template (HTTP ${HTTP_CODE})"
    exit 1
fi
echo ""

# 4. Récupérer des classes et années scolaires valides de l'école
echo "4️⃣ Récupération des classes et années scolaires de l'école ${ID_ECOLE}..."

# Récupérer les classes
CLASSES=$(curl -k -s "${BASE_URL}/api/Classe/ecole/${ID_ECOLE}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0:3] | .[] | "\(.idClasse)|\(.nomClasse)"')

# Récupérer les années scolaires
ANNEES=$(curl -k -s "${BASE_URL}/api/AnneeScolaire/ecole/${ID_ECOLE}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0:3] | .[] | "\(.idAnneeScolaire)|\(.libelleAnneeScolaire)"')

if [ -z "$CLASSES" ] || [ -z "$ANNEES" ]; then
    echo "⚠️  Aucune classe ou année scolaire trouvée dans l'école ${ID_ECOLE}"
    echo "   Utilisation de données d'exemple..."
    CLASSE_1="1|1ère Primaire"
    CLASSE_2="2|2ème Primaire"
    ANNEE_1="1|2024-2025"
    ANNEE_2="2|2025-2026"
else
    CLASSE_1=$(echo "$CLASSES" | head -1)
    CLASSE_2=$(echo "$CLASSES" | head -2 | tail -1)
    ANNEE_1=$(echo "$ANNEES" | head -1)
    ANNEE_2=$(echo "$ANNEES" | head -2 | tail -1)
fi

NOM_CLASSE_1=$(echo "$CLASSE_1" | cut -d'|' -f2)
NOM_CLASSE_2=$(echo "$CLASSE_2" | cut -d'|' -f2)
LIBELLE_ANNEE_1=$(echo "$ANNEE_1" | cut -d'|' -f2)
LIBELLE_ANNEE_2=$(echo "$ANNEE_2" | cut -d'|' -f2)

echo "   Classe 1: ${NOM_CLASSE_1}"
echo "   Classe 2: ${NOM_CLASSE_2}"
echo "   Année 1: ${LIBELLE_ANNEE_1}"
echo "   Année 2: ${LIBELLE_ANNEE_2}"
echo ""

# 5. Créer un fichier Excel de test à partir du template
echo "5️⃣ Création du fichier Excel de test..."
python3 create_test_excel_from_template.py \
    "template_inscriptions_v2.xlsx" \
    "test_inscriptions_v2.xlsx" \
    "${NOM_CLASSE_1},${NOM_CLASSE_2}" \
    "${LIBELLE_ANNEE_1},${LIBELLE_ANNEE_2}"

if [ $? -ne 0 ]; then
    echo "⚠️  openpyxl non disponible, création d'un fichier Excel basique..."
    # Fallback vers l'ancienne méthode si openpyxl n'est pas disponible
    python3 << PYTHON_SCRIPT
import zipfile
from datetime import datetime

# Date Excel (nombre de jours depuis 1900-01-01)
def excel_date(date):
    epoch = datetime(1899, 12, 30)
    delta = date - epoch
    return delta.days + delta.seconds / 86400

today = datetime.now()
date_inscription = excel_date(today)
date_naissance = excel_date(datetime(2010, 5, 15))

# Données de test
NOM_CLASSE_1 = "${NOM_CLASSE_1}"
NOM_CLASSE_2 = "${NOM_CLASSE_2}"
LIBELLE_ANNEE_1 = "${LIBELLE_ANNEE_1}"
LIBELLE_ANNEE_2 = "${LIBELLE_ANNEE_2}"

with zipfile.ZipFile('test_inscriptions_v2.xlsx', 'w', zipfile.ZIP_DEFLATED) as xlsx:
    # [Content_Types].xml
    content_types = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
<Default Extension="xml" ContentType="application/xml"/>
<Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
<Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
<Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
<Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>'''
    xlsx.writestr('[Content_Types].xml', content_types)
    
    # _rels/.rels
    rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>'''
    xlsx.writestr('_rels/.rels', rels)
    
    # xl/workbook.xml
    workbook = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
<sheets>
<sheet name="Inscriptions" sheetId="1" r:id="rId1"/>
</sheets>
</workbook>'''
    xlsx.writestr('xl/workbook.xml', workbook)
    
    # xl/_rels/workbook.xml.rels
    workbook_rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
<Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>'''
    xlsx.writestr('xl/_rels/workbook.xml.rels', workbook_rels)
    
    # xl/sharedStrings.xml
    shared_strings = f'''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="25" uniqueCount="25">
<si><t>Type</t></si>
<si><t>DateInscription</t></si>
<si><t>NomEleve</t></si>
<si><t>PostnomEleve</t></si>
<si><t>PrenomEleve</t></si>
<si><t>GenreEleve</t></si>
<si><t>DateNaissanceEleve</t></si>
<si><t>LieuNaissanceEleve</t></si>
<si><t>NationaliteEleve</t></si>
<si><t>NomCompletTuteur</t></si>
<si><t>GenreTuteur</t></si>
<si><t>NomClasse</t></si>
<si><t>LibelleAnneeScolaire</t></si>
<si><t>Inscription</t></si>
<si><t>MUKENDI</t></si>
<si><t>KALALA</t></si>
<si><t>Jean</t></si>
<si><t>M</t></si>
<si><t>Kinshasa</t></si>
<si><t>Congolaise</t></si>
<si><t>MUKENDI Pierre</t></si>
<si><t>{NOM_CLASSE_1}</t></si>
<si><t>{LIBELLE_ANNEE_1}</t></si>
<si><t>KALALA</t></si>
<si><t>Marie</t></si>
<si><t>F</t></si>
<si><t>KALALA Marie</t></si>
<si><t>{NOM_CLASSE_2}</t></si>
<si><t>{LIBELLE_ANNEE_2}</t></si>
</sst>'''
    xlsx.writestr('xl/sharedStrings.xml', shared_strings)
    
    # xl/styles.xml
    styles = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
<numFmts count="1">
<numFmt numFmtId="14" formatCode="mm/dd/yyyy"/>
</numFmts>
<fonts count="1">
<font><sz val="11"/></font>
</fonts>
<fills count="1">
<fill><patternFill patternType="none"/></fill>
</fills>
<borders count="1">
<border><left/><right/><top/><bottom/></border>
</borders>
<cellStyleXfs count="1">
<xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
</cellStyleXfs>
<cellXfs count="2">
<xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
<xf numFmtId="14" fontId="0" fillId="0" borderId="0"/>
</cellXfs>
</styleSheet>'''
    xlsx.writestr('xl/styles.xml', styles)
    
    # xl/worksheets/sheet1.xml
    sheet = f'''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
<sheetData>
<row r="1">
<c r="A1" t="s"><v>0</v></c>
<c r="B1" t="s"><v>1</v></c>
<c r="C1" t="s"><v>2</v></c>
<c r="D1" t="s"><v>3</v></c>
<c r="E1" t="s"><v>4</v></c>
<c r="F1" t="s"><v>5</v></c>
<c r="G1" t="s"><v>6</v></c>
<c r="H1" t="s"><v>7</v></c>
<c r="I1" t="s"><v>8</v></c>
<c r="J1" t="s"><v>9</v></c>
<c r="K1" t="s"><v>10</v></c>
<c r="L1" t="s"><v>11</v></c>
<c r="M1" t="s"><v>12</v></c>
</row>
<row r="2">
<c r="A2" t="s"><v>13</v></c>
<c r="B2" s="1"><v>{date_inscription}</v></c>
<c r="C2" t="s"><v>14</v></c>
<c r="D2" t="s"><v>15</v></c>
<c r="E2" t="s"><v>16</v></c>
<c r="F2" t="s"><v>17</v></c>
<c r="G2" s="1"><v>{date_naissance}</v></c>
<c r="H2" t="s"><v>18</v></c>
<c r="I2" t="s"><v>19</v></c>
<c r="J2" t="s"><v>20</v></c>
<c r="K2" t="s"><v>17</v></c>
<c r="L2" t="s"><v>21</v></c>
<c r="M2" t="s"><v>22</v></c>
</row>
<row r="3">
<c r="A3" t="s"><v>13</v></c>
<c r="B3" s="1"><v>{date_inscription}</v></c>
<c r="C3" t="s"><v>23</v></c>
<c r="D3" t="s"><v>24</v></c>
<c r="E3" t="s"><v>25</v></c>
<c r="F3" t="s"><v>26</v></c>
<c r="G3" s="1"><v>{date_naissance}</v></c>
<c r="H3" t="s"><v>18</v></c>
<c r="I3" t="s"><v>19</v></c>
<c r="J3" t="s"><v>27</v></c>
<c r="K3" t="s"><v>26</v></c>
<c r="L3" t="s"><v>28</v></c>
<c r="M3" t="s"><v>29</v></c>
</row>
</sheetData>
</worksheet>'''
    xlsx.writestr('xl/worksheets/sheet1.xml', sheet)

print("✅ Fichier Excel créé : test_inscriptions_v2.xlsx")
PYTHON_SCRIPT
fi

if [ ! -f "test_inscriptions_v2.xlsx" ]; then
    echo "❌ Erreur lors de la création du fichier Excel"
    exit 1
fi

echo "✅ Fichier Excel créé"
echo ""

# 6. Tester l'upload
echo "6️⃣ Upload du fichier Excel..."
UPLOAD_RESULT=$(curl -k -s -X POST "${BASE_URL}/api/Inscription/bulk-excel?idEcole=${ID_ECOLE}&idUtilisateur=${ID_UTILISATEUR}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "file=@test_inscriptions_v2.xlsx" \
    | jq '.')

SUCCESS=$(echo "$UPLOAD_RESULT" | jq -r '.success')
TOTAL_LIGNES=$(echo "$UPLOAD_RESULT" | jq -r '.totalLignes')
LIGNES_REUSSIES=$(echo "$UPLOAD_RESULT" | jq -r '.lignesReussies')
LIGNES_ECHOUES=$(echo "$UPLOAD_RESULT" | jq -r '.lignesEchouees')

echo "📊 Résultats :"
echo "$UPLOAD_RESULT" | jq '{
    success,
    message,
    totalLignes,
    lignesReussies,
    lignesEchouees,
    doublonsDetectes,
    inscriptionsCrees: (.inscriptionsCrees | length)
}'

echo ""
if [ "$SUCCESS" == "true" ] || [ "$LIGNES_REUSSIES" -gt 0 ]; then
    echo "✅ Test réussi !"
    echo "   - ${LIGNES_REUSSIES} inscription(s) créée(s) sur ${TOTAL_LIGNES} ligne(s)"
else
    echo "❌ Test échoué"
    echo "   - ${LIGNES_ECHOUES} erreur(s) sur ${TOTAL_LIGNES} ligne(s)"
    echo ""
    echo "Erreurs détaillées :"
    echo "$UPLOAD_RESULT" | jq '.lignesAvecErreurs[] | {ligne: .numeroLigne, erreurs}'
fi

echo ""
echo "======================================================"
echo "✅ Tests terminés"

