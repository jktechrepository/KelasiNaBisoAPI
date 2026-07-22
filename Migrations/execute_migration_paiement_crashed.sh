#!/bin/bash

# ============================================================
# Script d'exécution de la migration PaiementCrashed
# ============================================================

set -e  # Arrêter en cas d'erreur

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Migration PaiementCrashed - Dates Nullable${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Configuration (à adapter selon votre environnement)
DB_NAME="dev-knb_db"
DB_USER="kansa"
DB_HOST="localhost"
DB_PORT="3306"
SCRIPT_PATH="$(dirname "$0")/MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql"

# Demander le mot de passe
echo -e "${YELLOW}⚠️  Entrez le mot de passe MySQL pour l'utilisateur '$DB_USER':${NC}"
read -s DB_PASSWORD
echo ""

# Créer un fichier de sauvegarde
BACKUP_FILE="backup_paiement_crashed_$(date +%Y%m%d_%H%M%S).sql"
echo -e "${YELLOW}📦 Création d'une sauvegarde...${NC}"
mysqldump -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" PaiementsCrashed > "$BACKUP_FILE" 2>/dev/null || {
    echo -e "${RED}❌ Erreur lors de la création de la sauvegarde${NC}"
    echo "Continuer quand même ? (o/n)"
    read -r response
    if [[ ! "$response" =~ ^[Oo]$ ]]; then
        exit 1
    fi
}

echo -e "${GREEN}✅ Sauvegarde créée : $BACKUP_FILE${NC}"
echo ""

# Vérifier que le script SQL existe
if [ ! -f "$SCRIPT_PATH" ]; then
    echo -e "${RED}❌ Erreur : Le fichier $SCRIPT_PATH n'existe pas${NC}"
    exit 1
fi

# Afficher un résumé
echo -e "${YELLOW}📋 Résumé de la migration :${NC}"
echo "  - Base de données : $DB_NAME"
echo "  - Table : PaiementsCrashed"
echo "  - Colonnes à modifier : DateEchec, DateCreation"
echo "  - Action : Rendre nullable"
echo ""

# Demander confirmation
echo -e "${YELLOW}⚠️  Voulez-vous continuer ? (o/n)${NC}"
read -r response
if [[ ! "$response" =~ ^[Oo]$ ]]; then
    echo "Annulé."
    exit 0
fi

echo ""
echo -e "${YELLOW}🚀 Exécution du script SQL...${NC}"

# Exécuter le script
mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" < "$SCRIPT_PATH" && {
    echo ""
    echo -e "${GREEN}✅ Migration terminée avec succès !${NC}"
    echo ""
    echo -e "${GREEN}📊 Vérification des résultats :${NC}"
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" -e "
        SELECT 
            COLUMN_NAME, 
            IS_NULLABLE, 
            COLUMN_TYPE
        FROM 
            INFORMATION_SCHEMA.COLUMNS 
        WHERE 
            TABLE_SCHEMA = '$DB_NAME' AND 
            TABLE_NAME = 'PaiementsCrashed' AND 
            COLUMN_NAME IN ('DateEchec', 'DateCreation');
    "
    echo ""
    echo -e "${GREEN}✅ Les colonnes sont maintenant NULLABLE${NC}"
    echo -e "${YELLOW}📝 N'oubliez pas de redéployer l'application !${NC}"
} || {
    echo ""
    echo -e "${RED}❌ Erreur lors de l'exécution du script${NC}"
    echo -e "${YELLOW}💾 La sauvegarde est disponible dans : $BACKUP_FILE${NC}"
    exit 1
}

