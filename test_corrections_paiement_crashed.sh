#!/bin/bash

# 🧪 Script de Test des Corrections PaiementCrashed
# Date : 12 décembre 2024
# Objectif : Valider les corrections apportées à SaveCrashedPaiementsAsync

set -e

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="${BASE_URL:-https://localhost:7102}"
API_URL="${BASE_URL}/api"

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🧪 Tests des Corrections PaiementCrashed${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# Fonction pour afficher les résultats
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Vérifier que curl est installé
if ! command -v curl &> /dev/null; then
    print_error "curl n'est pas installé. Veuillez l'installer d'abord."
    exit 1
fi

# Vérifier que Python est installé
if ! command -v python3 &> /dev/null; then
    print_error "Python3 n'est pas installé. Veuillez l'installer d'abord."
    exit 1
fi

# Étape 1 : Authentification
print_info "Étape 1 : Authentification..."
echo ""

read -p "Email de l'utilisateur de test : " EMAIL
read -sp "Mot de passe : " PASSWORD
echo ""

AUTH_RESPONSE=$(curl -s -X POST "${API_URL}/Utilisateurs/Authentifier" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}")

TOKEN=$(echo $AUTH_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    print_error "Échec de l'authentification"
    echo "Réponse : $AUTH_RESPONSE"
    exit 1
fi

print_success "Authentification réussie"
echo ""

# Étape 2 : Récupérer les informations de l'utilisateur
print_info "Étape 2 : Récupération des informations utilisateur..."
echo ""

USER_INFO=$(curl -s -X GET "${API_URL}/Utilisateurs/me" \
  -H "Authorization: Bearer ${TOKEN}")

ID_ECOLE=$(echo $USER_INFO | grep -o '"idEcole":[0-9]*' | cut -d':' -f2)
ID_UTILISATEUR=$(echo $USER_INFO | grep -o '"idUtilisateur":[0-9]*' | cut -d':' -f2)

if [ -z "$ID_ECOLE" ] || [ -z "$ID_UTILISATEUR" ]; then
    print_error "Impossible de récupérer les informations utilisateur"
    echo "Réponse : $USER_INFO"
    exit 1
fi

print_success "ID École : $ID_ECOLE"
print_success "ID Utilisateur : $ID_UTILISATEUR"
echo ""

# Étape 3 : Créer un fichier Excel de test avec des données invalides
print_info "Étape 3 : Création d'un fichier Excel de test avec des données invalides..."
echo ""

python3 << EOF
from openpyxl import Workbook
from datetime import datetime

wb = Workbook()
ws = wb.active

# En-têtes
headers = ["DatePaiement", "Montant", "Devise", "ModePaiement", "NomCompletEleve", "LibelleFrais"]
ws.append(headers)

# Ligne 1 : Élève inexistant (doit échouer)
ws.append([
    datetime.now(),
    100,
    "USD",
    "Cash",
    "ELEVE_INEXISTANT_XYZ",
    "Frais Test"
])

# Ligne 2 : Frais inexistant (doit échouer)
ws.append([
    datetime.now(),
    200,
    "CDF",
    "Mobile Money",
    "ELEVE_EXISTANT",  # À remplacer par un nom réel
    "FRAIS_INEXISTANT_XYZ"
])

# Ligne 3 : DatePaiement null (doit être accepté)
ws.append([
    None,
    300,
    "USD",
    "Cash",
    "ELEVE_INEXISTANT_XYZ",
    "Frais Test"
])

# Ligne 4 : Montant invalide (doit échouer)
ws.append([
    datetime.now(),
    -50,  # Montant négatif
    "USD",
    "Cash",
    "ELEVE_INEXISTANT_XYZ",
    "Frais Test"
])

wb.save("test_paiements_crashed.xlsx")
print("✅ Fichier Excel créé : test_paiements_crashed.xlsx")
EOF

if [ ! -f "test_paiements_crashed.xlsx" ]; then
    print_error "Échec de la création du fichier Excel"
    exit 1
fi

print_success "Fichier Excel créé : test_paiements_crashed.xlsx"
echo ""

# Étape 4 : Uploader le fichier
print_info "Étape 4 : Upload du fichier Excel..."
echo ""

UPLOAD_RESPONSE=$(curl -s -X POST "${API_URL}/Paiement/bulk-excel" \
  -H "Authorization: Bearer ${TOKEN}" \
  -F "file=@test_paiements_crashed.xlsx")

echo "Réponse de l'upload :"
echo "$UPLOAD_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$UPLOAD_RESPONSE"
echo ""

# Vérifier le résultat
SUCCESS=$(echo $UPLOAD_RESPONSE | grep -o '"success":[^,}]*' | cut -d':' -f2)
LIGNES_ECHOUEES=$(echo $UPLOAD_RESPONSE | grep -o '"lignesEchouees":[0-9]*' | cut -d':' -f2)

if [ -z "$LIGNES_ECHOUEES" ]; then
    print_warning "Impossible de déterminer le nombre de lignes échouées"
else
    print_info "Lignes échouées : $LIGNES_ECHOUEES"
fi

echo ""

# Étape 5 : Vérifier les paiements échoués dans PaiementsCrashed
print_info "Étape 5 : Vérification des paiements échoués dans PaiementsCrashed..."
echo ""

CRASHED_RESPONSE=$(curl -s -X GET "${API_URL}/PaiementCrashed/ecole" \
  -H "Authorization: Bearer ${TOKEN}")

echo "Paiements échoués :"
echo "$CRASHED_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$CRASHED_RESPONSE"
echo ""

# Vérifier que les paiements échoués ont été sauvegardés
CRASHED_COUNT=$(echo $CRASHED_RESPONSE | grep -o '"total":[0-9]*' | cut -d':' -f2)

if [ -z "$CRASHED_COUNT" ] || [ "$CRASHED_COUNT" = "0" ]; then
    print_warning "Aucun paiement échoué trouvé dans PaiementsCrashed"
    print_warning "Cela peut indiquer un problème avec SaveCrashedPaiementsAsync"
else
    print_success "Paiements échoués trouvés : $CRASHED_COUNT"
    
    # Vérifier les champs critiques
    echo ""
    print_info "Vérification des champs critiques..."
    
    # Vérifier ErreursJson
    ERREURS_JSON=$(echo $CRASHED_RESPONSE | grep -o '"erreursJson":"[^"]*' | head -1 | cut -d'"' -f4)
    if [ -z "$ERREURS_JSON" ] || [ "$ERREURS_JSON" = "null" ]; then
        print_error "ErreursJson est null ou vide - PROBLÈME DÉTECTÉ"
    else
        print_success "ErreursJson est présent : ${ERREURS_JSON:0:50}..."
    fi
    
    # Vérifier DateEchec
    DATE_ECHEC=$(echo $CRASHED_RESPONSE | grep -o '"dateEchec":"[^"]*' | head -1 | cut -d'"' -f4)
    if [ -z "$DATE_ECHEC" ] || [ "$DATE_ECHEC" = "null" ]; then
        print_warning "DateEchec est null - Vérifier la correction"
    else
        print_success "DateEchec est présent : $DATE_ECHEC"
    fi
    
    # Vérifier DateCreation
    DATE_CREATION=$(echo $CRASHED_RESPONSE | grep -o '"dateCreation":"[^"]*' | head -1 | cut -d'"' -f4)
    if [ -z "$DATE_CREATION" ] || [ "$DATE_CREATION" = "null" ]; then
        print_warning "DateCreation est null - Vérifier la correction"
    else
        print_success "DateCreation est présent : $DATE_CREATION"
    fi
    
    # Vérifier NumeroLigne
    NUMERO_LIGNE=$(echo $CRASHED_RESPONSE | grep -o '"numeroLigne":[0-9]*' | head -1 | cut -d':' -f2)
    if [ -z "$NUMERO_LIGNE" ]; then
        print_warning "NumeroLigne est absent"
    else
        if [ "$NUMERO_LIGNE" -lt 0 ]; then
            print_error "NumeroLigne est négatif ($NUMERO_LIGNE) - PROBLÈME DÉTECTÉ"
        else
            print_success "NumeroLigne est valide : $NUMERO_LIGNE"
        fi
    fi
fi

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}📊 Résumé des Tests${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""

if [ -n "$CRASHED_COUNT" ] && [ "$CRASHED_COUNT" != "0" ]; then
    print_success "Les paiements échoués ont été sauvegardés dans PaiementsCrashed"
    print_success "Les corrections semblent fonctionner correctement"
else
    print_warning "Aucun paiement échoué sauvegardé"
    print_warning "Vérifier les logs de l'application pour plus de détails"
fi

echo ""
print_info "Nettoyage du fichier de test..."
rm -f test_paiements_crashed.xlsx
print_success "Fichier de test supprimé"

echo ""
echo -e "${GREEN}✅ Tests terminés${NC}"










