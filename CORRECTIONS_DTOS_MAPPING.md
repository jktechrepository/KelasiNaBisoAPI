# 🔧 MAPPING DES CORRECTIONS DTOs

## Corrections nécessaires par DTO

### 1. UpdateAnneeScolaireDto
- ❌ `NomAnneeScolaire` → ✅ `LibelleAnneeScolaire`
- ❌ `DateDebut` (nullable) → ✅ `DateDebut` (required)
- ❌ `DateFin` (nullable) → ✅ `DateFin` (required)

### 2. UpdateNoteDto
- ❌ `ValeurNote` → ✅ `NoteObtenue`
- ❌ `Commentaire` → ✅ `Appreciation`
- ❌ `EstPubliee` → N'EXISTE PAS (supprimer)
- + Ajouter: `Session`, `DateEvaluation`, `IdProfesseur`, `IdEleve`, `IdCours`, `IdAnneeScolaire`

### 3. UpdateEvaluationDto
- ❌ `TitreEvaluation` → N'EXISTE PAS
- ❌ `Description` → N'EXISTE PAS
- ❌ `DateEvaluation` → N'EXISTE PAS
- ❌ `PondMaximale` → N'EXISTE PAS
- ✅ Garder: `TypeEvaluation`
- + Ajouter: `Coefficient`, `IdCours`, `IdClasse`

### 4. UpdatePaiementDto
- ❌ `MontantPaye` → ✅ `Montant`
- + Tous les autres champs existent dans Paiement

### 5. UpdatePresenceDto
- ❌ `StatutPresence` → N'EXISTE PAS (vérifier modèle)
- ❌ `Commentaire` → ❓
- ❌ `JustificatifUrl` → ❓

### 6. UpdateAgentDto (déjà réparé partiellement)
- ❌ `Email` → ✅ `EmailAgent`
- ❌ `Telephone` → ✅ `TelephoneAgent`
- ❌ `Photo` → ✅ `PhotoUrl`
- ❌ `LieuNaissance` → ❓ (n'existe pas dans Agent.cs)
- ❌ `Grade`, `DateEmbauche` → ❓ (n'existent pas)

### 7. UpdateUserDeviceDto
- ❌ `Platform`, `AppVersion`, `EstActif` → À vérifier

### 8. UpdateClasseDto
- ❌ `CapaciteMaximale`, `Description` → À vérifier

### 9. UpdateDirectionDto, UpdateDocumentDto, etc.
- À vérifier modèle par modèle

### 10. UpdateEcoleDto
- Types de données incompatibles (double? → string)

