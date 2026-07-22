using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    /// <summary>
    /// Tests unitaires pour ClasseService - Pagination
    /// </summary>
    public class ClasseServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ClasseService _classeService;

        public ClasseServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _classeService = new ClasseService(_context);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnPagedResult_WhenClassesExist()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");
            var section = TestDataBuilder.CreateSection(1, "Section Test", 1);
            var option = TestDataBuilder.CreateOption(1, "Option Test", 1);

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Sections.Add(section);
            _context.Options.Add(option);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "5ème A", 1, 1, 1),
                TestDataBuilder.CreateClasse(2, "5ème B", 1, 1, 1),
                TestDataBuilder.CreateClasse(3, "6ème A", 1, 1, 1),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.TotalRecords.Should().Be(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(1);
            result.HasPrevious.Should().BeFalse();
            result.HasNext.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnOnlyActiveClasses_WhenIncludeInactiveIsFalse()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "5ème A", 1, statut: true),
                TestDataBuilder.CreateClasse(2, "5ème B", 1, statut: false), // Inactive
                TestDataBuilder.CreateClasse(3, "6ème A", 1, statut: true),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10,
                IncludeInactive = false
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.All(c => c.Statut == true).Should().BeTrue();
            result.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnAllClasses_WhenIncludeInactiveIsTrue()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "5ème A", 1, statut: true),
                TestDataBuilder.CreateClasse(2, "5ème B", 1, statut: false), // Inactive
                TestDataBuilder.CreateClasse(3, "6ème A", 1, statut: true),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10,
                IncludeInactive = true
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.TotalRecords.Should().Be(3);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldFilterBySearchTerm_WhenSearchTermIsProvided()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "5ème A", 1),
                TestDataBuilder.CreateClasse(2, "5ème B", 1),
                TestDataBuilder.CreateClasse(3, "6ème A", 1),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "5ème"
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.All(c => c.NomClasse.Contains("5ème")).Should().BeTrue();
            result.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldSortByNomClasseAscending_WhenSortByIsNotProvided()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "6ème A", 1),
                TestDataBuilder.CreateClasse(2, "5ème B", 1),
                TestDataBuilder.CreateClasse(3, "5ème A", 1),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10,
                SortDescending = false
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.Data[0].NomClasse.Should().Be("5ème A");
            result.Data[1].NomClasse.Should().Be("5ème B");
            result.Data[2].NomClasse.Should().Be("6ème A");
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldSortByNomClasseDescending_WhenSortDescendingIsTrue()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>
            {
                TestDataBuilder.CreateClasse(1, "5ème A", 1),
                TestDataBuilder.CreateClasse(2, "5ème B", 1),
                TestDataBuilder.CreateClasse(3, "6ème A", 1),
            };

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10,
                SortDescending = true
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.Data[0].NomClasse.Should().Be("6ème A");
            result.Data[1].NomClasse.Should().Be("5ème B");
            result.Data[2].NomClasse.Should().Be("5ème A");
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldPaginateCorrectly_WhenMultiplePages()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);

            var classes = new List<Classe>();
            for (int i = 1; i <= 25; i++)
            {
                classes.Add(TestDataBuilder.CreateClasse(i, $"Classe {i}", 1));
            }

            _context.Classes.AddRange(classes);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 2,
                PageSize = 10
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(10);
            result.TotalRecords.Should().Be(25);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(3);
            result.HasPrevious.Should().BeTrue();
            result.HasNext.Should().BeTrue();
            result.FirstRowOnPage.Should().Be(11);
            result.LastRowOnPage.Should().Be(20);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnEmptyResult_WhenNoClassesExist()
        {
            // Arrange
            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
            result.HasPrevious.Should().BeFalse();
            result.HasNext.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldIncludeNavigationProperties()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1, "Direction Test");
            var section = TestDataBuilder.CreateSection(1, "Section Test", 1);
            var option = TestDataBuilder.CreateOption(1, "Option Test", 1);

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Sections.Add(section);
            _context.Options.Add(option);

            var classe = TestDataBuilder.CreateClasse(1, "5ème A", 1, 1, 1);
            _context.Classes.Add(classe);
            await _context.SaveChangesAsync();

            var request = new PagedRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _classeService.GetAllPagedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            var classeResult = result.Data.First();
            classeResult.Direction.Should().NotBeNull();
            classeResult.Section.Should().NotBeNull();
            classeResult.Option.Should().NotBeNull();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}





