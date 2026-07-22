using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class NoteService : INoteRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public NoteService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _context.Notes
                .Include(n => n.AnneeScolaire)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<Note> GetByIdAsync(int id)
        {
            return await _context.Notes
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .FirstOrDefaultAsync(n => n.IdNote == id);
        }

        public async Task<IEnumerable<Note>> GetByEleveAsync(int idEleve)
        {
            return await _context.Notes
                .Where(n => n.IdEleve == idEleve)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByEvaluationAsync(int idEvaluation)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.IdEvaluation == idEvaluation)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        // ⚠️ DEPRECATED : Utiliser GetByEvaluationAsync. Fonctionne via Evaluation.IdCours
        public async Task<IEnumerable<Note>> GetByCoursAsync(int idCours)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.IdCours == idCours)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByProfesseurAsync(int idProfesseur)
        {
            return await _context.Notes
                .Where(n => n.IdProfesseur == idProfesseur)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.Notes
                .Where(n => n.IdAnneeScolaire == idAnneeScolaire)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByPeriodeAsync(string periode)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.Periode == periode)
                .Where(n => n.Statut == true) // ✅ Filtrer uniquement les notes actives
                .ToListAsync();
        }

        // ⚠️ DEPRECATED : Utiliser GetByPeriodeAsync. Fonctionne via Evaluation.Periode
        public async Task<IEnumerable<Note>> GetBySessionAsync(string session)
        {
            return await GetByPeriodeAsync(session);
        }

        public async Task<Note> CreateAsync(Note note)
        {
            note.DateCreation = DateTime.Now;
            
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> UpdateAsync(Note note)
        {
            var existingNote = await _context.Notes.FindAsync(note.IdNote);
            if (existingNote == null)
                return null;

            _context.Entry(existingNote).CurrentValues.SetValues(note);
            await _context.SaveChangesAsync();
            return existingNote;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Notes.AnyAsync(n => n.IdNote == id);
        }

        // ✅ SOFT DELETE: Toggle le statut d'une note (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            note.Statut = note.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
