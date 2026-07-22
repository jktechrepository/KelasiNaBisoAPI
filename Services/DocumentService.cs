using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class DocumentService : IDocumentRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public DocumentService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents
               // .Include(d => d.Eleve)
              //  .Include(d => d.Utilisateur)
                .Where(d => d.Statut == true) // ✅ Filtrer uniquement les documents actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents
              //  .Include(d => d.Eleve)
              //  .Include(d => d.Utilisateur)
                .FirstOrDefaultAsync(d => d.IdDocument == id);
        }

        //public async Task<Document> GetByNomAsync(string nom)
        //{
        //    return await _context.Documents
        //        .Include(d => d.Eleve)
        //        .Include(d => d.Utilisateur)
        //        .FirstOrDefaultAsync(d => d.Nom == nom);
        //}

        public async Task<IEnumerable<Document>> GetByEleveAsync(int idEleve)
        {
            return await _context.Documents
              //  .Include(d => d.Utilisateur)
                .Where(d => d.IdEleve == idEleve && d.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByUtilisateurAsync(int idUtilisateur)
        {
            return await _context.Documents
              //  .Include(d => d.Eleve)
                .Where(d => d.IdUtilisateur == idUtilisateur && d.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByTypeAsync(string type)
        {
            return await _context.Documents
               // .Include(d => d.Eleve)
              //  .Include(d => d.Utilisateur)
                .Where(d => d.TypeDocument == type && d.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDateCreationAsync(DateTime date)
        {
            return await _context.Documents
               // .Include(d => d.Eleve)
              //  .Include(d => d.Utilisateur)
                .Where(d => d.DateCreation.Date == date.Date && d.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.Documents
              //  .Include(d => d.Eleve)
              //  .Include(d => d.Utilisateur)
                .Where(d => d.DateCreation >= dateDebut && d.DateCreation <= dateFin && d.Statut == true) // ✅ Filtrer actifs
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        public async Task<Document> CreateAsync(Document document)
        {
            document.DateCreation = DateTime.Now;
            
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<Document> UpdateAsync(Document document)
        {
            var existingDocument = await _context.Documents.FindAsync(document.IdDocument);
            if (existingDocument == null)
                return null;

            _context.Entry(existingDocument).CurrentValues.SetValues(document);
            await _context.SaveChangesAsync();
            return existingDocument;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Documents.AnyAsync(d => d.IdDocument == id);
        }

        //public async Task<bool> ExistsByNomAsync(string nom)
        //{
        //    return await _context.Documents.AnyAsync(d => d.Nom == nom);
        //}

        // ✅ SOFT DELETE: Toggle le statut d'un document (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return false;

            document.Statut = document.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
