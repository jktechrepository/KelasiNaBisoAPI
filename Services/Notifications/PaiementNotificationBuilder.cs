using System;
using System.Collections.Generic;
using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Notifications
{
    public interface IPaiementNotificationBuilder
    {
        NotificationMessage Build(NotificationContext context);
    }

    public class PaiementNotificationBuilder : IPaiementNotificationBuilder
    {
        public NotificationMessage Build(NotificationContext context)
        {
            if (context.Paiement == null || context.Eleve == null || context.Tuteur == null)
            {
                throw new InvalidOperationException("Le contexte de notification paiement est incomplet.");
            }

            var paiement = context.Paiement;
            var eleve = context.Eleve;
            var tuteur = context.Tuteur;
            var ecole = context.Ecole;
            var classe = eleve.Inscriptions
                .Where(i => i.Statut == true &&
                    (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                .OrderByDescending(i => i.DateInscription)
                .Select(i => i.Classe)
                .FirstOrDefault();

            var montantFormate = $"{paiement.Montant:N2} {paiement.Devise ?? "USD"}";
            var dateFormatee = paiement.DatePaiement.ToString("dd/MM/yyyy HH:mm");
            var modePaiement = paiement.ModePaiement ?? "Non spécifié";
            var typeFrais = GetTypeFrais(paiement);
            var referencePaiement = !string.IsNullOrWhiteSpace(paiement.ReferencePaiemenet)
                ? paiement.ReferencePaiemenet
                : $"PAY-{paiement.IdPaiement}";

            // Informations supplémentaires
            var nomEcole = ecole?.Nom ?? "École";
            var nomClasse = classe?.NomClasse ?? "Classe";
            var section = classe?.Section?.NomSection;

            var iconeStatut = paiement.StatutPaiement switch
            {
                "Confirme" => "✅",
                "En attente" => "⏳",
                "Echoue" => "❌",
                _ => "💰"
            };

            var messageStatut = paiement.StatutPaiement switch
            {
                "Confirme" => "Paiement confirmé avec succès",
                "En attente" => "Paiement en cours de validation",
                "Echoue" => "Paiement échoué - Veuillez réessayer",
                _ => "Paiement enregistré"
            };

            var typeNotification = "PAIEMENT_ELEVE";
            
            // Titre simple et clair selon la proposition
            var titrePush = "Confirmation Paiement Frais";
            
            // Corps simple et informatif selon la proposition
            var dateCourte = paiement.DatePaiement.ToString("dd/MM/yyyy");
            var corpsPush = $"Paiement effectué pour l'élève {eleve.NomComplet} en Date du {dateCourte}. Montant: {montantFormate}. Type: {typeFrais}.";

            if (!string.IsNullOrWhiteSpace(paiement.Commentaire))
            {
                corpsPush += $" Note: {paiement.Commentaire}";
            }

            PushNotificationMessage? pushMessage = null;
            if (context.AllowPush)
            {
                pushMessage = new PushNotificationMessage
                {
                    IsEnabled = true,
                    Title = titrePush,
                    Body = corpsPush,
                    Type = typeNotification,
                    Data = BuildData(paiement, eleve, modePaiement, nomEcole, nomClasse, section, referencePaiement)
                };
            }

            var smsBody = BuildSmsBody(paiement, eleve, ecole, modePaiement);

            var smsMessage = new SmsNotificationMessage
            {
                IsEnabled = context.AllowSms,
                Body = smsBody
            };

            InAppNotificationMessage? inAppMessage = null;
            if (context.AllowInApp)
            {
                // Titre in-app simple
                var titreInApp = "Confirmation Paiement Frais";
                
                inAppMessage = new InAppNotificationMessage
                {
                    IsEnabled = true,
                    Title = titreInApp,
                    Content = corpsPush,
                    Type = typeNotification,
                    Icon = iconeStatut,
                    Metadata = BuildMetadata(paiement, eleve, modePaiement, nomEcole, nomClasse, section, referencePaiement)
                };
            }

            return new NotificationMessage
            {
                Push = pushMessage,
                Sms = smsMessage,
                InApp = inAppMessage
            };
        }

        private static string GetTypeFrais(Paiement paiement)
        {
            if (paiement.Frais != null && !string.IsNullOrWhiteSpace(paiement.Frais.TypeFrais))
            {
                return paiement.Frais.TypeFrais;
            }

            return "Frais";
        }

        private static Dictionary<string, string> BuildData(Paiement paiement, Eleve eleve, string modePaiement, string nomEcole, string nomClasse, string? section, string referencePaiement)
        {
            var data = new Dictionary<string, string>
            {
                { "type", "PAIEMENT_ELEVE" },
                { "idPaiement", paiement.IdPaiement.ToString() },
                { "idEleve", eleve.IdEleve.ToString() },
                { "nomEleve", eleve.NomComplet ?? "N/A" },
                { "montant", paiement.Montant.ToString("F2") },
                { "devise", paiement.Devise ?? "USD" },
                { "modePaiement", modePaiement },
                { "statutPaiement", paiement.StatutPaiement ?? "N/A" },
                { "datePaiement", paiement.DatePaiement.ToString("yyyy-MM-dd HH:mm:ss") },
                { "referencePaiement", referencePaiement },
                { "nomEcole", nomEcole },
                { "nomClasse", nomClasse },
                { "section", section ?? "N/A" }
            };

            if (paiement.IdFrais.HasValue)
            {
                data.Add("idFrais", paiement.IdFrais.Value.ToString());
                data.Add("typeFrais", paiement.Frais?.TypeFrais ?? "Frais");
            }

            return data;
        }

        private static Dictionary<string, string> BuildMetadata(Paiement paiement, Eleve eleve, string modePaiement, string nomEcole, string nomClasse, string? section, string referencePaiement)
        {
            var metadata = new Dictionary<string, string>
            {
                { "idPaiement", paiement.IdPaiement.ToString() },
                { "idEleve", eleve.IdEleve.ToString() },
                { "modePaiement", modePaiement },
                { "statutPaiement", paiement.StatutPaiement ?? "N/A" },
                { "montant", paiement.Montant.ToString("F2") },
                { "devise", paiement.Devise ?? "USD" },
                { "datePaiement", paiement.DatePaiement.ToString("yyyy-MM-dd HH:mm:ss") },
                { "referencePaiement", referencePaiement },
                { "nomEcole", nomEcole },
                { "nomClasse", nomClasse },
                { "section", section ?? "N/A" }
            };

            if (paiement.IdFrais.HasValue)
            {
                metadata.Add("idFrais", paiement.IdFrais.Value.ToString());
            }

            return metadata;
        }

        private static string BuildSmsBody(Paiement paiement, Eleve eleve, Ecole? ecole, string modePaiement)
        {
            var montantFormate = $"{paiement.Montant:N2} {paiement.Devise ?? "USD"}";
            var dateCourte = paiement.DatePaiement.ToString("dd/MM/yyyy");
            var typeFrais = paiement.Frais?.TypeFrais ?? "Frais";

            // Format SMS simple (sans emojis ni accents)
            var smsBody = $"Confirmation Paiement Frais\nPaiement effectue pour l'eleve {eleve.NomComplet} en Date du {dateCourte}. Montant: {montantFormate}. Type: {typeFrais}.";

            if (paiement.StatutPaiement == "Echoue")
            {
                smsBody = $"Paiement echoue pour l'eleve {eleve.NomComplet} en Date du {dateCourte}. Montant: {montantFormate}. Veuillez reessayer.";
            }
            else if (paiement.StatutPaiement == "En attente")
            {
                smsBody = $"Paiement en attente pour l'eleve {eleve.NomComplet} en Date du {dateCourte}. Montant: {montantFormate}. Type: {typeFrais}.";
            }

            return smsBody;
        }
    }
}

