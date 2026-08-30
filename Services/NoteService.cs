using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class NoteService : INoteRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public NoteService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _context.Notes
                .Include(n => n.AnneeScolaire)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<Note> GetByIdAsync(int id)
        {
            return await _context.Notes
                .Where(n => n.Statut == true)
                .FirstOrDefaultAsync(n => n.IdNote == id);
        }

        public async Task<IEnumerable<Note>> GetByEleveAsync(int idEleve)
        {
            return await _context.Notes
                .Where(n => n.IdEleve == idEleve)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByEvaluationAsync(int idEvaluation)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.IdEvaluation == idEvaluation)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByCoursAsync(int idCours)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.IdCours == idCours)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByProfesseurAsync(int idProfesseur)
        {
            return await _context.Notes
                .Where(n => n.IdProfesseur == idProfesseur)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.Notes
                .Where(n => n.IdAnneeScolaire == idAnneeScolaire)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByPeriodeAsync(string periode)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.Periode == periode)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetBySessionAsync(string session)
        {
            return await GetByPeriodeAsync(session);
        }

        public async Task<Note> CreateAsync(Note note)
        {
            await ValidateAndNormalizeForCreateAsync(note);
            note.DateCreation = DateTime.Now;
            note.Statut ??= true;

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> UpdateAsync(Note note)
        {
            var existingNote = await _context.Notes.FindAsync(note.IdNote);
            if (existingNote == null)
                return null;

            if (note.NoteObtenue < 0 || note.NoteObtenue > 100)
                throw new InvalidOperationException("NoteObtenue doit être entre 0 et 100.");

            existingNote.NoteObtenue = note.NoteObtenue;
            existingNote.Appreciation = note.Appreciation;
            await _context.SaveChangesAsync();
            return existingNote;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            // Soft delete (aligné avec ToggleStatut)
            note.Statut = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Notes.AnyAsync(n => n.IdNote == id && n.Statut == true);
        }

        public async Task<bool> ToggleStatutAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            note.Statut = note.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidateAndNormalizeForCreateAsync(Note note)
        {
            if (note.NoteObtenue < 0 || note.NoteObtenue > 100)
                throw new InvalidOperationException("NoteObtenue doit être entre 0 et 100.");

            var evaluation = await _context.Evaluations.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEvaluation == note.IdEvaluation && e.Statut == true)
                ?? throw new InvalidOperationException($"Évaluation {note.IdEvaluation} introuvable ou inactive.");

            var eleve = await _context.Eleves.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEleve == note.IdEleve && e.Statut == true)
                ?? throw new InvalidOperationException($"Élève {note.IdEleve} introuvable ou inactif.");

            var professeurExists = await _context.Utilisateurs.AsNoTracking()
                .AnyAsync(u => u.IdUtilisateur == note.IdProfesseur && u.Statut == true);
            if (!professeurExists)
                throw new InvalidOperationException($"Professeur {note.IdProfesseur} introuvable ou inactif.");

            if (note.IdAnneeScolaire <= 0)
            {
                var idEcole = await _context.Inscriptions.AsNoTracking()
                    .Where(i => i.IdEleve == note.IdEleve && i.Statut == true)
                    .OrderByDescending(i => i.DateInscription)
                    .Select(i => (int?)i.IdEcole)
                    .FirstOrDefaultAsync();

                if (!idEcole.HasValue || idEcole.Value <= 0)
                {
                    idEcole = await _context.Classes.AsNoTracking()
                        .Where(c => c.IdClasse == evaluation.IdClasse)
                        .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                        .FirstOrDefaultAsync();
                }

                if (!idEcole.HasValue || idEcole.Value <= 0)
                    throw new InvalidOperationException(
                        "Impossible de résoudre l'année scolaire : précisez IdAnneeScolaire.");

                note.IdAnneeScolaire = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, null);
            }
            else
            {
                var anneeOk = await _context.AnneeScolaires.AsNoTracking()
                    .AnyAsync(a => a.IdAnneeScolaire == note.IdAnneeScolaire && a.Statut == true);
                if (!anneeOk)
                    throw new InvalidOperationException($"Année scolaire {note.IdAnneeScolaire} introuvable.");
            }

            var duplicate = await _context.Notes.AsNoTracking()
                .AnyAsync(n => n.IdEleve == note.IdEleve
                    && n.IdEvaluation == note.IdEvaluation
                    && n.Statut == true);
            if (duplicate)
            {
                throw new InvalidOperationException(
                    $"Une note active existe déjà pour l'élève {note.IdEleve} et l'évaluation {note.IdEvaluation}.");
            }

            if (note.DateEvaluation == default)
                note.DateEvaluation = DateTime.Now;

            note.Appreciation ??= string.Empty;
        }
    }
}
