using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class RouteServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly RouteService _service;
        private readonly RouteRepository _repository;

        public RouteServiceTests()
        {
            _db = new SqliteTestDatabase();
            _repository = new RouteRepository();
            _service = new RouteService(_repository);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Add_ValidRoute_AssignsIdAndSavesSuccessfully()
        {
            var route = TestDataFactory.CreateValidRoute(name: "Говерла через Заросляк");

            _service.Add(route);

            Assert.True(route.Id > 0);
            var saved = _service.GetById(route.Id);
            Assert.NotNull(saved);
            Assert.Equal("Говерла через Заросляк", saved!.Name);
            Assert.Equal("Середній", saved.Difficulty);
        }

        [Fact]
        public void Add_NullRoute_ThrowsValidationException()
        {
            var ex = Assert.Throws<ValidationException>(() => _service.Add(null!));
            Assert.Contains("порожніми", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_EmptyOrWhitespaceName_ThrowsValidationException(string? invalidName)
        {
            var route = TestDataFactory.CreateValidRoute();
            route.Name = invalidName!;

            var ex = Assert.Throws<ValidationException>(() => _service.Add(route));
            Assert.Contains("Назва маршруту є обов'язковою", ex.Message);
        }

        [Theory]
        [InlineData("Екстремальний")]
        [InlineData("Дуже легкий")]
        [InlineData("Unknown")]
        public void Add_InvalidDifficulty_ThrowsValidationException(string difficulty)
        {
            var route = TestDataFactory.CreateValidRoute(difficulty: difficulty);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(route));
            Assert.Contains("Складність повинна бути", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Add_ZeroOrNegativeDuration_ThrowsValidationException(int duration)
        {
            var route = TestDataFactory.CreateValidRoute(durationDays: duration);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(route));
            Assert.Contains("Тривалість маршруту повинна бути не менше 1 дня", ex.Message);
        }

        [Fact]
        public void Add_NegativeLengthKm_ThrowsValidationException()
        {
            var route = TestDataFactory.CreateValidRoute(lengthKm: -5.0);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(route));
            Assert.Contains("не може бути від'ємною", ex.Message);
        }

        [Fact]
        public void Update_NonExistentRoute_ThrowsValidationException()
        {
            var route = TestDataFactory.CreateValidRoute();
            route.Id = 99999;

            var ex = Assert.Throws<ValidationException>(() => _service.Update(route));
            Assert.Contains("не знайдено", ex.Message);
        }

        [Fact]
        public void Update_InvalidId_ThrowsValidationException()
        {
            var route = TestDataFactory.CreateValidRoute();
            route.Id = 0;

            var ex = Assert.Throws<ValidationException>(() => _service.Update(route));
            Assert.Contains("Некоректний ідентифікатор", ex.Message);
        }

        [Fact]
        public void Delete_ActiveRoute_MovesToHistory()
        {
            var route = TestDataFactory.CreateValidRoute(name: "Маршрут для архіву");
            _service.Add(route);

            _service.Delete(route.Id);

            var active = _service.GetAll();
            var history = _service.GetHistory();

            Assert.DoesNotContain(active, r => r.Id == route.Id);
            Assert.Contains(history, r => r.Id == route.Id);
        }

        [Fact]
        public void Restore_ArchivedRoute_ReturnsToActiveList()
        {
            var route = TestDataFactory.CreateValidRoute(name: "Маршрут для відновлення");
            _service.Add(route);
            _service.Delete(route.Id);

            _service.Restore(route.Id);

            var active = _service.GetAll();
            var history = _service.GetHistory();

            Assert.Contains(active, r => r.Id == route.Id);
            Assert.DoesNotContain(history, r => r.Id == route.Id);
        }

        [Fact]
        public void HardDelete_RouteUsedInActiveHike_ThrowsValidationException()
        {
            var route = TestDataFactory.CreateValidRoute(name: "Маршрут з походом");
            _service.Add(route);

            var hikeRepo = new HikeRepository();
            var hike = TestDataFactory.CreateValidHike(route.Id, "Активний похід", isDeleted: false);
            hikeRepo.Insert(hike);

            var ex = Assert.Throws<ValidationException>(() => _service.HardDelete(route.Id));
            Assert.Contains("використовується", ex.Message);
        }

        [Fact]
        public void HardDelete_RouteUsedInHistoricalHike_ThrowsValidationException()
        {
            var route = TestDataFactory.CreateValidRoute(name: "Маршрут з архівним походом");
            _service.Add(route);

            var hikeRepo = new HikeRepository();
            var hike = TestDataFactory.CreateValidHike(route.Id, "Архівний похід", isDeleted: true);
            hikeRepo.Insert(hike);

            var ex = Assert.Throws<ValidationException>(() => _service.HardDelete(route.Id));
            Assert.Contains("використовується", ex.Message);
        }
    }
}
