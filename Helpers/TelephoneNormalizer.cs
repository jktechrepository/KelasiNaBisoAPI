namespace KelasiNaBiso.Helpers
{
    /// <summary>
    /// Normalise les numéros de téléphone pour le stockage et la recherche
    /// (supprime espaces et tirets ; format cible ex. +243XXXXXXXXX).
    /// </summary>
    public static class TelephoneNormalizer
    {
        public static string? Normalize(string? telephone)
        {
            if (string.IsNullOrWhiteSpace(telephone))
                return telephone;

            return telephone.Trim().Replace(" ", "").Replace("-", "");
        }
    }
}
