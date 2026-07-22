#!/bin/bash

# Script pour vérifier les notifications SignalR pour un devoir
# Usage: ./verifier-signalr-devoir.sh [ID_DEVOIR]

ID_DEVOIR=${1:-13}  # ID du devoir par défaut : 13

echo "🔍 Vérification des notifications SignalR pour le devoir ID: ${ID_DEVOIR}"
echo "=================================================================="
echo ""

# Couleurs pour l'affichage
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Vérifier les logs SignalR dans la console
echo -e "${GREEN}📡 1. Logs SignalR pour le devoir ${ID_DEVOIR}${NC}"
echo "─────────────────────────────────────────────"

# Chercher les logs SignalR dans les fichiers de logs
if [ -d "logs" ]; then
    echo "Recherche des logs SignalR..."
    echo ""
    
    # Logs de notifications SignalR
    echo -e "${YELLOW}📲 Notifications SignalR envoyées :${NC}"
    SIGNALR_LOGS=$(grep -h "devoir ${ID_DEVOIR}" logs/log-*.txt 2>/dev/null | grep -i "signalr\|SignalR\|NouveauDevoir" | tail -20)
    
    if [ -n "$SIGNALR_LOGS" ]; then
        echo "$SIGNALR_LOGS"
        echo ""
        echo -e "${GREEN}✅ Des notifications SignalR ont été envoyées${NC}"
    else
        echo -e "${RED}❌ Aucun log SignalR trouvé pour ce devoir${NC}"
    fi
    echo ""
    
    # Vérifier les groupes SignalR
    echo -e "${YELLOW}👥 Groupes SignalR ciblés :${NC}"
    grep -h "devoir ${ID_DEVOIR}" logs/log-*.txt 2>/dev/null | grep -iE "classe_|parents_classe_|ecole_|user_" | tail -10
    echo ""
    
    # Vérifier les connexions au hub
    echo -e "${YELLOW}🔌 Connexions au hub DevoirADomicileHub :${NC}"
    grep -h "DevoirADomicileHub" logs/log-*.txt 2>/dev/null | grep -i "connected\|disconnected" | tail -10
    echo ""
    
    # Logs d'erreurs SignalR
    echo -e "${YELLOW}❌ Erreurs SignalR éventuelles :${NC}"
    ERRORS=$(grep -h "devoir ${ID_DEVOIR}" logs/log-*.txt 2>/dev/null | grep -i "erreur.*signalr\|error.*signalr" | tail -10)
    if [ -n "$ERRORS" ]; then
        echo -e "${RED}$ERRORS${NC}"
    else
        echo -e "${GREEN}Aucune erreur SignalR trouvée${NC}"
    fi
    echo ""
else
    echo -e "${YELLOW}⚠️  Dossier logs/ introuvable${NC}"
    echo "Les logs sont probablement dans la console de l'application"
fi

echo ""
echo "─────────────────────────────────────────────"
echo ""

# 2. Vérifier via l'API
echo -e "${GREEN}🌐 2. Vérification via l'API${NC}"
echo "─────────────────────────────────────────────"

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

echo "Authentification..."
TOKEN=$(curl -k -s -X POST "${BASE_URL}/api/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d "{\"emailOuTelephone\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}" \
    | jq -r '.accessToken' 2>/dev/null)

if [ "$TOKEN" != "null" ] && [ -n "$TOKEN" ]; then
    echo -e "${GREEN}✅ Authentification réussie${NC}"
    echo ""
    
    # Récupérer les détails du devoir
    echo "📚 Détails du devoir :"
    DEVOIR=$(curl -k -s "${BASE_URL}/api/DevoirADomicile/${ID_DEVOIR}" \
        -H "Authorization: Bearer ${TOKEN}" 2>/dev/null)
    
    if [ -n "$DEVOIR" ] && [ "$DEVOIR" != "null" ]; then
        echo "$DEVOIR" | jq '{
            id: .idDevoirADomicile,
            titre: .titre,
            classe: .nomClasse,
            idClasse: .idClasse,
            idEcole: .idEcole,
            datePublication: .datePublication,
            dateLimite: .dateLimite
        }' 2>/dev/null
        echo ""
        
        ID_CLASSE=$(echo "$DEVOIR" | jq -r '.idClasse' 2>/dev/null)
        ID_ECOLE=$(echo "$DEVOIR" | jq -r '.idEcole' 2>/dev/null)
        
        if [ -n "$ID_CLASSE" ] && [ "$ID_CLASSE" != "null" ]; then
            echo -e "${YELLOW}📋 Groupes SignalR qui devraient recevoir la notification :${NC}"
            echo "  - classe_${ID_CLASSE} (élèves et enseignants)"
            echo "  - parents_classe_${ID_CLASSE} (parents de la classe)"
            if [ -n "$ID_ECOLE" ] && [ "$ID_ECOLE" != "null" ]; then
                echo "  - ecole_${ID_ECOLE} (administrateurs)"
            fi
            echo "  - all_users (tous les utilisateurs connectés)"
            echo ""
        fi
    else
        echo -e "${RED}❌ Devoir ${ID_DEVOIR} introuvable${NC}"
    fi
else
    echo -e "${RED}❌ Impossible de s'authentifier${NC}"
fi

echo ""
echo "─────────────────────────────────────────────"
echo ""

# 3. Instructions pour tester en temps réel
echo -e "${GREEN}💡 3. Comment tester SignalR en temps réel${NC}"
echo "─────────────────────────────────────────────"
echo ""
echo "Option 1 - Voir les logs dans la console :"
echo "  Les logs SignalR s'affichent directement dans la console où vous avez lancé 'dotnet run'"
echo ""
echo "Option 2 - Filtrer les logs SignalR :"
echo "  tail -f logs/log-\$(date +%Y%m%d).txt | grep -i 'signalr\|NouveauDevoir\|DevoirADomicileHub'"
echo ""
echo "Option 3 - Voir toutes les connexions au hub :"
echo "  tail -f logs/log-\$(date +%Y%m%d).txt | grep -i 'DevoirADomicileHub.*connected'"
echo ""
echo "Option 4 - Tester avec un client SignalR :"
echo "  Utilisez un client SignalR (ex: SignalR Client) pour vous connecter à :"
echo "  wss://localhost:7102/hubs/devoiradomicile"
echo "  Écoutez les événements : 'NouveauDevoir' et 'NouveauDevoirParent'"
echo ""

echo "─────────────────────────────────────────────"
echo ""
echo -e "${GREEN}✅ Vérification terminée${NC}"
echo ""
echo "📝 Note : Pour vérifier que les clients reçoivent bien les notifications,"
echo "   connectez-vous avec l'application mobile/web et créez un nouveau devoir."

