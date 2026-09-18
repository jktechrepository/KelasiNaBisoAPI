using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class NoteService : INoteRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public NoteService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _context.Notes
                .Include(n => n.AnneeScolaire)
                .Include(n => n.Evaluation)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<Note> GetByIdAsync(int id)
        {
            return await _context.Notes
                .Where(n => n.Statut == true)
                .FirstOrDefaultAsync(n => n.IdNote == id);
        }

        public async Task<IEnumerable<Note>> GetByEleveAsync(int idEleve)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.IdEleve == idEleve)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByEvaluationAsync(int idEvaluation)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.IdEvaluation == idEvaluation)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByCoursAsync(int idCours)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.IdCours == idCours)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByProfesseurAsync(int idProfesseur)
        {
            return await _context.Notes
                .Where(n => n.IdProfesseur == idProfesseur)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.IdAnneeScolaire == idAnneeScolaire)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByPeriodeAsync(string periode)
        {
            var code = PeriodeCotationAliases.ResolveCode(periode);
            if (!string.IsNullOrEmpty(code))
            {
                var periodeEntity = await _context.PeriodesCotation.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Code == code && p.Statut);
                if (periodeEntity != null)
                {
                    var aliases = PeriodeCotationAliases.GetAliases(code).ToList();
                    return await _context.Notes
                        .Include(n => n.Evaluation)
                        .Where(n => n.Statut == true
                            && n.Evaluation != null
                            && (n.Evaluation.IdPeriode == periodeEntity.IdPeriode
                                || (n.Evaluation.IdPeriode == null
                                    && n.Evaluation.Periode != null
                                    && aliases.Contains(n.Evaluation.Periode))))
                        .ToListAsync();
                }
            }

            return await _context.Notes
                .Include(n => n.Evaluation)
                .Where(n => n.Evaluation.Periode == periode)
                .Where(n => n.Statut == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetBySessionAsync(string session)
        {
            return await GetByPeriodeAsync(session);
        }

        public async Task<Note> CreateAsync(Note note)
        {
            await ValidateAndNormalizeForCreateAsync(note);
            note.DateCreation = DateTime.Now;
            note.Statut ??= true;

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<BulkNoteResultDto> UpsertBulkAsync(
            BulkNoteRequestDto request,
            int idProfesseur,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new InvalidOperationException("Corps de requête requis.");
            if (request.Lignes == null || request.Lignes.Count == 0)
                throw new InvalidOperationException("Au moins une ligne est requise.");
            if (request.Lignes.Count > 80)
                throw new InvalidOperationException("Maximum 80 notes par lot.");

            var duplicateEleves = request.Lignes
                .GroupBy(l => l.IdEleve)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateEleves.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Élèves en double dans le lot : {string.Join(", ", duplicateEleves)}.");
            }

            foreach (var ligne in request.Lignes)
            {
                if (ligne.NoteObtenue < 0 || ligne.NoteObtenue > 100)
                    throw new InvalidOperationException(
                        $"NoteObtenue invalide pour l'élève {ligne.IdEleve} (0–100).");
            }

            var evaluation = await _context.Evaluations.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEvaluation == request.IdEvaluation && e.Statut == true, cancellationToken)
                ?? throw new InvalidOperationException($"Évaluation {request.IdEvaluation} introuvable ou inactive.");

            var professeurExists = await _context.Utilisateurs.AsNoTracking()
                .AnyAsync(u => u.IdUtilisateur == idProfesseur && u.Statut == true, cancellationToken);
            if (!professeurExists)
                throw new InvalidOperationException($"Professeur {idProfesseur} introuvable ou inactif.");

            var idAnnee = request.IdAnneeScolaire;
            if (idAnnee <= 0)
            {
                var idEcole = await _context.Classes.AsNoTracking()
                    .Where(c => c.IdClasse == evaluation.IdClasse)
                    .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!idEcole.HasValue || idEcole.Value <= 0)
                    throw new InvalidOperationException(
                        "Impossible de résoudre l'année scolaire : précisez IdAnneeScolaire.");

                idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, null);
            }
            else
            {
                var anneeOk = await _context.AnneeScolaires.AsNoTracking()
                    .AnyAsync(a => a.IdAnneeScolaire == idAnnee && a.Statut == true, cancellationToken);
                if (!anneeOk)
                    throw new InvalidOperationException($"Année scolaire {idAnnee} introuvable.");
            }

            var eleveIds = request.Lignes.Select(l => l.IdEleve).ToList();
            var elevesOk = await _context.Eleves.AsNoTracking()
                .Where(e => eleveIds.Contains(e.IdEleve) && e.Statut == true)
                .Select(e => e.IdEleve)
                .ToListAsync(cancellationToken);
            var missingEleves = eleveIds.Except(elevesOk).ToList();
            if (missingEleves.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Élèves introuvables ou inactifs : {string.Join(", ", missingEleves)}.");
            }

            // Élèves inscrits dans la classe de l'évaluation pour l'année
            var elevesInClasse = await _context.Inscriptions.AsNoTracking()
                .Where(i => i.IdClasse == evaluation.IdClasse
                    && i.IdAnneeScolaire == idAnnee
                    && i.Statut == true
                    && eleveIds.Contains(i.IdEleve))
                .Select(i => i.IdEleve)
                .Distinct()
                .ToListAsync(cancellationToken);
            var horsClasse = eleveIds.Except(elevesInClasse).ToList();
            if (horsClasse.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Élèves hors classe de l'évaluation ({evaluation.IdClasse}) pour l'année {idAnnee} : {string.Join(", ", horsClasse)}.");
            }

            var existingNotes = await _context.Notes
                .Where(n => n.IdEvaluation == request.IdEvaluation
                    && n.Statut == true
                    && eleveIds.Contains(n.IdEleve))
                .ToListAsync(cancellationToken);
            var byEleve = existingNotes.ToDictionary(n => n.IdEleve);

            var dateEval = request.DateEvaluation ?? DateTime.Now;
            var summaries = new List<NoteSummaryDto>();
            var created = 0;
            var updated = 0;

            var useTransaction = _context.Database.IsRelational();
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;
            if (useTransaction)
                tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var ligne in request.Lignes)
                {
                    if (byEleve.TryGetValue(ligne.IdEleve, out var existing))
                    {
                        existing.NoteObtenue = ligne.NoteObtenue;
                        existing.Appreciation = ligne.Appreciation ?? string.Empty;
                        existing.DateEvaluation = dateEval;
                        existing.IdProfesseur = idProfesseur;
                        existing.IdAnneeScolaire = idAnnee;
                        updated++;
                        summaries.Add(new NoteSummaryDto
                        {
                            IdNote = existing.IdNote,
                            IdEleve = existing.IdEleve,
                            NoteObtenue = existing.NoteObtenue,
                            Appreciation = existing.Appreciation,
                            Action = "updated"
                        });
                    }
                    else
                    {
                        var note = new Note
                        {
                            NoteObtenue = ligne.NoteObtenue,
                            Appreciation = ligne.Appreciation ?? string.Empty,
                            DateEvaluation = dateEval,
                            IdProfesseur = idProfesseur,
                            IdEleve = ligne.IdEleve,
                            IdEvaluation = request.IdEvaluation,
                            IdAnneeScolaire = idAnnee,
                            Statut = true,
                            DateCreation = DateTime.Now
                        };
                        _context.Notes.Add(note);
                        await _context.SaveChangesAsync(cancellationToken);
                        created++;
                        summaries.Add(new NoteSummaryDto
                        {
                            IdNote = note.IdNote,
                            IdEleve = note.IdEleve,
                            NoteObtenue = note.NoteObtenue,
                            Appreciation = note.Appreciation,
                            Action = "created"
                        });
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                if (tx != null)
                    await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                if (tx != null)
                    await tx.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (tx != null)
                    await tx.DisposeAsync();
            }

            return new BulkNoteResultDto
            {
                IdEvaluation = request.IdEvaluation,
                IdAnneeScolaire = idAnnee,
                Created = created,
                Updated = updated,
                Notes = summaries
            };
        }

        public async Task<Note> UpdateAsync(Note note)
        {
            var existingNote = await _context.Notes.FindAsync(note.IdNote);
            if (existingNote == null)
                return null;

            if (note.NoteObtenue < 0 || note.NoteObtenue > 100)
                throw new InvalidOperationException("NoteObtenue doit être entre 0 et 100.");

            existingNote.NoteObtenue = note.NoteObtenue;
            existingNote.Appreciation = note.Appreciation;
            await _context.SaveChangesAsync();
            return existingNote;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            // Soft delete (aligné avec ToggleStatut)
            note.Statut = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Notes.AnyAsync(n => n.IdNote == id && n.Statut == true);
        }

        public async Task<bool> ToggleStatutAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return false;

            note.Statut = note.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidateAndNormalizeForCreateAsync(Note note)
        {
            if (note.NoteObtenue < 0 || note.NoteObtenue > 100)
                throw new InvalidOperationException("NoteObtenue doit être entre 0 et 100.");

            var evaluation = await _context.Evaluations.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEvaluation == note.IdEvaluation && e.Statut == true)
                ?? throw new InvalidOperationException($"Évaluation {note.IdEvaluation} introuvable ou inactive.");

            var eleve = await _context.Eleves.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEleve == note.IdEleve && e.Statut == true)
                ?? throw new InvalidOperationException($"Élève {note.IdEleve} introuvable ou inactif.");

            var professeurExists = await _context.Utilisateurs.AsNoTracking()
                .AnyAsync(u => u.IdUtilisateur == note.IdProfesseur && u.Statut == true);
            if (!professeurExists)
                throw new InvalidOperationException($"Professeur {note.IdProfesseur} introuvable ou inactif.");

            if (note.IdAnneeScolaire <= 0)
            {
                var idEcole = await _context.Inscriptions.AsNoTracking()
                    .Where(i => i.IdEleve == note.IdEleve && i.Statut == true)
                    .OrderByDescending(i => i.DateInscription)
                    .Select(i => (int?)i.IdEcole)
                    .FirstOrDefaultAsync();

                if (!idEcole.HasValue || idEcole.Value <= 0)
                {
                    idEcole = await _context.Classes.AsNoTracking()
                        .Where(c => c.IdClasse == evaluation.IdClasse)
                        .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                        .FirstOrDefaultAsync();
                }

                if (!idEcole.HasValue || idEcole.Value <= 0)
                    throw new InvalidOperationException(
                        "Impossible de résoudre l'année scolaire : précisez IdAnneeScolaire.");

                note.IdAnneeScolaire = await _scope.ResolveIdAnneeScolaireAsync(idEcole.Value, null);
            }
            else
            {
                var anneeOk = await _context.AnneeScolaires.AsNoTracking()
                    .AnyAsync(a => a.IdAnneeScolaire == note.IdAnneeScolaire && a.Statut == true);
                if (!anneeOk)
                    throw new InvalidOperationException($"Année scolaire {note.IdAnneeScolaire} introuvable.");
            }

            var duplicate = await _context.Notes.AsNoTracking()
                .AnyAsync(n => n.IdEleve == note.IdEleve
                    && n.IdEvaluation == note.IdEvaluation
                    && n.Statut == true);
            if (duplicate)
            {
                throw new InvalidOperationException(
                    $"Une note active existe déjà pour l'élève {note.IdEleve} et l'évaluation {note.IdEvaluation}.");
            }

            if (note.DateEvaluation == default)
                note.DateEvaluation = DateTime.Now;

            note.Appreciation ??= string.Empty;
        }
    }
}
