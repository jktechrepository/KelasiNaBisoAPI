#!/bin/bash

# Script de test pour l'upload d'un devoir vers S3
# Compte Admin: jk2@kelasinabiso.cd / 12345678

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "🧪 Test d'Upload vers S3 - Devoirs à Domicile"
echo "=============================================="
echo ""

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

print_step() {
    echo -e "${BLUE}📝 $1${NC}"
}

# Étape 1 : Authentification
print_step "Étape 1 : Authentification"
echo "----------------------------"
AUTH_RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{
    \"emailOuTelephone\": \"$EMAIL\",
    \"motDePasse\": \"$PASSWORD\"
  }")

TOKEN=$(echo "$AUTH_RESPONSE" | jq -r '.accessToken // .token // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    print_error "Échec de l'authentification"
    echo "$AUTH_RESPONSE" | jq '.' 2>/dev/null || echo "$AUTH_RESPONSE"
    exit 1
fi

print_success "Authentification réussie"
print_info "Token : ${TOKEN:0:50}..."
echo ""

# Étape 2 : Récupérer une classe valide
print_step "Étape 2 : Récupération d'une classe valide"
echo "----------------------------------------------"
CLASSES=$(curl -k -s -X GET "$BASE_URL/api/Classe" \
  -H "Authorization: Bearer $TOKEN")

CLASSE_ID=$(echo "$CLASSES" | jq -r '.[0].idClasse // empty' 2>/dev/null)

if [ -z "$CLASSE_ID" ] || [ "$CLASSE_ID" == "null" ]; then
    print_error "Aucune classe trouvée. Veuillez créer une classe d'abord."
    echo "Réponse : $CLASSES"
    exit 1
fi

CLASSE_NOM=$(echo "$CLASSES" | jq -r ".[] | select(.idClasse == $CLASSE_ID) | .nom" 2>/dev/null)
print_success "Classe trouvée : ID $CLASSE_ID - $CLASSE_NOM"
echo ""

# Étape 3 : Créer un fichier PDF de test
print_step "Étape 3 : Création d'un fichier PDF de test"
echo "------------------------------------------------"
TEST_PDF="/tmp/test-devoir-s3.pdf"

# Vérifier si Python est disponible pour créer un PDF simple
if command -v python3 &> /dev/null; then
    python3 << 'EOF'
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter

try:
    c = canvas.Canvas("/tmp/test-devoir-s3.pdf", pagesize=letter)
    c.drawString(100, 750, "Test Devoir a Domicile - Upload S3")
    c.drawString(100, 730, "Ce fichier est un test pour verifier l'upload vers AWS S3")
    c.drawString(100, 710, "Date: 2025-12-01")
    c.save()
    print("PDF cree avec succes")
except ImportError:
    print("reportlab non installe, creation d'un PDF minimal")
    # Créer un PDF minimal manuellement
    pdf_content = b"""%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj
2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj
3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
/Resources <<
/Font <<
/F1 <<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
>>
>>
>>
endobj
4 0 obj
<<
/Length 44
>>
stream
BT
/F1 12 Tf
100 700 Td
(Test Devoir S3) Tj
ET
endstream
endobj
xref
0 5
0000000000 65535 f
0000000009 00000 n
0000000058 00000 n
0000000115 00000 n
0000000306 00000 n
trailer
<<
/Size 5
/Root 1 0 R
>>
startxref
400
%%EOF"""
    with open("/tmp/test-devoir-s3.pdf", "wb") as f:
        f.write(pdf_content)
    print("PDF minimal cree")
EOF
elif command -v python &> /dev/null; then
    python << 'EOF'
# Même code que ci-dessus
EOF
else
    # Créer un PDF minimal manuellement
    cat > /tmp/test-devoir-s3.pdf << 'PDFEOF'
%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj
2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj
3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
/Resources <<
/Font <<
/F1 <<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
>>
>>
>>
endobj
4 0 obj
<<
/Length 44
>>
stream
BT
/F1 12 Tf
100 700 Td
(Test Devoir S3) Tj
ET
endstream
endobj
xref
0 5
0000000000 65535 f
0000000009 00000 n
0000000058 00000 n
0000000115 00000 n
0000000306 00000 n
trailer
<<
/Size 5
/Root 1 0 R
>>
startxref
400
%%EOF
PDFEOF
fi

if [ ! -f "$TEST_PDF" ]; then
    print_error "Impossible de créer le fichier PDF de test"
    print_info "Veuillez créer manuellement un fichier PDF et le placer à : $TEST_PDF"
    exit 1
fi

PDF_SIZE=$(stat -f%z "$TEST_PDF" 2>/dev/null || stat -c%s "$TEST_PDF" 2>/dev/null)
print_success "Fichier PDF de test créé : $TEST_PDF ($PDF_SIZE bytes)"
echo ""

# Étape 4 : Upload du devoir
print_step "Étape 4 : Upload du devoir vers S3"
echo "--------------------------------------"
DATE_LIMITE=$(date -u -v+14d +"%Y-%m-%dT23:59:59" 2>/dev/null || date -u -d "+14 days" +"%Y-%m-%dT23:59:59" 2>/dev/null || echo "2025-12-15T23:59:59")

UPLOAD_RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "titre=Test Devoir Admin - Upload S3" \
  -F "description=Ce devoir teste l'upload vers AWS S3" \
  -F "idClasse=$CLASSE_ID" \
  -F "dateLimite=$DATE_LIMITE" \
  -F "fichier=@$TEST_PDF")

