using System;
using System.Data.SQLite;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.DataAccess
{
    [Collection("DatabaseCollection")]
    public class CascadeDeleteIntegrationTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly RouteRepository _routeRepo;
        private readonly HikeRepository _hikeRepo;
        private readonly ParticipantRepository _partRepo;
        private readonly EquipmentRepository _equipRepo;
        private readonly ExpenseRepository _expRepo;

        public CascadeDeleteIntegrationTests()
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
        public void DeleteHike_CascadeDeletesParticipantsEquipmentAndExpenses()
        {
            var route = TestDataFactory.CreateValidRoute("Каскадний маршрут");
            _routeRepo.Insert(route);

            var hike = TestDataFactory.CreateValidHike(route.Id, "Похід для каскадного видалення");
            _hikeRepo.Insert(hike);
            int hikeId = hike.Id;

            _partRepo.Insert(TestDataFactory.CreateValidParticipant(hikeId, "Каскадний Учасник"));
            _equipRepo.Insert(TestDataFactory.CreateValidEquipmentItem(hikeId, "Каскадний Намет", 1));
            _expRepo.Insert(TestDataFactory.CreateValidExpense(hikeId, "Харчування", 500m));

            Assert.Single(_partRepo.GetByHike(hikeId));
            Assert.Single(_equipRepo.GetByHike(hikeId));
            Assert.Single(_expRepo.GetByHike(hikeId));

            _hikeRepo.Delete(hikeId);

            Assert.Empty(_partRepo.GetByHike(hikeId));
            Assert.Empty(_equipRepo.GetByHike(hikeId));
            Assert.Empty(_expRepo.GetByHike(hikeId));
        }

        [Fact]
        public void DeleteRoute_ReferencedByHike_IsRejectedByForeignKeyConstraint()
        {
            var route = TestDataFactory.CreateValidRoute("Захищений маршрут");
            _routeRepo.Insert(route);

            var hike = TestDataFactory.CreateValidHike(route.Id, "Похід на захищений маршрут");
            _hikeRepo.Insert(hike);

            var ex = Assert.Throws<SQLiteException>(() => _routeRepo.Delete(route.Id));
            Assert.Contains("FOREIGN KEY", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
