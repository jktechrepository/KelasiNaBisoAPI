namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Résultat de liste élèves scopé à une école et une année scolaire résolue.
    /// </summary>
    public class ElevesAnneeScopedResult<T>
    {
        public T Data { get; set; } = default!;
        public int IdEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
    }
}
