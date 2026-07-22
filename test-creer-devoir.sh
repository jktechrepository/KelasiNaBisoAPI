#!/bin/bash

# Script de test pour créer un devoir à domicile
# Teste avec et sans fichier

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"
ID_ECOLE=13
ID_DIRECTION=20
ID_CLASSE=43

echo "🧪 Test Création Devoir à Domicile"
echo "===================================="
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

# 2. Récupérer les informations de la classe et du cours
echo "2️⃣ Récupération des informations..."
COURS=$(curl -k -s "${BASE_URL}/api/Cours/classe/${ID_CLASSE}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0].idCours // empty')

if [ -z "$COURS" ]; then
    echo "⚠️  Aucun cours trouvé pour la classe ${ID_CLASSE}, création sans cours"
    ID_COURS=""
else
    ID_COURS="$COURS"
    echo "✅ Cours trouvé : ${ID_COURS}"
fi
echo ""

# 3. Test 1 : Créer un devoir SANS fichier (seulement avec contenu textuel)
echo "3️⃣ Test 1 : Création d'un devoir SANS fichier (contenu textuel uniquement)"
echo "─────────────────────────────────────────────────────────────────────────"

DATE_LIMITE=$(date -u -v+7d '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null || date -u -d '+7 days' '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null || echo "$(date -u -d '+7 days' '+%Y-%m-%dT%H:%M:%SZ')")

RESULT1=$(curl -k -s -X POST "${BASE_URL}/api/DevoirADomicile" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "Titre=Devoir de test sans fichier" \
    -F "Description=Ceci est un devoir de test créé sans fichier" \
    -F "Contenu=Voici le contenu textuel du devoir. Les élèves doivent répondre aux questions suivantes :\n1. Question 1\n2. Question 2\n3. Question 3" \
    -F "IdClasse=${ID_CLASSE}" \
    -F "IdCours=${ID_COURS}" \
    -F "DateLimite=${DATE_LIMITE}")

echo "$RESULT1" | jq '.'

SUCCESS1=$(echo "$RESULT1" | jq -r '.idDevoirADomicile // empty')
if [ -n "$SUCCESS1" ]; then
    echo "✅ Devoir créé avec succès (ID: ${SUCCESS1})"
    ID_DEVOIR1="$SUCCESS1"
else
    echo "❌ Échec de la création du devoir"
    echo "$RESULT1" | jq '.'
fi
echo ""

# 4. Test 2 : Créer un devoir SANS fichier ET SANS contenu (seulement titre et description)
echo "4️⃣ Test 2 : Création d'un devoir SANS fichier ET SANS contenu (titre + description uniquement)"
echo "────────────────────────────────────────────────────────────────────────────────────────────"

RESULT2=$(curl -k -s -X POST "${BASE_URL}/api/DevoirADomicile" \
    -H "Authorization: Bearer ${TOKEN}" \
    -F "Titre=Devoir de test minimal" \
    -F "Description=Ceci est un devoir de test créé avec seulement un titre et une description" \
    -F "IdClasse=${ID_CLASSE}" \
    -F "IdCours=${ID_COURS}" \
    -F "DateLimite=${DATE_LIMITE}")

echo "$RESULT2" | jq '.'

SUCCESS2=$(echo "$RESULT2" | jq -r '.idDevoirADomicile // empty')
if [ -n "$SUCCESS2" ]; then
    echo "✅ Devoir créé avec succès (ID: ${SUCCESS2})"
    ID_DEVOIR2="$SUCCESS2"
else
    echo "❌ Échec de la création du devoir"
    echo "$RESULT2" | jq '.'
fi
echo ""

# 5. Test 3 : Créer un devoir AVEC fichier (si un fichier de test existe)
echo "5️⃣ Test 3 : Création d'un devoir AVEC fichier (optionnel)"
echo "─────────────────────────────────────────────────────────"

# Créer un petit fichier PDF de test si nécessaire
if [ ! -f "test_devoir.pdf" ]; then
    echo "📝 Création d'un fichier PDF de test..."
    # Créer un PDF minimal avec Python si disponible
    python3 << 'PYTHON_SCRIPT'
try:
    from reportlab.pdfgen import canvas
    from reportlab.lib.pagesizes import letter
    
    c = canvas.Canvas("test_devoir.pdf", pagesize=letter)
    c.drawString(100, 750, "Devoir de test")
    c.drawString(100, 730, "Ceci est un fichier PDF de test pour le devoir a domicile.")
    c.save()
    print("✅ Fichier PDF créé")
except ImportError:
    # Si reportlab n'est pas disponible, créer un fichier texte simple
    with open("test_devoir.txt", "w") as f:
        f.write("Devoir de test\nCeci est un fichier de test.")
    print("✅ Fichier texte créé (test_devoir.txt)")
except Exception as e:
    print(f"⚠️  Erreur lors de la création du fichier : {e}")
PYTHON_SCRIPT
fi

if [ -f "test_devoir.pdf" ]; then
    RESULT3=$(curl -k -s -X POST "${BASE_URL}/api/DevoirADomicile" \
        -H "Authorization: Bearer ${TOKEN}" \
        -F "Titre=Devoir de test avec fichier PDF" \
        -F "Description=Ceci est un devoir de test créé avec un fichier PDF" \
        -F "IdClasse=${ID_CLASSE}" \
        -F "IdCours=${ID_COURS}" \
        -F "DateLimite=${DATE_LIMITE}" \
        -F "fichier=@test_devoir.pdf")
    
    echo "$RESULT3" | jq '.'
    
    SUCCESS3=$(echo "$RESULT3" | jq -r '.idDevoirADomicile // empty')
    if [ -n "$SUCCESS3" ]; then
        echo "✅ Devoir créé avec succès avec fichier (ID: ${SUCCESS3})"
        ID_DEVOIR3="$SUCCESS3"
    else
        echo "❌ Échec de la création du devoir avec fichier"
        echo "$RESULT3" | jq '.'
    fi
elif [ -f "test_devoir.txt" ]; then
    echo "⚠️  Fichier PDF non disponible, utilisation du fichier texte"
    echo "   (Note: Le fichier texte pourrait être rejeté selon les types autorisés)"
else
    echo "⚠️  Aucun fichier de test disponible, test avec fichier ignoré"
fi
echo ""

# 6. Résumé
echo "📊 Résumé des tests"
echo "=================="
echo ""
if [ -n "$SUCCESS1" ]; then
    echo "✅ Test 1 (sans fichier, avec contenu) : SUCCÈS (ID: ${SUCCESS1})"
else
    echo "❌ Test 1 (sans fichier, avec contenu) : ÉCHEC"
fi

if [ -n "$SUCCESS2" ]; then
    echo "✅ Test 2 (sans fichier, sans contenu) : SUCCÈS (ID: ${SUCCESS2})"
else
    echo "❌ Test 2 (sans fichier, sans contenu) : ÉCHEC"
fi

if [ -n "$SUCCESS3" ]; then
    echo "✅ Test 3 (avec fichier) : SUCCÈS (ID: ${SUCCESS3})"
elif [ -f "test_devoir.pdf" ] || [ -f "test_devoir.txt" ]; then
    echo "❌ Test 3 (avec fichier) : ÉCHEC"
else
    echo "⚠️  Test 3 (avec fichier) : IGNORÉ (fichier de test non disponible)"
fi
echo ""
echo "✅ Tests terminés"

