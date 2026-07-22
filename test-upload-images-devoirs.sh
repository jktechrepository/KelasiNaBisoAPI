#!/bin/bash

# ============================================================================
# SCRIPT DE TEST : Support des Images pour les Devoirs à Domicile
# ============================================================================
# Date : 2025-01-30
# Objectif : Tester l'upload de fichiers PDF, JPG et PNG pour les devoirs
# ============================================================================

set -e

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="https://localhost:7102"
EMAIL="dondej@kelasinabiso.cd"  # Compte Enseignant
PASSWORD="123456"
CLASSE_ID=80  # 5ème Primaire (classe de l'enseignant)

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  TEST : Support des Images pour les Devoirs à Domicile${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# ÉTAPE 1 : Authentification
# ============================================================================

echo -e "${YELLOW}📝 Étape 1 : Authentification...${NC}"

TOKEN=$(curl -k -s -X POST "$BASE_URL/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d "{\"emailOuTelephone\":\"$EMAIL\",\"motDePasse\":\"$PASSWORD\"}" \
  | jq -r '.accessToken')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo -e "${RED}❌ Échec de l'authentification${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Authentification réussie${NC}"
echo ""

# ============================================================================
# ÉTAPE 2 : Créer des fichiers de test
# ============================================================================

echo -e "${YELLOW}📝 Étape 2 : Création des fichiers de test...${NC}"

# Créer un fichier PDF de test (plus grand pour passer la validation)
python3 << 'PYEOF'
# Créer un PDF minimal mais plus grand (au moins 1 KB)
pdf_content = b"%PDF-1.4\n"
pdf_content += b"1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"
pdf_content += b"2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"
pdf_content += b"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>\nendobj\n"
# Ajouter du contenu pour atteindre au moins 1 KB
pdf_content += b"4 0 obj\n<< /Length 1000 >>\nstream\n"
pdf_content += b"x" * 1000  # 1000 bytes de données
pdf_content += b"\nendstream\nendobj\n"
pdf_content += b"xref\n0 5\n"
pdf_content += b"trailer\n<< /Size 5 /Root 1 0 R >>\n"
pdf_content += b"startxref\n"
pdf_content += str(len(pdf_content) + 20).encode()
pdf_content += b"\n%%EOF"
with open('/tmp/test-devoir.pdf', 'wb') as f:
    f.write(pdf_content)
print("✅ PDF créé (taille:", len(pdf_content), "bytes)")
PYEOF

# Créer une image JPG de test (simple, valide avec magic bytes)
# Signature JPEG : FF D8 FF
python3 << 'PYEOF'
# Créer un fichier JPG minimal valide
jpg_data = bytes([
    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
    0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
    0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
    0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
    0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
    0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
    0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
    0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x64,
    0x00, 0x64, 0x03, 0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01,
    0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xFF, 0xC4,
    0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08,
    0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0xD2, 0xBF, 0xFF, 0xD9
])
with open('/tmp/test-devoir.jpg', 'wb') as f:
    f.write(jpg_data)
print("✅ Image JPG créée")
PYEOF

# Créer une image PNG de test (plus grande pour passer la validation)
python3 << 'PYEOF'
# Créer un fichier PNG valide mais plus grand (au moins 100 bytes)
png_header = bytes([
    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
    0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x64,
    0x08, 0x02, 0x00, 0x00, 0x00, 0xFF, 0x80, 0x00, 0x00, 0x00, 0x09, 0x70,
    0x48, 0x59, 0x73, 0x00, 0x00, 0x0B, 0x13, 0x00, 0x00, 0x0B, 0x13, 0x01,
    0x00, 0x9A, 0x9C, 0x18, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
    0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A,
    0x2D, 0xDB, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42,
    0x60, 0x82
])
# Ajouter des données supplémentaires pour atteindre au moins 100 bytes
png_data = png_header + b"\x00" * 50  # Ajouter 50 bytes de padding
with open('/tmp/test-devoir.png', 'wb') as f:
    f.write(png_data)
print("✅ Image PNG créée (taille:", len(png_data), "bytes)")
PYEOF

# Créer un fichier texte (non autorisé)
echo "Ce fichier ne devrait pas être accepté" > /tmp/test-devoir.txt

echo -e "${GREEN}✅ Fichiers de test créés${NC}"
echo ""

# ============================================================================
# ÉTAPE 3 : Test 1 - Upload PDF (devrait fonctionner)
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 1 : Upload d'un fichier PDF${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "Titre=Test PDF - Support Images" \
  -F "Description=Test d'upload d'un fichier PDF" \
  -F "IdClasse=$CLASSE_ID" \
  -F "DateLimite=2025-02-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir.pdf")

DEVOIR_ID_PDF=$(echo "$RESPONSE" | jq -r '.idDevoirADomicile // empty')

if [ -n "$DEVOIR_ID_PDF" ] && [ "$DEVOIR_ID_PDF" != "null" ]; then
  echo -e "${GREEN}✅ PDF uploadé avec succès (ID: $DEVOIR_ID_PDF)${NC}"
  echo "$RESPONSE" | jq '{id: .idDevoirADomicile, titre: .titre, nomFichier: .nomFichier, typeMIME: .typeMIME}'
else
  echo -e "${RED}❌ Échec de l'upload PDF${NC}"
  echo "$RESPONSE" | jq '.'
fi
echo ""

# ============================================================================
# ÉTAPE 4 : Test 2 - Upload JPG (nouveau, devrait fonctionner)
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 2 : Upload d'une image JPG${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "Titre=Test JPG - Support Images" \
  -F "Description=Test d'upload d'une image JPG" \
  -F "IdClasse=$CLASSE_ID" \
  -F "DateLimite=2025-02-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir.jpg")

DEVOIR_ID_JPG=$(echo "$RESPONSE" | jq -r '.idDevoirADomicile // empty')

if [ -n "$DEVOIR_ID_JPG" ] && [ "$DEVOIR_ID_JPG" != "null" ]; then
  echo -e "${GREEN}✅ JPG uploadé avec succès (ID: $DEVOIR_ID_JPG)${NC}"
  echo "$RESPONSE" | jq '{id: .idDevoirADomicile, titre: .titre, nomFichier: .nomFichier, typeMIME: .typeMIME}'
else
  echo -e "${RED}❌ Échec de l'upload JPG${NC}"
  echo "$RESPONSE" | jq '.'
fi
echo ""

# ============================================================================
# ÉTAPE 5 : Test 3 - Upload PNG (nouveau, devrait fonctionner)
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 3 : Upload d'une image PNG${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "Titre=Test PNG - Support Images" \
  -F "Description=Test d'upload d'une image PNG" \
  -F "IdClasse=$CLASSE_ID" \
  -F "DateLimite=2025-02-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir.png")

DEVOIR_ID_PNG=$(echo "$RESPONSE" | jq -r '.idDevoirADomicile // empty')

if [ -n "$DEVOIR_ID_PNG" ] && [ "$DEVOIR_ID_PNG" != "null" ]; then
  echo -e "${GREEN}✅ PNG uploadé avec succès (ID: $DEVOIR_ID_PNG)${NC}"
  echo "$RESPONSE" | jq '{id: .idDevoirADomicile, titre: .titre, nomFichier: .nomFichier, typeMIME: .typeMIME}'
else
  echo -e "${RED}❌ Échec de l'upload PNG${NC}"
  echo "$RESPONSE" | jq '.'
fi
echo ""

# ============================================================================
# ÉTAPE 6 : Test 4 - Upload fichier non autorisé (devrait être rejeté)
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 4 : Upload d'un fichier non autorisé (.txt)${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

RESPONSE=$(curl -k -s -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "Titre=Test TXT - Devrait être rejeté" \
  -F "Description=Test d'upload d'un fichier .txt (non autorisé)" \
  -F "IdClasse=$CLASSE_ID" \
  -F "DateLimite=2025-02-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir.txt")

HTTP_CODE=$(curl -k -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "Titre=Test TXT" \
  -F "IdClasse=$CLASSE_ID" \
  -F "DateLimite=2025-02-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir.txt")

if [ "$HTTP_CODE" == "400" ]; then
  echo -e "${GREEN}✅ Fichier .txt correctement rejeté (Code HTTP: $HTTP_CODE)${NC}"
  echo "$RESPONSE" | jq '.message // .'
else
  echo -e "${RED}❌ Problème : Le fichier .txt devrait être rejeté (Code HTTP: $HTTP_CODE)${NC}"
  echo "$RESPONSE" | jq '.'
fi
echo ""

# ============================================================================
# ÉTAPE 7 : Test 5 - Téléchargement des fichiers uploadés
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 5 : Téléchargement des fichiers uploadés${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

# Télécharger le PDF
if [ -n "$DEVOIR_ID_PDF" ] && [ "$DEVOIR_ID_PDF" != "null" ]; then
  HTTP_CODE=$(curl -k -s -o /tmp/downloaded-pdf.pdf -w "%{http_code}" \
    -X GET "$BASE_URL/api/DevoirADomicile/$DEVOIR_ID_PDF/telecharger" \
    -H "Authorization: Bearer $TOKEN")
  
  if [ "$HTTP_CODE" == "200" ] && [ -f /tmp/downloaded-pdf.pdf ]; then
    echo -e "${GREEN}✅ PDF téléchargé avec succès (${HTTP_CODE})${NC}"
    ls -lh /tmp/downloaded-pdf.pdf
  else
    echo -e "${RED}❌ Échec du téléchargement PDF (${HTTP_CODE})${NC}"
  fi
fi

# Télécharger le JPG
if [ -n "$DEVOIR_ID_JPG" ] && [ "$DEVOIR_ID_JPG" != "null" ]; then
  HTTP_CODE=$(curl -k -s -o /tmp/downloaded-jpg.jpg -w "%{http_code}" \
    -X GET "$BASE_URL/api/DevoirADomicile/$DEVOIR_ID_JPG/telecharger" \
    -H "Authorization: Bearer $TOKEN")
  
  if [ "$HTTP_CODE" == "200" ] && [ -f /tmp/downloaded-jpg.jpg ]; then
    echo -e "${GREEN}✅ JPG téléchargé avec succès (${HTTP_CODE})${NC}"
    ls -lh /tmp/downloaded-jpg.jpg
  else
    echo -e "${RED}❌ Échec du téléchargement JPG (${HTTP_CODE})${NC}"
  fi
fi

# Télécharger le PNG
if [ -n "$DEVOIR_ID_PNG" ] && [ "$DEVOIR_ID_PNG" != "null" ]; then
  HTTP_CODE=$(curl -k -s -o /tmp/downloaded-png.png -w "%{http_code}" \
    -X GET "$BASE_URL/api/DevoirADomicile/$DEVOIR_ID_PNG/telecharger" \
    -H "Authorization: Bearer $TOKEN")
  
  if [ "$HTTP_CODE" == "200" ] && [ -f /tmp/downloaded-png.png ]; then
    echo -e "${GREEN}✅ PNG téléchargé avec succès (${HTTP_CODE})${NC}"
    ls -lh /tmp/downloaded-png.png
  else
    echo -e "${RED}❌ Échec du téléchargement PNG (${HTTP_CODE})${NC}"
  fi
fi
echo ""

# ============================================================================
# RÉSUMÉ FINAL
# ============================================================================

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  RÉSUMÉ DES TESTS${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"

SUCCESS_COUNT=0
FAIL_COUNT=0

if [ -n "$DEVOIR_ID_PDF" ] && [ "$DEVOIR_ID_PDF" != "null" ]; then
  echo -e "${GREEN}✅ Test 1 (PDF) : RÉUSSI${NC}"
  ((SUCCESS_COUNT++))
else
  echo -e "${RED}❌ Test 1 (PDF) : ÉCHEC${NC}"
  ((FAIL_COUNT++))
fi

if [ -n "$DEVOIR_ID_JPG" ] && [ "$DEVOIR_ID_JPG" != "null" ]; then
  echo -e "${GREEN}✅ Test 2 (JPG) : RÉUSSI${NC}"
  ((SUCCESS_COUNT++))
else
  echo -e "${RED}❌ Test 2 (JPG) : ÉCHEC${NC}"
  ((FAIL_COUNT++))
fi

if [ -n "$DEVOIR_ID_PNG" ] && [ "$DEVOIR_ID_PNG" != "null" ]; then
  echo -e "${GREEN}✅ Test 3 (PNG) : RÉUSSI${NC}"
  ((SUCCESS_COUNT++))
else
  echo -e "${RED}❌ Test 3 (PNG) : ÉCHEC${NC}"
  ((FAIL_COUNT++))
fi

echo ""
echo -e "${BLUE}Total : ${GREEN}$SUCCESS_COUNT réussis${NC} / ${RED}$FAIL_COUNT échecs${NC}"
echo ""

# Nettoyage
echo -e "${YELLOW}🧹 Nettoyage des fichiers temporaires...${NC}"
rm -f /tmp/test-devoir.* /tmp/downloaded-*.* 2>/dev/null || true
echo -e "${GREEN}✅ Nettoyage terminé${NC}"

