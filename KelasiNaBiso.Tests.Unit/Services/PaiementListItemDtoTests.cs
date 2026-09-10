using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PaiementListItemDtoTests
    {
        [Fact]
        public void FromEntity_MapsMatriculeAndNomComplet()
        {
            var paiement = new Paiement
            {
                IdPaiement = 42,
                IdEleve = 10,
                Montant = 50000,
                StatutPaiement = "Confirme",
                ModePaiement = "Mobile Money",
                Eleve = new Eleve
                {
                    IdEleve = 10,
                    Matricule = "ELV-2024-001",
                    NomComplet = "KABONGO Jean Pierre"
                }
            };

            var dto = PaiementListItemDto.FromEntity(paiement);

            dto.IdPaiement.Should().Be(42);
            dto.IdEleve.Should().Be(10);
            dto.MatriculeEleve.Should().Be("ELV-2024-001");
            dto.NomCompletEleve.Should().Be("KABONGO Jean Pierre");
        }

        [Fact]
        public void FromEntity_WhenNoEleve_LeavesMatriculeAndNomCompletNull()
        {
            var paiement = new Paiement
            {
                IdPaiement = 1,
                IdEleve = null,
                Montant = 1000
            };

            var dto = PaiementListItemDto.FromEntity(paiement);

            dto.MatriculeEleve.Should().BeNull();
            dto.NomCompletEleve.Should().BeNull();
        }
    }
}
