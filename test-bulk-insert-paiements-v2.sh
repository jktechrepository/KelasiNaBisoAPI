#!/bin/bash

# Script de test pour Bulk Insert Paiements V2
# Teste le nouveau format simplifié avec NomCompletEleve et LibelleFrais

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🧪 Test Bulk Insert Paiements V2 - Format Simplifié"
echo "=================================================="
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
TEMPLATE_RESPONSE=$(curl -k -s -w "\n%{http_code}" "${BASE_URL}/api/Paiement/template-excel" \
    -H "Authorization: Bearer ${TOKEN}" \
    -o "template_paiements_v2.xlsx")

HTTP_CODE=$(echo "$TEMPLATE_RESPONSE" | tail -1)
if [ "$HTTP_CODE" == "200" ]; then
    TEMPLATE_SIZE=$(stat -f%z template_paiements_v2.xlsx 2>/dev/null || stat -c%s template_paiements_v2.xlsx 2>/dev/null)
    echo "✅ Template téléchargé (${TEMPLATE_SIZE} bytes)"
else
    echo "❌ Erreur lors du téléchargement du template (HTTP ${HTTP_CODE})"
    exit 1
fi
echo ""

# 4. Récupérer des élèves et frais valides de l'école
echo "4️⃣ Récupération des élèves et frais de l'école ${ID_ECOLE}..."

# Récupérer les élèves
ELEVES=$(curl -k -s "${BASE_URL}/api/Eleve/ecole/${ID_ECOLE}/paged?pageSize=5" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.data[0:3] | .[] | "\(.idEleve)|\(.nomComplet)"')

# Récupérer les frais
FRAIS=$(curl -k -s "${BASE_URL}/api/Frais/ecole/${ID_ECOLE}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0:3] | .[] | "\(.idFrais)|\(.libelleFrais)"')

if [ -z "$ELEVES" ] || [ -z "$FRAIS" ]; then
    echo "⚠️  Aucun élève ou frais trouvé dans l'école ${ID_ECOLE}"
    echo "   Utilisation de données d'exemple..."
    ELEVE_1="1|ELEVE TEST 1"
    ELEVE_2="2|ELEVE TEST 2"
    FRAIS_1="1|Minerval"
    FRAIS_2="2|Frais examen"
else
    ELEVE_1=$(echo "$ELEVES" | head -1)
    ELEVE_2=$(echo "$ELEVES" | head -2 | tail -1)
    FRAIS_1=$(echo "$FRAIS" | head -1)
    FRAIS_2=$(echo "$FRAIS" | head -2 | tail -1)
fi

NOM_ELEVE_1=$(echo "$ELEVE_1" | cut -d'|' -f2)
NOM_ELEVE_2=$(echo "$ELEVE_2" | cut -d'|' -f2)
LIBELLE_FRAIS_1=$(echo "$FRAIS_1" | cut -d'|' -f2)
LIBELLE_FRAIS_2=$(echo "$FRAIS_2" | cut -d'|' -f2)

echo "   Élève 1: ${NOM_ELEVE_1}"
echo "   Élève 2: ${NOM_ELEVE_2}"
echo "   Frais 1: ${LIBELLE_FRAIS_1}"
echo "   Frais 2: ${LIBELLE_FRAIS_2}"
echo ""

# 5. Créer un fichier Excel de test avec Python
echo "5️⃣ Création du fichier Excel de test..."
python3 << PYTHON_SCRIPT
import zipfile
from datetime import datetime

# Date Excel (nombre de jours depuis 1900-01-01)
def excel_date(date):
    epoch = datetime(1899, 12, 30)
    delta = date - epoch
    return delta.days + delta.seconds / 86400

today = datetime.now()
date_excel = excel_date(today)

# Données de test
NOM_ELEVE_1 = "${NOM_ELEVE_1}"
NOM_ELEVE_2 = "${NOM_ELEVE_2}"
LIBELLE_FRAIS_1 = "${LIBELLE_FRAIS_1}"
LIBELLE_FRAIS_2 = "${LIBELLE_FRAIS_2}"

with zipfile.ZipFile('test_paiements_v2.xlsx', 'w', zipfile.ZIP_DEFLATED) as xlsx:
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
<sheet name="Paiements" sheetId="1" r:id="rId1"/>
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
<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="15" uniqueCount="15">
<si><t>DatePaiement</t></si>
<si><t>Montant</t></si>
<si><t>Devise</t></si>
<si><t>ModePaiement</t></si>
<si><t>NomCompletEleve</t></si>
<si><t>LibelleFrais</t></si>
<si><t>USD</t></si>
<si><t>Cash</t></si>
<si><t>{NOM_ELEVE_1}</t></si>
<si><t>{LIBELLE_FRAIS_1}</t></si>
<si><t>CDF</t></si>
<si><t>Mobile Money</t></si>
<si><t>{NOM_ELEVE_2}</t></si>
<si><t>{LIBELLE_FRAIS_2}</t></si>
<si><t>Carte</t></si>
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
</row>
<row r="2">
<c r="A2" s="1"><v>{date_excel}</v></c>
<c r="B2"><v>100</v></c>
<c r="C2" t="s"><v>6</v></c>
<c r="D2" t="s"><v>7</v></c>
<c r="E2" t="s"><v>8</v></c>
<c r="F2" t="s"><v>9</v></c>
</row>
<row r="3">
<c r="A3" s="1"><v>{date_excel}</v></c>
<c r="B3"><v>150.5</v></c>
<c r="C3" t="s"><v>10</v></c>
<c r="D3" t="s"><v>11</v></c>
<c r="E3" t="s"><v>12</v></c>
<c r="F3" t="s"><v>13</v></c>
</row>
<row r="4">
<c r="A4" s="1"><v>{date_excel}</v></c>
<c r="B4"><v>75.25</v></c>
<c r="C4" t="s"><v>6</v></c>
<c r="D4" t="s"><v>14</v></c>
<c r="E4" t="s"><v>8</v></c>
<c r="F4" t="s"><v>9</v></c>
</row>
</sheetData>
</worksheet>'''
    xlsx.writestr('xl/worksheets/sheet1.xml', sheet)

print("✅ Fichier Excel créé : test_paiements_v2.xlsx")
PYTHON_SCRIPT

if [ ! -f "test_paiements_v2.xlsx" ]; then
    echo "❌ Erreur lors de la création du fichier Excel"
    exit 1
fi

echo "✅ Fichier Excel créé"
echo ""

# 6. Tester l'upload
echo "6️⃣ Upload du fichier Excel..."
UPLOAD_RESULT=$(curl -k -s -X POST "${BASE_URL}/api/Paiement/bulk-excel?idEcole=${ID_ECOLE}&idUtilisateur=${ID_UTILISATEUR}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "file=@test_paiements_v2.xlsx" \
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
    paiementsCrees: (.paiementsCrees | length)
}'

echo ""
if [ "$SUCCESS" == "true" ] || [ "$LIGNES_REUSSIES" -gt 0 ]; then
    echo "✅ Test réussi !"
    echo "   - ${LIGNES_REUSSIES} paiement(s) créé(s) sur ${TOTAL_LIGNES} ligne(s)"
else
    echo "❌ Test échoué"
    echo "   - ${LIGNES_ECHOUES} erreur(s) sur ${TOTAL_LIGNES} ligne(s)"
    echo ""
    echo "Erreurs détaillées :"
    echo "$UPLOAD_RESULT" | jq '.lignesAvecErreurs[] | {ligne: .numeroLigne, erreurs}'
fi

echo ""
echo "=================================================="
echo "✅ Tests terminés"

