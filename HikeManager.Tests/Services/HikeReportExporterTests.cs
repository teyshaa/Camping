using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class HikeReportExporterTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly HikeReportExporter _exporter;
        private readonly Hike _testHike;

        public HikeReportExporterTests()
        {
            _db = new SqliteTestDatabase();

            var routeRepo = new RouteRepository();
            var route = TestDataFactory.CreateValidRoute("Чорногірський масив", "Карпати", "Складний", 4, 45.0);
            routeRepo.Insert(route);

            var hikeRepo = new HikeRepository();
            _testHike = TestDataFactory.CreateValidHike(route.Id, "Великий похід", new DateTime(2026, 8, 10), durationDays: 4);
            hikeRepo.Insert(_testHike);

            var partRepo = new ParticipantRepository();
            partRepo.Insert(TestDataFactory.CreateValidParticipant(_testHike.Id, "Іван Франко", "+380501112233"));
            partRepo.Insert(TestDataFactory.CreateValidParticipant(_testHike.Id, "Леся Українка", "+380672223344"));

            var equipRepo = new EquipmentRepository();
            equipRepo.Insert(TestDataFactory.CreateValidEquipmentItem(_testHike.Id, "Палатка 3-місна", 1));

            var expRepo = new ExpenseRepository();
            expRepo.Insert(TestDataFactory.CreateValidExpense(_testHike.Id, "Харчування", 1200m));
            expRepo.Insert(TestDataFactory.CreateValidExpense(_testHike.Id, "Проїзд", 800m));

            _exporter = new HikeReportExporter();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Generate_NullHike_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => _exporter.Generate(null!));
        }

        [Fact]
        public void Generate_HikeWithZeroOrNegativeId_ThrowsValidationException()
        {
            var invalidHike = new Hike { Id = 0 };
            Assert.Throws<ValidationException>(() => _exporter.Generate(invalidHike));
        }

        [Fact]
        public void Generate_NonExistentRoute_ThrowsValidationException()
        {
            var hikeWithBadRoute = new Hike { Id = _testHike.Id, RouteId = 99999 };
            var ex = Assert.Throws<ValidationException>(() => _exporter.Generate(hikeWithBadRoute));
            Assert.Contains("Маршрут не знайдено", ex.Message);
        }

        [Fact]
        public void Generate_ValidHike_ProducesCompleteTextReport()
        {
            string report = _exporter.Generate(_testHike);

            Assert.Contains("ЗВІТ ПРО ГІРСЬКИЙ ПОХІД", report);
            Assert.Contains("Великий похід", report);
            Assert.Contains("Чорногірський масив", report);
            Assert.Contains("Іван Франко", report);
            Assert.Contains("Леся Українка", report);
            Assert.Contains("Палатка 3-місна", report);
            Assert.Contains("Харчування: 1 200,00 грн", report.Replace("\u00A0", " "));
            Assert.Contains("Проїзд: 800,00 грн", report.Replace("\u00A0", " "));
            Assert.Contains("Загальний бюджет: 2 000,00 грн", report.Replace("\u00A0", " "));
            Assert.Contains("На людину: 1 000,00 грн", report.Replace("\u00A0", " "));
        }
    }
}

