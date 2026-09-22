namespace KelasiNaBiso.Models.Enums
{
    /// <summary>Type de règle d'exonération / allègement de frais.</summary>
    public static class TypeRegleExonerationFrais
    {
        public const string Totale = "Totale";
        public const string Pourcentage = "Pourcentage";
        public const string MontantReduction = "MontantReduction";
        public const string MontantDuFixe = "MontantDuFixe";

        public static bool IsKnown(string? type) =>
            type == Totale || type == Pourcentage || type == MontantReduction || type == MontantDuFixe;
    }
}
