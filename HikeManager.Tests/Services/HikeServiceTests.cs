using System;
using System.Collections.Generic;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class HikeServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly HikeService _hikeService;
        private readonly RouteService _routeService;
        private readonly int _validRouteId;

        public HikeServiceTests()
        {
            _db = new SqliteTestDatabase();
            var hikeRepo = new HikeRepository();
            var routeRepo = new RouteRepository();
            _routeService = new RouteService(routeRepo);
            _hikeService = new HikeService(hikeRepo, _routeService);

            var route = TestDataFactory.CreateValidRoute("Базовий маршрут");
            _routeService.Add(route);
            _validRouteId = route.Id;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Create_ValidHike_AssignsIdAndSavesSuccessfully()
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId, "Травневий похід");

            _hikeService.Create(hike);

            Assert.True(hike.Id > 0);
            var saved = _hikeService.GetById(hike.Id);
            Assert.NotNull(saved);
            Assert.Equal("Травневий похід", saved!.Name);
            Assert.Equal(_validRouteId, saved.RouteId);
        }

        [Fact]
        public void Create_NullHike_ThrowsValidationException()
        {
            var ex = Assert.Throws<ValidationException>(() => _hikeService.Create(null!));
            Assert.Contains("порожніми", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_EmptyOrWhitespaceName_ThrowsValidationException(string? invalidName)
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId);
            hike.Name = invalidName!;

            var ex = Assert.Throws<ValidationException>(() => _hikeService.Create(hike));
            Assert.Contains("Назва походу є обов'язковою", ex.Message);
        }

        [Fact]
        public void Create_DefaultDateTime_ThrowsValidationException()
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId);
            hike.StartDate = default;

            var ex = Assert.Throws<ValidationException>(() => _hikeService.Create(hike));
            Assert.Contains("Дата початку", ex.Message);
        }

        [Fact]
        public void Create_NonExistentRouteId_ThrowsValidationException()
        {
            var hike = TestDataFactory.CreateValidHike(routeId: 99999);

            var ex = Assert.Throws<ValidationException>(() => _hikeService.Create(hike));
            Assert.Contains("Маршрут з ID 99999 не знайдено", ex.Message);
        }

        [Fact]
        public void Update_NonExistentHike_ThrowsValidationException()
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId);
            hike.Id = 88888;

            var ex = Assert.Throws<ValidationException>(() => _hikeService.Update(hike));
            Assert.Contains("не знайдено", ex.Message);
        }

        [Fact]
        public void Delete_ActiveHike_MovesToHistory()
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId, "Похід для архівування");
            _hikeService.Create(hike);

            _hikeService.Delete(hike.Id);

            var active = _hikeService.GetActive();
            var history = _hikeService.GetHistory();

            Assert.DoesNotContain(active, h => h.Id == hike.Id);
            Assert.Contains(history, h => h.Id == hike.Id);
        }

        [Fact]
        public void RestoreHike_ArchivedHike_ReturnsToActiveList()
        {
            var hike = TestDataFactory.CreateValidHike(_validRouteId, "Похід для відновлення");
            _hikeService.Create(hike);
            _hikeService.Delete(hike.Id);

            _hikeService.RestoreHike(hike.Id);

            var active = _hikeService.GetActive();
            var history = _hikeService.GetHistory();

            Assert.Contains(active, h => h.Id == hike.Id);
            Assert.DoesNotContain(history, h => h.Id == hike.Id);
        }

        [Fact]
        public void GetActive_MultipleHikes_ReturnsOrderedByStartDateAscending()
        {
            var hike1 = TestDataFactory.CreateValidHike(_validRouteId, "Похід вересень", new DateTime(2026, 9, 1));
            var hike2 = TestDataFactory.CreateValidHike(_validRouteId, "Похід липень", new DateTime(2026, 7, 1));
            var hike3 = TestDataFactory.CreateValidHike(_validRouteId, "Похід серпень", new DateTime(2026, 8, 1));

            _hikeService.Create(hike1);
            _hikeService.Create(hike2);
            _hikeService.Create(hike3);

            var active = _hikeService.GetActive();

            var dates = active.ConvertAll(h => h.StartDate);
            for (int i = 0; i < dates.Count - 1; i++)
            {
                Assert.True(dates[i] <= dates[i + 1]);
            }
        }
    }
}