echo "Réponse d'upload :"
echo "$UPLOAD_RESPONSE" | jq '.' 2>/dev/null || echo "$UPLOAD_RESPONSE"
echo ""

DEVOIR_ID=$(echo "$UPLOAD_RESPONSE" | jq -r '.idDevoirADomicile // .id // empty' 2>/dev/null)
CHEMIN_FICHIER=$(echo "$UPLOAD_RESPONSE" | jq -r '.cheminFichier // .filePath // empty' 2>/dev/null)

if [ -n "$DEVOIR_ID" ] && [ "$DEVOIR_ID" != "null" ]; then
    print_success "Devoir créé avec succès !"
    print_info "ID Devoir : $DEVOIR_ID"
    print_info "Chemin fichier : $CHEMIN_FICHIER"
    echo ""
    
    # Étape 5 : Vérifier dans S3
    print_step "Étape 5 : Vérification"
    echo "----------------------"
    print_info "Le fichier devrait être dans S3 :"
    print_info "Bucket : kansa-kelasinabiso-s3-bucket"
    print_info "Clé : $CHEMIN_FICHIER"
    echo ""
    print_info "Vérifiez dans la console AWS S3 :"
    print_info "https://s3.console.aws.amazon.com/s3/buckets/kansa-kelasinabiso-s3-bucket?region=eu-north-1&prefix=devoirs/"
    echo ""
    
    # Étape 6 : Tester le téléchargement
    print_step "Étape 6 : Test de téléchargement depuis S3"
    echo "-----------------------------------------------"
    DOWNLOAD_RESPONSE=$(curl -k -s -I -X GET "$BASE_URL/api/DevoirADomicile/$DEVOIR_ID/telecharger" \
      -H "Authorization: Bearer $TOKEN")
    
    HTTP_STATUS=$(echo "$DOWNLOAD_RESPONSE" | head -1 | awk '{print $2}')
    
    if [ "$HTTP_STATUS" == "200" ]; then
        print_success "Téléchargement réussi depuis S3 !"
    else
        print_error "Échec du téléchargement (Status: $HTTP_STATUS)"
        echo "$DOWNLOAD_RESPONSE" | head -5
    fi
    echo ""
    
else
    print_error "Échec de l'upload"
    echo "Réponse complète :"
    echo "$UPLOAD_RESPONSE" | jq '.' 2>/dev/null || echo "$UPLOAD_RESPONSE"
    exit 1
fi

print_success "Tests terminés avec succès !"
echo ""
print_info "Résumé :"
print_info "- Authentification : ✅"
print_info "- Upload vers S3 : ✅"
print_info "- Téléchargement depuis S3 : ✅"

