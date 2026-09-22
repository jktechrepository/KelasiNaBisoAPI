using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AppUpdateServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly AppUpdateService _sut;

        public AppUpdateServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new AppUpdateService(_context, Mock.Of<IFirebaseNotificationService>());
            Seed();
        }

        private void Seed()
        {
            _context.MobileAppVersionPolicies.Add(new MobileAppVersionPolicy
            {
                IdMobileAppVersionPolicy = 1,
                Platform = AppUpdatePlatforms.Android,
                MinSupportedVersion = "2.0.0",
                RecommendFromVersion = "2.5.0",
                LatestVersion = "3.0.0",
                Message = "Mettez à jour l'application.",
                StoreUrl = "https://play.google.com/store/apps/details?id=cd.kelasi",
                IsActive = true,
                DateCreation = DateTime.UtcNow
            });
            _context.MobileAppVersionPolicies.Add(new MobileAppVersionPolicy
            {
                IdMobileAppVersionPolicy = 2,
                Platform = AppUpdatePlatforms.Ios,
                MinSupportedVersion = "1.0.0",
                RecommendFromVersion = "1.0.0",
                LatestVersion = "1.0.0",
                IsActive = false,
                DateCreation = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [Theory]
        [InlineData("1.9.9", AppUpdateLevels.Force)]
        [InlineData("2.0.0", AppUpdateLevels.Important)]
        [InlineData("2.4.9", AppUpdateLevels.Important)]
        [InlineData("2.5.0", AppUpdateLevels.Soft)]
        [InlineData("2.9.0", AppUpdateLevels.Soft)]
        [InlineData("3.0.0", AppUpdateLevels.None)]
        [InlineData("3.1.0", AppUpdateLevels.None)]
        public void ResolveUpdateLevel_MatchesPlanRules(string client, string expected)
        {
            var level = AppUpdateService.ResolveUpdateLevel(client, "2.0.0", "2.5.0", "3.0.0");
            level.Should().Be(expected);
        }

        [Fact]
        public void CompareSemver_OrdersCorrectly()
        {
            AppUpdateService.CompareSemver("1.0.0", "1.0.1").Should().BeNegative();
            AppUpdateService.CompareSemver("2.0.0", "1.9.9").Should().BePositive();
            AppUpdateService.CompareSemver("1.2.3", "1.2.3").Should().Be(0);
            AppUpdateService.CompareSemver("v1.2.0", "1.2.0").Should().Be(0);
        }

        [Fact]
        public async Task CheckPolicy_Force_WhenBelowMin()
        {
            var result = await _sut.CheckPolicyAsync("Android", "1.5.0");
            result.UpdateLevel.Should().Be(AppUpdateLevels.Force);
            result.ForceUpdate.Should().BeTrue();
            result.StoreUrl.Should().Contain("play.google.com");
            result.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CheckPolicy_Inactive_ReturnsNone()
        {
            var result = await _sut.CheckPolicyAsync("iOS", "0.0.1");
            result.UpdateLevel.Should().Be(AppUpdateLevels.None);
            result.ForceUpdate.Should().BeFalse();
        }

        [Fact]
        public async Task UpsertPolicy_UpdatesAndActivates()
        {
            var dto = await _sut.UpsertPolicyAsync("iOS", new UpsertAppUpdatePolicyDto
            {
                MinSupportedVersion = "1.1.0",
                RecommendFromVersion = "1.2.0",
                LatestVersion = "1.3.0",
                Message = "Update iOS",
                StoreUrl = "https://apps.apple.com/app/id123",
                IsActive = true
            }, idAuteur: 99);

            dto.Platform.Should().Be("iOS");
            dto.IsActive.Should().BeTrue();
            dto.LatestVersion.Should().Be("1.3.0");

            var check = await _sut.CheckPolicyAsync("ios", "1.0.0");
            check.UpdateLevel.Should().Be(AppUpdateLevels.Force);
        }

        [Fact]
        public async Task CheckPolicy_InvalidPlatform_Throws()
        {
            var act = () => _sut.CheckPolicyAsync("Windows", "1.0.0");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        public void Dispose() => _context.Dispose();
    }
}
