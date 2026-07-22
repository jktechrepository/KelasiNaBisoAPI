using System.Net;
using KelasiNaBiso.Models.DTOs.Vitrine;
using KelasiNaBisoAPI.Services.Repositories;

namespace KelasiNaBiso.Services
{
    public interface IVitrineContactService
    {
        Task<ContactVitrineResponseDto> EnvoyerMessageAsync(
            ContactVitrineRequestDto request,
            string? adresseIp = null,
            CancellationToken cancellationToken = default);
    }

    public class VitrineContactService : IVitrineContactService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VitrineContactService> _logger;

        public VitrineContactService(
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<VitrineContactService> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ContactVitrineResponseDto> EnvoyerMessageAsync(
            ContactVitrineRequestDto request,
            string? adresseIp = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                _logger.LogWarning("Contact vitrine ignoré (honeypot rempli) depuis {Ip}", adresseIp);
                return SuccessResponse();
            }

            var destinataire = _configuration["EmailSettings:ContactRecipientEmail"]
                ?? "contact@kelasinabiso.com";
            var destinataireNom = _configuration["EmailSettings:ContactRecipientName"]
                ?? "Kelasi Na Biso - Contact";

            var objetAffiche = string.IsNullOrWhiteSpace(request.Objet) ? "Demande de contact" : request.Objet.Trim();
            var sujet = $"[Site vitrine] {objetAffiche} — {request.NomComplet.Trim()}";

            var plain = BuildPlainText(request, objetAffiche, adresseIp);
            var html = BuildHtml(request, objetAffiche, adresseIp);

            var envoye = await _emailService.SendGenericEmailAsync(
                destinataire,
                destinataireNom,
                sujet,
                plain,
                html,
                replyToEmail: request.Email.Trim(),
                replyToName: request.NomComplet.Trim());

            if (!envoye)
            {
                _logger.LogError("Échec envoi email contact vitrine pour {Email}", request.Email);
                return new ContactVitrineResponseDto
                {
                    Success = false,
                    Message = "Impossible d'envoyer votre message pour le moment. Réessayez plus tard ou écrivez à contact@kelasinabiso.com."
                };
            }

            _logger.LogInformation(
                "Contact vitrine envoyé à {Destinataire} — expéditeur {Email}",
                destinataire,
                request.Email);

            return SuccessResponse();
        }

        private static ContactVitrineResponseDto SuccessResponse() => new()
        {
            Success = true,
            Message = "Votre message a bien été envoyé. Notre équipe vous répondra sous 24 à 48 h ouvrées."
        };

        private static string BuildPlainText(ContactVitrineRequestDto r, string objet, string? ip)
        {
            var lines = new List<string>
            {
                "Nouveau message depuis le formulaire de contact KelasiNaBiso",
                "",
                $"Nom : {r.NomComplet.Trim()}",
                $"Email : {r.Email.Trim()}",
                $"Objet : {objet}"
            };

            if (!string.IsNullOrWhiteSpace(r.Etablissement))
                lines.Add($"Établissement : {r.Etablissement.Trim()}");
            if (!string.IsNullOrWhiteSpace(r.Telephone))
                lines.Add($"Téléphone : {r.Telephone.Trim()}");
            if (!string.IsNullOrWhiteSpace(r.Effectif))
                lines.Add($"Effectif : {r.Effectif.Trim()}");
            if (!string.IsNullOrWhiteSpace(ip))
                lines.Add($"IP : {ip}");

            lines.Add("");
            lines.Add("Message :");
            lines.Add(r.Message.Trim());

            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildHtml(ContactVitrineRequestDto r, string objet, string? ip)
        {
            string E(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);

            var rows = new List<string>
            {
                Row("Nom complet", E(r.NomComplet)),
                Row("Email", $"<a href=\"mailto:{E(r.Email)}\">{E(r.Email)}</a>"),
                Row("Objet", E(objet))
            };

            if (!string.IsNullOrWhiteSpace(r.Etablissement))
                rows.Add(Row("Établissement", E(r.Etablissement)));
            if (!string.IsNullOrWhiteSpace(r.Telephone))
                rows.Add(Row("Téléphone", E(r.Telephone)));
            if (!string.IsNullOrWhiteSpace(r.Effectif))
                rows.Add(Row("Effectif", E(r.Effectif)));
            if (!string.IsNullOrWhiteSpace(ip))
                rows.Add(Row("Adresse IP", E(ip)));

            var bodyRows = string.Join("", rows);
            var messageHtml = E(r.Message).Replace("\r\n", "<br/>").Replace("\n", "<br/>");

            return $@"<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""utf-8""></head>
<body style=""font-family:Segoe UI,Arial,sans-serif;background:#f4f6f9;padding:24px;color:#1f2937"">
  <div style=""max-width:640px;margin:0 auto;background:#fff;border-radius:12px;padding:28px;border:1px solid #e5e7eb"">
    <h2 style=""margin:0 0 8px;color:#0b1f3a"">Nouveau contact — Site vitrine</h2>
    <p style=""margin:0 0 20px;color:#6b7280"">Formulaire kelasinabiso.com/contact</p>
    <table style=""width:100%;border-collapse:collapse;margin-bottom:20px"">{bodyRows}</table>
    <h3 style=""margin:0 0 8px;font-size:16px"">Message</h3>
    <div style=""background:#f9fafb;border:1px solid #e5e7eb;border-radius:8px;padding:16px;line-height:1.6"">{messageHtml}</div>
    <p style=""margin-top:24px;font-size:12px;color:#9ca3af"">Répondre à cet email contacte directement l'expéditeur (Reply-To).</p>
  </div>
</body>
</html>";
        }

        private static string Row(string label, string value) =>
            $@"<tr>
  <td style=""padding:8px 12px 8px 0;font-weight:600;vertical-align:top;width:140px;color:#374151"">{WebUtility.HtmlEncode(label)}</td>
  <td style=""padding:8px 0;vertical-align:top"">{value}</td>
</tr>";
    }
}
