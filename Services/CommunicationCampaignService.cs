using System.Text.Json;
using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Communication;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CommunicationCampaignService : ICommunicationCampaignService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<CommunicationCampaignService> _logger;
        private readonly ICommunicationDispatchScheduler _dispatchScheduler;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public CommunicationCampaignService(
            KelasiNaBisoDbContext context,
            ILogger<CommunicationCampaignService> logger,
            ICommunicationDispatchScheduler dispatchScheduler)
        {
            _context = context;
            _logger = logger;
            _dispatchScheduler = dispatchScheduler;
        }

        #region Public API

        public async Task<PagedResult<CommunicationCampaignSummaryDto>> GetCampaignsAsync(int currentUserId, string currentUserRole, int? currentUserEcoleId, PagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = BuildCampaignQueryForUser(currentUserRole, currentUserEcoleId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Titre.ToLower().Contains(term) || c.Importance.ToLower().Contains(term));
            }

            query = request.SortBy switch
            {
                "Titre" => request.SortDescending ? query.OrderByDescending(c => c.Titre) : query.OrderBy(c => c.Titre),
                "Importance" => request.SortDescending ? query.OrderByDescending(c => c.Importance) : query.OrderBy(c => c.Importance),
                "Statut" => request.SortDescending ? query.OrderByDescending(c => c.Statut) : query.OrderBy(c => c.Statut),
                _ => request.SortDescending ? query.OrderByDescending(c => c.DateCreation) : query.OrderBy(c => c.DateCreation)
            };

            var total = await query.CountAsync(cancellationToken);

            var campaigns = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CommunicationCampaignSummaryProjection
                {
                    IdCampaign = c.IdCampaign,
                    IdEcole = c.IdEcole,
                    NomEcole = c.Ecole != null ? c.Ecole.Nom ?? string.Empty : string.Empty,
                    Titre = c.Titre,
                    Importance = c.Importance,
                    Statut = c.Statut,
                    DateCreation = c.DateCreation,
                    PlanifiedAt = c.PlanifiedAt,
                    ExpirationAt = c.ExpirationAt,
                    RappelAuto = c.RappelAuto,
                    Auteur = c.Auteur != null ? $"{c.Auteur.PrenomUtilisateur} {c.Auteur.NomUtilisateur}".Trim() : string.Empty,
                    Validateur = c.Validateur != null ? $"{c.Validateur.PrenomUtilisateur} {c.Validateur.NomUtilisateur}".Trim() : null
                })
                .ToListAsync(cancellationToken);

            var campaignIds = campaigns.Select(c => c.IdCampaign).ToList();

            var stats = await _context.CampaignRecipients
                .Where(r => campaignIds.Contains(r.IdCampaign))
                .GroupBy(r => r.IdCampaign)
                .Select(g => new
                {
                    IdCampaign = g.Key,
                    Total = g.Count(),
                    Envoyes = g.Count(x => x.Status == "Envoye"),
                    Echecs = g.Count(x => x.Status == "Echec")
                })
                .ToListAsync(cancellationToken);

            var statLookup = stats.ToDictionary(x => x.IdCampaign, x => x);

            var result = new List<CommunicationCampaignSummaryDto>(campaigns.Count);

            foreach (var item in campaigns)
            {
                statLookup.TryGetValue(item.IdCampaign, out var stat);

                result.Add(new CommunicationCampaignSummaryDto
                {
                    IdCampaign = item.IdCampaign,
                    IdEcole = item.IdEcole,
                    NomEcole = item.NomEcole,
                    Titre = item.Titre,
                    Importance = item.Importance,
                    Statut = item.Statut,
                    DateCreation = item.DateCreation,
                    PlanifiedAt = item.PlanifiedAt,
                    ExpirationAt = item.ExpirationAt,
                    RappelAuto = item.RappelAuto,
                    Auteur = item.Auteur,
                    Validateur = item.Validateur,
                    TotalDestinataires = stat?.Total ?? 0,
                    Envoyes = stat?.Envoyes ?? 0,
                    Echecs = stat?.Echecs ?? 0
                });
            }

            return new PagedResult<CommunicationCampaignSummaryDto>(result, total, request.PageNumber, request.PageSize);
        }

        public async Task<CommunicationCampaignDetailDto?> GetCampaignByIdAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var campaign = await BuildCampaignQueryForUser(currentUserRole, currentUserEcoleId)
                .Include(c => c.Segments)
                .Include(c => c.Destinataires)
                .Include(c => c.Historique)
                .Include(c => c.Auteur)
                .Include(c => c.Validateur)
                .Include(c => c.Ecole)
                .FirstOrDefaultAsync(c => c.IdCampaign == idCampaign, cancellationToken);

            if (campaign == null)
            {
                return null;
            }

            var channels = CommunicationChannelHelper.DeserializeChannels(campaign.ChannelsJson);

            var detail = new CommunicationCampaignDetailDto
            {
                IdCampaign = campaign.IdCampaign,
                IdEcole = campaign.IdEcole,
                NomEcole = campaign.Ecole?.Nom ?? string.Empty,
                Titre = campaign.Titre,
                ContenuMarkdown = campaign.ContenuMarkdown,
                Importance = campaign.Importance,
                Statut = campaign.Statut,
                Canaux = channels,
                RappelAuto = campaign.RappelAuto,
                DateCreation = campaign.DateCreation,
                PlanifiedAt = campaign.PlanifiedAt,
                ExpirationAt = campaign.ExpirationAt,
                ValidationAt = campaign.ValidationAt,
                IdAuteur = campaign.IdAuteur,
                NomAuteur = BuildNomComplet(campaign.Auteur),
                ValidatedBy = campaign.ValidatedBy,
                NomValidateur = BuildNomComplet(campaign.Validateur),
                TotalDestinataires = campaign.Destinataires?.Count ?? 0,
                Envoyes = campaign.Destinataires?.Count(r => r.Status == "Envoye") ?? 0,
                Echecs = campaign.Destinataires?.Count(r => r.Status == "Echec") ?? 0
            };

            if (campaign.Segments != null && campaign.Segments.Count > 0)
            {
                var createurIds = campaign.Segments.Select(s => s.CreatedBy).Distinct().ToList();
                var createurs = await _context.Utilisateurs
                    .Where(u => createurIds.Contains(u.IdUtilisateur))
                    .Select(u => new { u.IdUtilisateur, Nom = BuildNomComplet(u) })
                    .ToDictionaryAsync(u => u.IdUtilisateur, u => u.Nom, cancellationToken);

                foreach (var segment in campaign.Segments)
                {
                    var criteria = DeserializeCriteria(segment.CriteriaJson);
                    detail.Segments.Add(new CommunicationSegmentDto
                    {
                        IdSegment = segment.IdSegment,
                        NomSegment = segment.NomSegment,
                        TypeSegment = segment.TypeSegment,
                        IsReusable = segment.IsReusable,
                        Criteria = criteria,
                        DateCreation = segment.DateCreation,
                        CreatedBy = segment.CreatedBy,
                        CreatedByNom = createurs.TryGetValue(segment.CreatedBy, out var nom) ? nom : null
                    });
                }
            }

            return detail;
        }

        public async Task<PagedResult<CommunicationCampaignDetailDto>> GetCampaignsByEcoleAsync(int ecoleId, int currentUserId, string currentUserRole, int? currentUserEcoleId, PagedRequest request, CancellationToken cancellationToken = default)
        {
            if (ecoleId <= 0)
            {
                throw new ArgumentException("Identifiant d'école invalide.", nameof(ecoleId));
            }

            request ??= new PagedRequest();

            if (currentUserRole != UserRoles.SUPER_ADMIN)
            {
                var userEcoleId = currentUserEcoleId ?? 0;
                if (userEcoleId <= 0)
                {
                    throw new UnauthorizedAccessException("Impossible de déterminer l'école de l'utilisateur.");
                }

                if (userEcoleId != ecoleId)
                {
                    throw new UnauthorizedAccessException("Vous n'avez pas accès à cette école.");
                }
            }
            else
            {
                var exists = await _context.Ecoles
                    .AsNoTracking()
                    .AnyAsync(e => e.IdEcole == ecoleId, cancellationToken);

                if (!exists)
                {
                    throw new KeyNotFoundException("École introuvable.");
                }
            }

            var query = BuildCampaignQueryForUser(currentUserRole, currentUserEcoleId)
                .Where(c => c.IdEcole == ecoleId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Titre.ToLower().Contains(term) || c.Importance.ToLower().Contains(term));
            }

            query = request.SortBy switch
            {
                "Titre" => request.SortDescending ? query.OrderByDescending(c => c.Titre) : query.OrderBy(c => c.Titre),
                "Importance" => request.SortDescending ? query.OrderByDescending(c => c.Importance) : query.OrderBy(c => c.Importance),
                "Statut" => request.SortDescending ? query.OrderByDescending(c => c.Statut) : query.OrderBy(c => c.Statut),
                _ => request.SortDescending ? query.OrderByDescending(c => c.DateCreation) : query.OrderBy(c => c.DateCreation)
            };

            var total = await query.CountAsync(cancellationToken);
            var campaignIds = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => c.IdCampaign)
                .ToListAsync(cancellationToken);

            var results = new List<CommunicationCampaignDetailDto>(campaignIds.Count);
            foreach (var campaignId in campaignIds)
            {
                var detail = await GetCampaignByIdAsync(campaignId, currentUserId, currentUserRole, currentUserEcoleId, cancellationToken);
                if (detail != null)
                {
                    results.Add(detail);
                }
            }

            return new PagedResult<CommunicationCampaignDetailDto>(results, total, request.PageNumber, request.PageSize);
        }

        public async Task<CommunicationCampaignDetailDto> CreateCampaignAsync(CreateCommunicationCampaignDto dto, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            ValidateChannels(dto.Canaux);

            int ecoleId;
            if (currentUserRole == UserRoles.SUPER_ADMIN)
            {
                ecoleId = dto.IdEcole ?? throw new ArgumentException("IdEcole est obligatoire pour un Super-Admin.");
            }
            else
            {
                ecoleId = currentUserEcoleId ?? 0;
                if (ecoleId <= 0)
                {
                    throw new InvalidOperationException("Impossible de déterminer l'école de l'utilisateur.");
                }
            }

            var ecoleExiste = await _context.Ecoles.AnyAsync(e => e.IdEcole == ecoleId, cancellationToken);
            if (!ecoleExiste)
            {
                throw new KeyNotFoundException($"École {ecoleId} introuvable.");
            }

            var campaign = new CommunicationCampaign
            {
                IdEcole = ecoleId,
                IdAuteur = currentUserId,
                Titre = dto.Titre.Trim(),
                ContenuMarkdown = dto.ContenuMarkdown,
                Importance = dto.Importance,
                ChannelsJson = CommunicationChannelHelper.SerializeChannels(dto.Canaux),
                RappelAuto = dto.RappelAuto,
                DateCreation = DateTime.UtcNow,
                PlanifiedAt = dto.PlanifiedAt,
                ExpirationAt = dto.ExpirationAt,
                Statut = CampaignStatuses.Brouillon
            };

            await _context.CommunicationCampaigns.AddAsync(campaign, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await UpsertSegmentsAsync(campaign, dto.Segments, currentUserId, cancellationToken);
            await AddHistoryAsync(campaign.IdCampaign, currentUserId, "Création", null, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            if (dto.SendImmediately)
            {
                try
                {
                    await DispatchCampaignAsync(campaign.IdCampaign, currentUserId, currentUserRole, currentUserEcoleId, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    _logger.LogWarning("Échec de l'envoi immédiat pour la campagne {IdCampaign}", campaign.IdCampaign);
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogWarning("Envoi immédiat refusé pour la campagne {IdCampaign}", campaign.IdCampaign);
                    throw;
                }
            }

            _context.ChangeTracker.Clear();

            return await GetCampaignByIdAsync(campaign.IdCampaign, currentUserId, currentUserRole, currentUserEcoleId, cancellationToken)
                ?? throw new InvalidOperationException("Impossible de récupérer la campagne après création.");
        }

        public async Task<CommunicationCampaignDetailDto> UpdateCampaignAsync(int idCampaign, UpdateCommunicationCampaignDto dto, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var campaign = await FindCampaignForUpdateAsync(idCampaign, currentUserRole, currentUserEcoleId, cancellationToken);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campagne introuvable ou accès refusé.");
            }

            if (campaign.Statut is CampaignStatuses.EnCours or CampaignStatuses.Envoye or CampaignStatuses.Partiel)
            {
                throw new InvalidOperationException("La campagne ne peut plus être modifiée à ce stade.");
            }

            ValidateChannels(dto.Canaux);

            campaign.Titre = dto.Titre.Trim();
            campaign.ContenuMarkdown = dto.ContenuMarkdown;
            campaign.Importance = dto.Importance;
            campaign.ChannelsJson = CommunicationChannelHelper.SerializeChannels(dto.Canaux);
            campaign.RappelAuto = dto.RappelAuto;
            campaign.PlanifiedAt = dto.PlanifiedAt;
            campaign.ExpirationAt = dto.ExpirationAt;

            if (dto.Segments != null)
            {
                await UpsertSegmentsAsync(campaign, dto.Segments, currentUserId, cancellationToken);
            }

            await AddHistoryAsync(idCampaign, currentUserId, "MiseAJour", null, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.ChangeTracker.Clear();

            return await GetCampaignByIdAsync(idCampaign, currentUserId, currentUserRole, currentUserEcoleId, cancellationToken)
                ?? throw new InvalidOperationException("Impossible de récupérer la campagne après mise à jour.");
        }

        public async Task<bool> CancelCampaignAsync(int idCampaign, CancelCommunicationRequest? request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var campaign = await FindCampaignForUpdateAsync(idCampaign, currentUserRole, currentUserEcoleId, cancellationToken);
            if (campaign == null)
            {
                return false;
            }

            if (campaign.Statut == CampaignStatuses.Envoye || campaign.Statut == CampaignStatuses.Partiel)
            {
                throw new InvalidOperationException("Impossible d'annuler une campagne déjà envoyée.");
            }

            campaign.Statut = CampaignStatuses.Annule;
            campaign.PlanifiedAt = null;

            var detail = request?.Message;
            await AddHistoryAsync(idCampaign, currentUserId, "Annulation", detail, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> RefreshRecipientsAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var campaign = await FindCampaignForUpdateAsync(idCampaign, currentUserRole, currentUserEcoleId, cancellationToken);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campagne introuvable ou accès refusé.");
            }

            var total = await RebuildRecipientsAsync(campaign, cancellationToken);

            if (total == 0)
            {
                _logger.LogWarning("Aucun destinataire trouvé pour la campagne {IdCampaign}", idCampaign);
            }

            await AddHistoryAsync(idCampaign, currentUserId, "DestinatairesGeneres", $"Total={total}", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return total;
        }

        public async Task<PagedResult<CommunicationRecipientDto>> GetRecipientsAsync(int idCampaign, PagedRequest request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var accessible = await BuildCampaignQueryForUser(currentUserRole, currentUserEcoleId)
                .AnyAsync(c => c.IdCampaign == idCampaign, cancellationToken);

            if (!accessible)
            {
                throw new KeyNotFoundException("Campagne introuvable ou accès refusé.");
            }

            IQueryable<CampaignRecipient> query = _context.CampaignRecipients
                .AsNoTracking()
                .Where(r => r.IdCampaign == idCampaign)
                .Include(r => r.Utilisateur)
                .ThenInclude(u => u.Tuteur)
                .Include(r => r.Eleve)
                .ThenInclude(e => e.Classe);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(r =>
                    (r.Utilisateur.NomUtilisateur ?? "").ToLower().Contains(term) ||
                    (r.Utilisateur.PrenomUtilisateur ?? "").ToLower().Contains(term) ||
                    (r.Utilisateur.Email ?? "").ToLower().Contains(term) ||
                    (r.Utilisateur.Telephone ?? "").ToLower().Contains(term));
            }

            var orderedQuery = request.SortBy switch
            {
                "Status" => request.SortDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "DateEnvoi" => request.SortDescending ? query.OrderByDescending(r => r.DateEnvoi) : query.OrderBy(r => r.DateEnvoi),
                _ => request.SortDescending ? query.OrderByDescending(r => r.DatePlanifie) : query.OrderBy(r => r.DatePlanifie)
            };

            var total = await orderedQuery.CountAsync(cancellationToken);

            var data = await orderedQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new CommunicationRecipientDto
                {
                    IdRecipient = r.IdRecipient,
                    IdUtilisateur = r.IdUtilisateur,
                    IdTuteur = r.IdTuteur,
                    IdEleve = r.IdEleve,
                    NomDestinataire = BuildNomComplet(r.Utilisateur),
                    Email = r.Utilisateur != null ? r.Utilisateur.Email : null,
                    Telephone = r.Utilisateur != null ? r.Utilisateur.Telephone : null,
                    Status = r.Status,
                    PreferredChannel = r.PreferredChannel,
                    FinalChannel = r.FinalChannel,
                    DatePlanifie = r.DatePlanifie,
                    DateEnvoi = r.DateEnvoi,
                    ErrorMessage = r.ErrorMessage,
                    NomEleve = r.Eleve != null ? r.Eleve.NomComplet ?? $"{r.Eleve.Prenom} {r.Eleve.Nom}" : null,
                    Classe = r.Eleve != null && r.Eleve.Classe != null ? r.Eleve.Classe.NomClasse : null
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CommunicationRecipientDto>(data, total, request.PageNumber, request.PageSize);
        }

        public async Task<PagedResult<CommunicationHistoryDto>> GetHistoryAsync(int idCampaign, PagedRequest request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            var accessible = await BuildCampaignQueryForUser(currentUserRole, currentUserEcoleId)
                .AnyAsync(c => c.IdCampaign == idCampaign, cancellationToken);

            if (!accessible)
            {
                throw new KeyNotFoundException("Campagne introuvable ou accès refusé.");
            }

            IQueryable<CommunicationHistory> query = _context.CommunicationHistory
                .AsNoTracking()
                .Where(h => h.IdCampaign == idCampaign)
                .Include(h => h.Utilisateur);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(h => h.Action.ToLower().Contains(term) || (h.DetailJson ?? "").ToLower().Contains(term));
            }

            var orderedQuery = request.SortDescending
                ? query.OrderByDescending(h => h.DateAction)
                : query.OrderBy(h => h.DateAction);

            var total = await orderedQuery.CountAsync(cancellationToken);

            var data = await orderedQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(h => new CommunicationHistoryDto
                {
                    IdHistory = h.IdHistory,
                    Action = h.Action,
                    Detail = h.DetailJson,
                    DateAction = h.DateAction,
                    UserId = h.UserId,
                    NomUtilisateur = BuildNomComplet(h.Utilisateur),
                    AdresseIP = h.AdresseIP
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CommunicationHistoryDto>(data, total, request.PageNumber, request.PageSize);
        }

        public async Task<bool> DispatchCampaignAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default)
        {
            if (currentUserRole != UserRoles.SUPER_ADMIN &&
                currentUserRole != UserRoles.ADMIN &&
                currentUserRole != UserRoles.DIRECTEUR &&
                currentUserRole != UserRoles.SOUS_DIRECTEUR)
            {
                throw new UnauthorizedAccessException("Vous n'avez pas l'autorisation d'envoyer une campagne.");
            }

            var campaign = await FindCampaignForUpdateAsync(idCampaign, currentUserRole, currentUserEcoleId, cancellationToken);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campagne introuvable ou accès refusé.");
            }

            if (campaign.Statut == CampaignStatuses.Envoye)
            {
                throw new InvalidOperationException("Cette campagne a déjà été envoyée.");
            }

            if (campaign.Statut == CampaignStatuses.EnCours)
            {
                throw new InvalidOperationException("La campagne est déjà en cours d'envoi.");
            }

            var totalDestinataires = await RebuildRecipientsAsync(campaign, cancellationToken);

            if (totalDestinataires == 0)
            {
                throw new InvalidOperationException("Aucun destinataire trouvé pour cette campagne.");
            }

            campaign.Statut = CampaignStatuses.EnCours;
            campaign.ValidationAt = DateTime.UtcNow;
            campaign.ValidatedBy = currentUserId;

            await AddHistoryAsync(idCampaign, currentUserId, "EnvoiDemarre", $"Total={totalDestinataires}", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _dispatchScheduler.EnqueueDispatchAsync(idCampaign, currentUserId, cancellationToken);
            return true;
        }

        #endregion

        #region Helpers

        private async Task<int> RebuildRecipientsAsync(CommunicationCampaign campaign, CancellationToken cancellationToken)
        {
            await _context.Entry(campaign).Collection(c => c.Segments).LoadAsync(cancellationToken);

            if (campaign.Segments == null || campaign.Segments.Count == 0)
            {
                throw new InvalidOperationException("La campagne ne contient aucun segment.");
            }

            var existingRecipients = await _context.CampaignRecipients
                .Where(r => r.IdCampaign == campaign.IdCampaign)
                .ToListAsync(cancellationToken);

            if (existingRecipients.Count > 0)
            {
                _context.CampaignRecipients.RemoveRange(existingRecipients);
            }

            var recipients = await GenerateRecipientsAsync(campaign, campaign.Segments.ToList(), cancellationToken);

            if (recipients.Count > 0)
            {
                await _context.CampaignRecipients.AddRangeAsync(recipients, cancellationToken);
            }

            return recipients.Count;
        }

        private IQueryable<CommunicationCampaign> BuildCampaignQueryForUser(string currentUserRole, int? currentUserEcoleId)
        {
            var query = _context.CommunicationCampaigns
                .AsNoTracking()
                .Include(c => c.Ecole)
                .Include(c => c.Auteur)
                .Include(c => c.Validateur)
                .AsQueryable();

            if (currentUserRole == UserRoles.SUPER_ADMIN)
            {
                return query;
            }

            var ecoleId = currentUserEcoleId ?? 0;
            return query.Where(c => c.IdEcole == ecoleId);
        }

        private async Task<CommunicationCampaign?> FindCampaignForUpdateAsync(int idCampaign, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken)
        {
            var query = _context.CommunicationCampaigns.AsQueryable();

            if (currentUserRole != UserRoles.SUPER_ADMIN)
            {
                var ecoleId = currentUserEcoleId ?? 0;
                query = query.Where(c => c.IdEcole == ecoleId);
            }

            return await query.FirstOrDefaultAsync(c => c.IdCampaign == idCampaign, cancellationToken);
        }

        private async Task UpsertSegmentsAsync(CommunicationCampaign campaign, List<CreateCommunicationSegmentDto> segments, int currentUserId, CancellationToken cancellationToken)
        {
            var existing = await _context.CommunicationSegments
                .Where(s => s.IdCampaign == campaign.IdCampaign)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                _context.CommunicationSegments.RemoveRange(existing);
            }

            foreach (var segmentDto in segments)
            {
                var criteria = new CommunicationSegmentCriteria
                {
                    ClasseIds = segmentDto.ClasseIds,
                    DirectionIds = segmentDto.DirectionIds,
                    UtilisateurIds = segmentDto.UtilisateurIds,
                    TuteurIds = segmentDto.TuteurIds,
                    Tags = segmentDto.Tags,
                    Niveau = segmentDto.Niveau
                };

                var segment = new CommunicationSegment
                {
                    IdCampaign = campaign.IdCampaign,
                    IdEcole = campaign.IdEcole,
                    NomSegment = segmentDto.NomSegment.Trim(),
                    TypeSegment = segmentDto.TypeSegment,
                    CriteriaJson = JsonSerializer.Serialize(criteria, JsonOptions),
                    IsReusable = segmentDto.IsReusable,
                    CreatedBy = currentUserId,
                    DateCreation = DateTime.UtcNow
                };

                await _context.CommunicationSegments.AddAsync(segment, cancellationToken);
            }
        }

        private static void ValidateChannels(CommunicationChannelsDto channels)
        {
            if (!channels.Push && !channels.Email && !channels.Sms && !channels.InApp)
            {
                throw new ArgumentException("Au moins un canal doit être activé.");
            }
        }

        private static string BuildNomComplet(Utilisateur? utilisateur)
        {
            if (utilisateur == null)
            {
                return string.Empty;
            }

            var prenom = utilisateur.PrenomUtilisateur ?? "";
            var nom = utilisateur.NomUtilisateur ?? "";
            var postnom = utilisateur.PostNomUtilisateur ?? "";

            return string.Join(" ", new[] { prenom, postnom, nom }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        }

        private static Dictionary<string, object?> DeserializeCriteria(string criteriaJson)
        {
            if (string.IsNullOrWhiteSpace(criteriaJson))
            {
                return new Dictionary<string, object?>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(criteriaJson, JsonOptions)
                       ?? new Dictionary<string, object?>();
            }
            catch
            {
                return new Dictionary<string, object?>();
            }
        }

        private async Task<List<CampaignRecipient>> GenerateRecipientsAsync(CommunicationCampaign campaign, List<CommunicationSegment> segments, CancellationToken cancellationToken)
        {
            var recipients = new Dictionary<int, RecipientCandidate>();

            foreach (var segment in segments)
            {
                var criteria = JsonSerializer.Deserialize<CommunicationSegmentCriteria>(segment.CriteriaJson, JsonOptions) ?? new CommunicationSegmentCriteria();
                var candidates = await ResolveSegmentAsync(campaign, criteria, cancellationToken);

                foreach (var candidate in candidates)
                {
                    recipients[candidate.IdUtilisateur] = candidate;
                }
            }

            if (recipients.Count == 0)
            {
                return new List<CampaignRecipient>();
            }

            var channels = CommunicationChannelHelper.DeserializeChannels(campaign.ChannelsJson);
            var orderedChannels = CommunicationChannelHelper.GetOrderedChannels(channels);

            var tuteurIds = recipients.Values.Where(r => r.IdTuteur.HasValue).Select(r => r.IdTuteur!.Value).Distinct().ToList();
            var preferences = await _context.ParentCommunicationPreferences
                .Where(p => tuteurIds.Contains(p.IdTuteur))
                .ToListAsync(cancellationToken);

            var prefLookup = preferences.GroupBy(p => p.IdTuteur)
                .ToDictionary(g => g.Key, g => g.ToDictionary(p => p.Canal.ToLowerInvariant(), p => p.OptIn));

            var result = new List<CampaignRecipient>(recipients.Count);
            var utcNow = DateTime.UtcNow;

            foreach (var candidate in recipients.Values)
            {
                var allowedChannels = DetermineAllowedChannels(candidate, orderedChannels, prefLookup);
                if (allowedChannels.Count == 0)
                {
                    // Aucun canal disponible → on ignore le destinataire
                    continue;
                }

                var recipient = new CampaignRecipient
                {
                    IdCampaign = campaign.IdCampaign,
                    IdUtilisateur = candidate.IdUtilisateur,
                    IdTuteur = candidate.IdTuteur,
                    IdEleve = candidate.IdEleve,
                    PreferredChannel = allowedChannels[0],
                    Status = "Planifie",
                    DatePlanifie = utcNow,
                    ErrorMessage = null,
                    FinalChannel = null
                };

                result.Add(recipient);
            }

            return result;
        }

        private async Task<List<RecipientCandidate>> ResolveSegmentAsync(CommunicationCampaign campaign, CommunicationSegmentCriteria criteria, CancellationToken cancellationToken)
        {
            var results = new List<RecipientCandidate>();

            if (criteria.UtilisateurIds?.Count > 0)
            {
                var ids = criteria.UtilisateurIds.Distinct().ToList();
                var users = await _context.Utilisateurs
                    .AsNoTracking()
                    .Where(u => ids.Contains(u.IdUtilisateur) && u.Statut == true)
                    .Select(u => new RecipientCandidate
                    {
                        IdUtilisateur = u.IdUtilisateur,
                        IdTuteur = u.IdTuteur,
                        Email = u.Email,
                        Telephone = u.Telephone,
                        NomDestinataire = BuildNomComplet(u)
                    })
                    .ToListAsync(cancellationToken);

                results.AddRange(users);
            }

            if (criteria.TuteurIds?.Count > 0)
            {
                var ids = criteria.TuteurIds.Distinct().ToList();
                var users = await _context.Utilisateurs
                    .AsNoTracking()
                    .Where(u => u.IdTuteur != null && ids.Contains(u.IdTuteur.Value) && u.Statut == true)
                    .Select(u => new RecipientCandidate
                    {
                        IdUtilisateur = u.IdUtilisateur,
                        IdTuteur = u.IdTuteur,
                        Email = u.Email,
                        Telephone = u.Telephone,
                        NomDestinataire = BuildNomComplet(u)
                    })
                    .ToListAsync(cancellationToken);

                results.AddRange(users);
            }

            if (criteria.ClasseIds?.Count > 0)
            {
                var classeIds = criteria.ClasseIds.Where(id => id > 0).Distinct().ToList();
                if (classeIds.Count > 0)
                {
                    var candidats = await (from user in _context.Utilisateurs.AsNoTracking()
                                            where user.IdTuteur != null && user.Statut == true
                                            join tuteur in _context.Tuteurs.AsNoTracking() on user.IdTuteur equals tuteur.IdTuteur
                                            join eleve in _context.Eleves.AsNoTracking() on tuteur.IdTuteur equals eleve.IdTuteur
                                            join classe in _context.Classes.AsNoTracking() on eleve.IdClasse equals classe.IdClasse
                                            where classeIds.Contains(classe.IdClasse) && tuteur.IdEcole == campaign.IdEcole
                                            select new RecipientCandidate
                                            {
                                                IdUtilisateur = user.IdUtilisateur,
                                                IdTuteur = tuteur.IdTuteur,
                                                IdEleve = eleve.IdEleve,
                                                Email = user.Email,
                                                Telephone = user.Telephone,
                                                NomDestinataire = BuildNomComplet(user),
                                                NomEleve = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom}".Trim(),
                                                Classe = classe.NomClasse
                                            }).ToListAsync(cancellationToken);

                    results.AddRange(candidats);
                }
            }

            if (criteria.DirectionIds?.Count > 0)
            {
                var directionIds = criteria.DirectionIds.Distinct().ToList();
                if (directionIds.Count > 0)
                {
                    var candidats = await (from user in _context.Utilisateurs.AsNoTracking()
                                            where user.IdTuteur != null && user.Statut == true
                                            join tuteur in _context.Tuteurs.AsNoTracking() on user.IdTuteur equals tuteur.IdTuteur
                                            join eleve in _context.Eleves.AsNoTracking() on tuteur.IdTuteur equals eleve.IdTuteur
                                            join classe in _context.Classes.AsNoTracking() on eleve.IdClasse equals classe.IdClasse
                                            where classe.IdDirection != null && directionIds.Contains(classe.IdDirection.Value) && tuteur.IdEcole == campaign.IdEcole
                                            select new RecipientCandidate
                                            {
                                                IdUtilisateur = user.IdUtilisateur,
                                                IdTuteur = tuteur.IdTuteur,
                                                IdEleve = eleve.IdEleve,
                                                Email = user.Email,
                                                Telephone = user.Telephone,
                                                NomDestinataire = BuildNomComplet(user),
                                                NomEleve = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom}".Trim(),
                                                Classe = classe.NomClasse
                                            }).ToListAsync(cancellationToken);

                    results.AddRange(candidats);
                }
            }

            if (!string.IsNullOrWhiteSpace(criteria.Niveau))
            {
                var niveau = criteria.Niveau.Trim();
                var directionIds = await _context.Directions.AsNoTracking()
                    .Where(d => d.IdEcole == campaign.IdEcole && d.NiveauEnseignement == niveau)
                    .Select(d => d.IdDirection)
                    .ToListAsync(cancellationToken);

                if (directionIds.Count > 0)
                {
                    var clonedCriteria = new CommunicationSegmentCriteria
                    {
                        ClasseIds = criteria.ClasseIds != null ? new List<int>(criteria.ClasseIds) : null,
                        DirectionIds = criteria.DirectionIds != null ? new List<int>(criteria.DirectionIds) : new List<int>(),
                        UtilisateurIds = criteria.UtilisateurIds != null ? new List<int>(criteria.UtilisateurIds) : null,
                        TuteurIds = criteria.TuteurIds != null ? new List<int>(criteria.TuteurIds) : null,
                        Tags = criteria.Tags != null ? new List<string>(criteria.Tags) : null,
                        Niveau = null
                    };

                    foreach (var dirId in directionIds)
                    {
                        if (!clonedCriteria.DirectionIds!.Contains(dirId))
                        {
                            clonedCriteria.DirectionIds.Add(dirId);
                        }
                    }

                    results.AddRange(await ResolveSegmentAsync(campaign, clonedCriteria, cancellationToken));
                }
            }

            // Tags non implémentés pour le moment – conservés pour évolutions futures

            return results;
        }

        private static List<string> DetermineAllowedChannels(RecipientCandidate candidate, IReadOnlyList<string> orderedChannels, Dictionary<int, Dictionary<string, bool>> prefLookup)
        {
            var channels = new List<string>(4);
            Dictionary<string, bool>? tuteurPrefs = null;

            if (candidate.IdTuteur.HasValue)
            {
                prefLookup.TryGetValue(candidate.IdTuteur.Value, out tuteurPrefs);
            }

            foreach (var channel in orderedChannels)
            {
                var key = channel.ToLowerInvariant();
                var isOptedIn = tuteurPrefs == null || !tuteurPrefs.TryGetValue(key, out var optIn) || optIn;

                if (!isOptedIn)
                {
                    continue;
                }

                switch (channel)
                {
                    case "Email" when string.IsNullOrWhiteSpace(candidate.Email):
                        continue;
                    case "Sms" when string.IsNullOrWhiteSpace(candidate.Telephone):
                        continue;
                }

                channels.Add(channel);
            }

            return channels;
        }

        private async Task AddHistoryAsync(int idCampaign, int? userId, string action, string? detail, CancellationToken cancellationToken)
        {
            string? normalizedDetail = null;

            if (!string.IsNullOrWhiteSpace(detail))
            {
                var trimmed = detail.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                {
                    normalizedDetail = trimmed;
                }
                else
                {
                    normalizedDetail = JsonSerializer.Serialize(new { message = detail });
                }
            }

            var history = new CommunicationHistory
            {
                IdCampaign = idCampaign,
                UserId = userId,
                Action = action,
                DetailJson = normalizedDetail,
                DateAction = DateTime.UtcNow
            };

            await _context.CommunicationHistory.AddAsync(history, cancellationToken);
        }

        #endregion

        #region Inner Types

        private class CommunicationCampaignSummaryProjection
        {
            public int IdCampaign { get; set; }
            public int IdEcole { get; set; }
            public string NomEcole { get; set; } = string.Empty;
            public string Titre { get; set; } = string.Empty;
            public string Importance { get; set; } = string.Empty;
            public string Statut { get; set; } = string.Empty;
            public DateTime DateCreation { get; set; }
            public DateTime? PlanifiedAt { get; set; }
            public DateTime? ExpirationAt { get; set; }
            public bool RappelAuto { get; set; }
            public string Auteur { get; set; } = string.Empty;
            public string? Validateur { get; set; }
        }

        private record RecipientCandidate
        {
            public int IdUtilisateur { get; init; }
            public int? IdTuteur { get; init; }
            public int? IdEleve { get; init; }
            public string Email { get; init; } = string.Empty;
            public string Telephone { get; init; } = string.Empty;
            public string NomDestinataire { get; init; } = string.Empty;
            public string? NomEleve { get; init; }
            public string? Classe { get; init; }
        }

        #endregion
    }
}

