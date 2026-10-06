using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class EleveDetailDtoTests
    {
        [Fact]
        public void From_InclutLesInformationsDuTuteur()
        {
            var eleve = new Eleve
            {
                IdEleve = 7,
                NomComplet = "Alice Dupont",
                IdTuteur = 12,
                Tuteur = new Tuteur
                {
                    IdTuteur = 12,
                    NomComplet = "Marie Dupont",
                    Genre = "F",
                    Email = "marie@example.com",
                    Telephone = "+243810000001",
                    NomCompletRepresentant = "Jean Dupont",
                    TelephoneRepresentant = "+243810000002",
                    PhotoTuteurUrl = "photo.jpg",
                    PieceIdentiteTuteur = "piece.pdf",
                    Statut = true,
                    SerialNumber = "TUT-12"
                }
            };

            var result = EleveDetailDto.From(eleve);

            result.Tuteur.Should().NotBeNull();
            result.Tuteur!.IdTuteur.Should().Be(12);
            result.Tuteur.NomComplet.Should().Be("Marie Dupont");
            result.Tuteur.Email.Should().Be("marie@example.com");
            result.Tuteur.TelephoneRepresentant.Should().Be("+243810000002");
            result.Tuteur.SerialNumber.Should().Be("TUT-12");
        }

        [Fact]
        public void From_SansTuteur_LaisseTuteurNull()
        {
            var result = EleveDetailDto.From(new Eleve { IdEleve = 7 });

            result.Tuteur.Should().BeNull();
        }
    }
}
