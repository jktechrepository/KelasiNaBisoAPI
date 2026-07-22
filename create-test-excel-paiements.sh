#!/bin/bash

# Script pour créer un fichier Excel de test avec des données valides
# Nécessite : python3 avec openpyxl OU utilisation du template modifié

TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/Authentifier" \
    -H "Content-Type: application/json" \
    -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}' \
    | jq -r '.accessToken')

# Récupérer des élèves valides
ELEVES=$(curl -k -s "https://localhost:7102/api/Eleve/paged?pageSize=5" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.data[0:5] | .[] | "\(.idEleve)"')

# Récupérer des frais valides
FRAIS=$(curl -k -s "https://localhost:7102/api/Frais" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0:5] | .[] | "\(.idFrais)"')

echo "Élèves disponibles :"
echo "$ELEVES" | head -3
echo ""
echo "Frais disponibles :"
echo "$FRAIS" | head -3

# Prendre les premiers IDs
ID_ELEVE_1=$(echo "$ELEVES" | head -1)
ID_ELEVE_2=$(echo "$ELEVES" | head -2 | tail -1)
ID_FRAIS_1=$(echo "$FRAIS" | head -1)
ID_FRAIS_2=$(echo "$FRAIS" | head -2 | tail -1)

echo ""
echo "IDs sélectionnés :"
echo "  IdEleve 1: $ID_ELEVE_1"
echo "  IdEleve 2: $ID_ELEVE_2"
echo "  IdFrais 1: $ID_FRAIS_1"
echo "  IdFrais 2: $ID_FRAIS_2"

# Créer un fichier Python temporaire pour générer l'Excel
cat > /tmp/create_excel_paiements.py << EOF
import openpyxl
from openpyxl.styles import Font, PatternFill, Alignment
from datetime import datetime

wb = openpyxl.Workbook()
ws = wb.active
ws.title = "Paiements"

headers = [
    "DatePaiement", "Montant", "Devise", "ModePaiement", "StatutPaiement",
    "ReferenceTransaction", "JustificatifUrl", "Commentaire",
    "IdEleve", "IdFrais", "IdUtilisateur", "Statut"
]

header_fill = PatternFill(start_color="4472C4", end_color="4472C4", fill_type="solid")
header_font = Font(bold=True, color="FFFFFF")

for col_idx, header in enumerate(headers, start=1):
    cell = ws.cell(row=1, column=col_idx, value=header)
    cell.fill = header_fill
    cell.font = header_font
    cell.alignment = Alignment(horizontal="center", vertical="center")

# Données valides
today = datetime.now()
test_data = [
    {
        "DatePaiement": today,
        "Montant": 100.00,
        "Devise": "USD",
        "ModePaiement": "Cash",
        "StatutPaiement": "Confirmé",
        "ReferenceTransaction": "REF-TEST-001",
        "JustificatifUrl": None,
        "Commentaire": "Paiement test valide 1",
        "IdEleve": $ID_ELEVE_1,
        "IdFrais": $ID_FRAIS_1,
        "IdUtilisateur": None,
        "Statut": True
    },
    {
        "DatePaiement": today,
        "Montant": 150.50,
        "Devise": "CDF",
        "ModePaiement": "Mobile Money",
        "StatutPaiement": "Confirmé",
        "ReferenceTransaction": "REF-TEST-002",
        "JustificatifUrl": None,
        "Commentaire": "Paiement test valide 2",
        "IdEleve": $ID_ELEVE_2,
        "IdFrais": $ID_FRAIS_2,
        "IdUtilisateur": None,
        "Statut": True
    },
    {
        "DatePaiement": today,
        "Montant": 75.25,
        "Devise": "USD",
        "ModePaiement": "Carte",
        "StatutPaiement": "Confirmé",
        "ReferenceTransaction": "REF-TEST-003",
        "JustificatifUrl": None,
        "Commentaire": "Paiement test valide 3",
        "IdEleve": $ID_ELEVE_1,
        "IdFrais": $ID_FRAIS_1,
        "IdUtilisateur": None,
        "Statut": True
    }
]

for row_idx, data in enumerate(test_data, start=2):
    for col_idx, header in enumerate(headers, start=1):
        value = data.get(header, None)
        ws.cell(row=row_idx, column=col_idx, value=value)

for col_idx, header in enumerate(headers, start=1):
    ws.column_dimensions[openpyxl.utils.get_column_letter(col_idx)].width = max(len(header), 15)

wb.save("test_paiements_valides.xlsx")
print("✅ Fichier Excel créé : test_paiements_valides.xlsx")
print(f"   - {len(test_data)} lignes de données valides")
EOF

python3 /tmp/create_excel_paiements.py



