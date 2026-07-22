using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(int id);
        //Task<Document> GetByNomAsync(string nom);
        Task<IEnumerable<Document>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<Document>> GetByUtilisateurAsync(int idUtilisateur);
        Task<IEnumerable<Document>> GetByTypeAsync(string type);
        Task<IEnumerable<Document>> GetByDateCreationAsync(DateTime date);
        Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<Document> CreateAsync(Document document);
        Task<Document> UpdateAsync(Document document);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
       // Task<bool> ExistsByNomAsync(string nom);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
