using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public interface IMokoFeeCalculator
    {
        MokoFeeEstimateDto Estimate(decimal montantNet, string method, string currency = "CDF");
    }

    public class MokoFeeCalculator : IMokoFeeCalculator
    {
        private static readonly Dictionary<string, (decimal CollectePct, decimal DecaissementPct, decimal TvaPct)> Grille = new(StringComparer.OrdinalIgnoreCase)
        {
            [MomoOperators.Orange] = (3.00m, 2.00m, 0m),
            [MomoOperators.Mpesa] = (2.50m, 2.00m, 0m),
            [MomoOperators.Airtel] = (3.00m, 2.50m, 0m),
            [MomoOperators.Africell] = (3.00m, 2.50m, 0m),
            [MomoOperators.Card] = (3.50m, 3.00m, 16m),
        };

        public MokoFeeEstimateDto Estimate(decimal montantNet, string method, string currency = "CDF")
        {
            if (!Grille.TryGetValue(method, out var taux))
            {
                throw new ArgumentException($"Méthode de paiement MOKO non supportée : {method}. Valeurs : {string.Join(", ", MomoOperators.All)}");
            }

            var fraisCollecteBase = Round(montantNet * (taux.CollectePct / 100m));
            var fraisTva = taux.TvaPct > 0
                ? Round(fraisCollecteBase * (taux.TvaPct / 100m))
                : 0m;
            var fraisCollecte = fraisCollecteBase + fraisTva;
            var fraisDecaissement = Round(montantNet * (taux.DecaissementPct / 100m));
            var montantCollecte = Round(montantNet + fraisCollecte + fraisDecaissement);

            return new MokoFeeEstimateDto
            {
                MontantNet = montantNet,
                FraisCollecte = fraisCollecte,
                FraisDecaissement = fraisDecaissement,
                MontantCollecte = montantCollecte,
                Devise = currency,
                Method = method.ToLowerInvariant()
            };
        }

        private static decimal Round(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
