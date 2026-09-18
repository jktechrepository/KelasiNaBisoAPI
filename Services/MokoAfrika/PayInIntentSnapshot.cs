using System.Text.Json;
using System.Text.Json.Serialization;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.MokoAfrika
{
    /// <summary>
    /// Snapshot métier stocké dans TransactionMoko.RawRequest (enveloppe intent + gateway).
    /// Permet de créer le Paiement uniquement à la confirmation.
    /// </summary>
    public class PayInIntentSnapshot
    {
        public int IdEleve { get; set; }
        public int IdFrais { get; set; }
        public int? IdUtilisateur { get; set; }
        /// <summary>Montant net en devise du frais.</summary>
        public decimal MontantNet { get; set; }
        public decimal MontantCollecte { get; set; }
        public decimal MontantGatewayNet { get; set; }
        /// <summary>Devise dans laquelle le frais a été enregistré (ancre métier).</summary>
        public string CodeDeviseFrais { get; set; } = "USD";
        public string CodeDevisePrincipale { get; set; } = "USD";
        public string CodeDevisePaiement { get; set; } = "CDF";
        public decimal TauxVersDevisePrincipale { get; set; } = 1m;
        public decimal MontantPayeDevisePrincipale { get; set; }
        public string ModePaiement { get; set; } = "Mobile Money";
        public string OperateurMobileMoney { get; set; } = string.Empty;
        public string? Commentaire { get; set; }
        public string TelephonePayeur { get; set; } = string.Empty;
    }

    public class PayInRawRequestEnvelope
    {
        [JsonPropertyName("intent")]
        public PayInIntentSnapshot Intent { get; set; } = new();

        [JsonPropertyName("gateway")]
        public Dictionary<string, object?> Gateway { get; set; } = new();
    }

    public static class PayInRawRequestHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(PayInIntentSnapshot intent, Dictionary<string, object?> gatewayPayload) =>
            JsonSerializer.Serialize(new PayInRawRequestEnvelope
            {
                Intent = intent,
                Gateway = gatewayPayload
            }, JsonOptions);

        public static PayInIntentSnapshot? TryReadIntent(string? rawRequest)
        {
            if (string.IsNullOrWhiteSpace(rawRequest))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(rawRequest);
                if (doc.RootElement.TryGetProperty("intent", out var intentEl))
                    return JsonSerializer.Deserialize<PayInIntentSnapshot>(intentEl.GetRawText(), JsonOptions);
            }
            catch (JsonException)
            {
                // Ancien format : RawRequest = payload gateway seul
            }

            return null;
        }

        public static Paiement CreateConfirmedPaiement(PayInIntentSnapshot intent, string reference)
        {
            var now = DateTime.Now;
            var deviseFrais = string.IsNullOrWhiteSpace(intent.CodeDeviseFrais)
                ? intent.CodeDevisePrincipale
                : intent.CodeDeviseFrais;
            var montantPayePrincipale = intent.MontantPayeDevisePrincipale != 0
                ? intent.MontantPayeDevisePrincipale
                : intent.MontantNet;

            return new Paiement
            {
                IdEleve = intent.IdEleve,
                IdFrais = intent.IdFrais,
                IdUtilisateur = intent.IdUtilisateur,
                Montant = (double)intent.MontantNet,
                MontantNet = intent.MontantNet,
                MontantCollecte = intent.MontantCollecte,
                Devise = deviseFrais,
                CodeDevisePaiement = intent.CodeDevisePaiement,
                CodeDevisePrincipale = intent.CodeDevisePrincipale,
                TauxVersDevisePrincipale = intent.TauxVersDevisePrincipale,
                MontantPayeDevisePrincipale = montantPayePrincipale,
                ModePaiement = intent.ModePaiement,
                OperateurMobileMoney = intent.OperateurMobileMoney,
                StatutPaiement = "Confirme",
                Statut = true,
                Commentaire = intent.Commentaire,
                DatePaiement = now,
                DateCreation = now,
                ReferencePaiemenet = reference,
                ReferenceTransaction = reference
            };
        }
    }
}
