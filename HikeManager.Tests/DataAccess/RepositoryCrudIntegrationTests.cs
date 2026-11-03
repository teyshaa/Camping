using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.DataAccess
{
    [Collection("DatabaseCollection")]
    public class RepositoryCrudIntegrationTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly RouteRepository _routeRepo;
        private readonly HikeRepository _hikeRepo;
        private readonly ParticipantRepository _partRepo;
        private readonly EquipmentRepository _equipRepo;
        private readonly ExpenseRepository _expRepo;

        public RepositoryCrudIntegrationTests()
        {
            _db = new SqliteTestDatabase();
            _routeRepo = new RouteRepository();
            _hikeRepo = new HikeRepository();
            _partRepo = new ParticipantRepository();
            _equipRepo = new EquipmentRepository();
            _expRepo = new ExpenseRepository();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void RouteRepository_CrudOperations_WorkCorrectly()
        {
            var route = TestDataFactory.CreateValidRoute("Маршрут CRUD", "Буковина", "Легкий", 2, 18.0);
            _routeRepo.Insert(route);
            Assert.True(route.Id > 0);

            var loaded = _routeRepo.GetById(route.Id);
            Assert.NotNull(loaded);
            Assert.Equal("Маршрут CRUD", loaded!.Name);
            Assert.Equal("Буковина", loaded.Location);
            Assert.Equal(18.0, loaded.LengthKm);

            loaded.Name = "Маршрут CRUD Оновлений";
            loaded.DurationDays = 3;
            _routeRepo.Update(loaded);

            var updated = _routeRepo.GetById(route.Id);
            Assert.NotNull(updated);
            Assert.Equal("Маршрут CRUD Оновлений", updated!.Name);
            Assert.Equal(3, updated.DurationDays);

            _routeRepo.Delete(route.Id);
            Assert.Null(_routeRepo.GetById(route.Id));
        }

        [Fact]
        public void HikeRepository_DateStoredAsYyyyMmDd_ReadsBackIdentically()
        {
            var route = TestDataFactory.CreateValidRoute("Маршрут для дати");
            _routeRepo.Insert(route);

            var expectedDate = new DateTime(2027, 5, 23);
            var hike = TestDataFactory.CreateValidHike(route.Id, "Похід на Трійцю", expectedDate);
            _hikeRepo.Insert(hike);

            var loaded = _hikeRepo.GetById(hike.Id);
            Assert.NotNull(loaded);
            Assert.Equal(expectedDate, loaded!.StartDate);
            Assert.Equal("2027-05-23", loaded.StartDate.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void ParticipantRepository_HandlesNullsCyrillicAndApostrophes()
        {
            var route = TestDataFactory.CreateValidRoute();
            _routeRepo.Insert(route);

            var hike = TestDataFactory.CreateValidHike(route.Id);
            _hikeRepo.Insert(hike);

            var participant = new Participant
            {
                HikeId = hike.Id,
                FullName = "Мар'ян Дем'янчук-Лук'яненко",
                Phone = null,
                Note = null
            };
            _partRepo.Insert(participant);

            var list = _partRepo.GetByHike(hike.Id);
            var loaded = list.Find(p => p.FullName == "Мар'ян Дем'янчук-Лук'яненко");

            Assert.NotNull(loaded);
            Assert.Equal("Мар'ян Дем'янчук-Лук'яненко", loaded!.FullName);
            Assert.Null(loaded.Phone);
            Assert.Null(loaded.Note);
        }

        [Fact]
        public void EquipmentRepository_CrudOperations_WorkCorrectly()
        {
            var route = TestDataFactory.CreateValidRoute();
            _routeRepo.Insert(route);

            var hike = TestDataFactory.CreateValidHike(route.Id);
            _hikeRepo.Insert(hike);

            var item = new EquipmentItem
            {
                HikeId = hike.Id,
                Name = "Каремат Іжевський",
                Quantity = 4
            };
            _equipRepo.Insert(item);

            var list = _equipRepo.GetByHike(hike.Id);
            var loaded = list.Find(x => x.Name == "Каремат Іжевський");
            Assert.NotNull(loaded);
            Assert.Equal(4, loaded!.Quantity);

            loaded.Quantity = 6;
            _equipRepo.Update(loaded);

            var updatedList = _equipRepo.GetByHike(hike.Id);
            var updated = updatedList.Find(x => x.Id == loaded.Id);
            Assert.NotNull(updated);
            Assert.Equal(6, updated!.Quantity);

            _equipRepo.Delete(loaded.Id);
            var finalList = _equipRepo.GetByHike(hike.Id);
            Assert.DoesNotContain(finalList, x => x.Id == loaded.Id);
        }

        [Fact]
        public void ExpenseRepository_CrudOperations_WorkCorrectly()
        {
            var route = TestDataFactory.CreateValidRoute();
            _routeRepo.Insert(route);

            var hike = TestDataFactory.CreateValidHike(route.Id);
            _hikeRepo.Insert(hike);

            var expense = new Expense
            {
                HikeId = hike.Id,
                Category = "Страхування",
                Amount = 350.50m,
                Description = "Медичний поліс"
            };
            _expRepo.Insert(expense);

            var list = _expRepo.GetByHike(hike.Id);
            var loaded = list.Find(e => e.Category == "Страхування");
            Assert.NotNull(loaded);
            Assert.Equal(350.50m, loaded!.Amount);

            loaded.Amount = 400.00m;
            _expRepo.Update(loaded);

            var updatedList = _expRepo.GetByHike(hike.Id);
            var updated = updatedList.Find(e => e.Id == loaded.Id);
            Assert.NotNull(updated);
            Assert.Equal(400.00m, updated!.Amount);

            _expRepo.Delete(loaded.Id);
            var finalList = _expRepo.GetByHike(hike.Id);
            Assert.DoesNotContain(finalList, e => e.Id == loaded.Id);
        }

        [Fact]
        public void SqlInjectionAttempt_InParameters_DoesNotHarmDatabase()
        {
            string maliciousName = "'; DROP TABLE ROUTES;--";

            var route = TestDataFactory.CreateValidRoute(name: maliciousName);
            _routeRepo.Insert(route);

            var loaded = _routeRepo.GetById(route.Id);
            Assert.NotNull(loaded);
            Assert.Equal(maliciousName, loaded!.Name);

            var allRoutes = _routeRepo.GetAll();
            Assert.NotEmpty(allRoutes);
        }
    }
}
