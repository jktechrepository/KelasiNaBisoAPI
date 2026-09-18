using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class NoteServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly NoteService _service;
        private readonly EvaluationService _evaluationService;
        private readonly DateTime _now = DateTime.Now;

        public NoteServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new NoteService(_context, scope);
            _evaluationService = new EvaluationService(_context, new PeriodeCotationResolver(_context));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-3), _now.AddMonths(6)));
            _context.Utilisateurs.Add(TestDataBuilder.CreateUtilisateur(5, "prof@test.com", "Prof"));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Eleve A"));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(2, null, "Eleve B"));
            _context.Cours.Add(new Cours
            {
                IdCours = 50,
                NomCours = "Math",
                IdClasse = 10,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.Evaluations.Add(new Evaluation
            {
                IdEvaluation = 1,
                TypeEvaluation = "Interro",
                TitreEvaluation = "Interro 1",
                Periode = "Trimestre 1",
                Coefficient = 2,
                IdCours = 50,
                IdClasse = 10,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(2, 2, 1, 10, 100));
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_WithoutAnnee_DefaultsToCurrentYear()
        {
            var note = await _service.CreateAsync(new Note
            {
                NoteObtenue = 15,
                IdProfesseur = 5,
                IdEleve = 1,
                IdEvaluation = 1,
                IdAnneeScolaire = 0
            });

            note.IdAnneeScolaire.Should().Be(100);
            note.IdNote.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_DuplicateEleveEvaluation_Throws()
        {
            await _service.CreateAsync(new Note
            {
                NoteObtenue = 12,
                IdProfesseur = 5,
                IdEleve = 1,
                IdEvaluation = 1,
                IdAnneeScolaire = 100
            });

            var act = async () => await _service.CreateAsync(new Note
            {
                NoteObtenue = 14,
                IdProfesseur = 5,
                IdEleve = 1,
                IdEvaluation = 1,
                IdAnneeScolaire = 100
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*existe déjà*");
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletes()
        {
            var note = await _service.CreateAsync(new Note
            {
                NoteObtenue = 10,
                IdProfesseur = 5,
                IdEleve = 1,
                IdEvaluation = 1,
                IdAnneeScolaire = 100
            });

            var ok = await _service.DeleteAsync(note.IdNote);
            ok.Should().BeTrue();
            (await _service.GetByIdAsync(note.IdNote)).Should().BeNull();
        }

        [Fact]
        public async Task UpsertBulk_CreatesThenUpdates()
        {
            var first = await _service.UpsertBulkAsync(new Models.DTOs.BulkNoteRequestDto
            {
                IdEvaluation = 1,
                IdAnneeScolaire = 100,
                Lignes =
                {
                    new Models.DTOs.BulkNoteLigneDto { IdEleve = 1, NoteObtenue = 12 },
                    new Models.DTOs.BulkNoteLigneDto { IdEleve = 2, NoteObtenue = 15, Appreciation = "Bien" }
                }
            }, idProfesseur: 5);

            first.Created.Should().Be(2);
            first.Updated.Should().Be(0);

            var second = await _service.UpsertBulkAsync(new Models.DTOs.BulkNoteRequestDto
            {
                IdEvaluation = 1,
                IdAnneeScolaire = 100,
                Lignes =
                {
                    new Models.DTOs.BulkNoteLigneDto { IdEleve = 1, NoteObtenue = 14 }
                }
            }, idProfesseur: 5);

            second.Created.Should().Be(0);
            second.Updated.Should().Be(1);
            second.Notes.Single().NoteObtenue.Should().Be(14);
        }

        [Fact]
        public async Task UpsertBulk_EleveHorsClasse_Throws()
        {
            _context.Eleves.Add(TestDataBuilder.CreateEleve(99, null, "Hors Classe"));
            _context.SaveChanges();

            var act = async () => await _service.UpsertBulkAsync(new Models.DTOs.BulkNoteRequestDto
            {
                IdEvaluation = 1,
                IdAnneeScolaire = 100,
                Lignes = { new Models.DTOs.BulkNoteLigneDto { IdEleve = 99, NoteObtenue = 10 } }
            }, idProfesseur: 5);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*hors classe*");
        }

        [Fact]
        public async Task CreateEvaluation_MissingCours_Throws()
        {
            var act = async () => await _evaluationService.CreateAsync(new Evaluation
            {
                TypeEvaluation = "Examen",
                IdCours = 999,
                IdClasse = 10
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Cours*");
        }

        public void Dispose() => _context.Dispose();
    }
}
