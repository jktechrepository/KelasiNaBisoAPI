#!/bin/bash

# Script pour vérifier les logs de notifications push pour un devoir
# Usage: ./verifier-notifications-devoir.sh [ID_DEVOIR]

ID_DEVOIR=${1:-11}  # ID du devoir par défaut : 11

echo "🔍 Vérification des notifications push pour le devoir ID: ${ID_DEVOIR}"
echo "=================================================================="
echo ""

# 1. Vérifier les logs dans les fichiers
echo "📄 1. Logs dans les fichiers (logs/log-*.txt)"
echo "─────────────────────────────────────────────"

if [ -d "logs" ]; then
    # Chercher les logs récents concernant ce devoir
    echo "Recherche des logs pour devoir ${ID_DEVOIR}..."
    echo ""
    
    # Logs de notifications push
    echo "📲 Notifications Push :"
    grep -h "devoir ${ID_DEVOIR}" logs/log-*.txt 2>/dev/null | grep -i "notification\|push\|firebase" | tail -20
    echo ""
    
    # Logs de recherche de parents
    echo "👥 Recherche de parents :"
    grep -h "devoir ${ID_DEVOIR}" logs/log-*.txt 2>/dev/null | grep -i "parent\|recherche" | tail -10
    echo ""
    
    # Logs d'erreurs
    echo "❌ Erreurs éventuelles :"
    grep -h "devoir ${ID_DEVOIR}" logs/errors-*.txt 2>/dev/null | tail -10
    echo ""
    
    # Logs récents généraux
    echo "📋 Logs récents (dernières 30 lignes) :"
    tail -30 logs/log-*.txt 2>/dev/null | grep -i "devoir\|notification\|push" | tail -20
else
    echo "⚠️  Dossier logs/ introuvable"
fi

echo ""
echo "─────────────────────────────────────────────"
echo ""

# 2. Vérifier via l'API (si disponible)
echo "🌐 2. Vérification via l'API"
echo "─────────────────────────────────────────────"

BASE_URL="https://localhost:7102"
EMAIL="jk2@kelasinabiso.cd"
PASSWORD="12345678"

TOKEN=$(curl -k -s -X POST "${BASE_URL}/api/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d "{\"emailOuTelephone\":\"${EMAIL}\",\"motDePasse\":\"${PASSWORD}\"}" \
    | jq -r '.accessToken')

if [ "$TOKEN" != "null" ] && [ -n "$TOKEN" ]; then
    echo "✅ Authentification réussie"
    echo ""
    
    # Récupérer les détails du devoir
    echo "📚 Détails du devoir :"
    DEVOIR=$(curl -k -s "${BASE_URL}/api/DevoirADomicile/${ID_DEVOIR}" \
        -H "Authorization: Bearer ${TOKEN}")
    
    echo "$DEVOIR" | jq '{
        id: .idDevoirADomicile,
        titre: .titre,
        classe: .nomClasse,
        datePublication: .datePublication
    }'
    echo ""
    
    # Vérifier s'il y a des parents dans la classe
    ID_CLASSE=$(echo "$DEVOIR" | jq -r '.idClasse')
    if [ -n "$ID_CLASSE" ] && [ "$ID_CLASSE" != "null" ]; then
        echo "👥 Vérification des parents de la classe ${ID_CLASSE}..."
        # Note: Il faudrait un endpoint pour récupérer les parents d'une classe
        echo "   (Endpoint pour récupérer les parents non disponible dans cette version)"
    fi
else
    echo "❌ Impossible de s'authentifier"
fi

echo ""
echo "─────────────────────────────────────────────"
echo ""

# 3. Instructions pour consulter les logs en temps réel
echo "💡 3. Comment consulter les logs en temps réel"
echo "─────────────────────────────────────────────"
echo ""
echo "Option 1 - Console de l'application :"
echo "  Les logs s'affichent directement dans la console où vous avez lancé 'dotnet run'"
echo ""
echo "Option 2 - Fichiers de logs :"
echo "  tail -f logs/log-\$(date +%Y%m%d).txt"
echo ""
echo "Option 3 - Filtrer les notifications push :"
echo "  tail -f logs/log-\$(date +%Y%m%d).txt | grep -i 'notification\|push\|devoir'"
echo ""
echo "Option 4 - Voir toutes les erreurs :"
echo "  tail -f logs/errors-\$(date +%Y%m%d).txt"
echo ""

echo "✅ Vérification terminée"

