using System;
using System.Collections.Generic;
using System.Data.SQLite;
using HikeManager.DataAccess;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.DataAccess
{
    [Collection("DatabaseCollection")]
    public class DatabaseInitializerTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;

        public DatabaseInitializerTests()
        {
            _db = new SqliteTestDatabase(initializeSchema: false);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Initialize_CreatesAllFiveTables()
        {
            DatabaseInitializer.Initialize();

            var tables = new List<string>();
            using (var conn = _db.OpenConnection())
            using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            Assert.Contains("ROUTES", tables);
            Assert.Contains("HIKES", tables);
            Assert.Contains("PARTICIPANTS", tables);
            Assert.Contains("EQUIPMENT_ITEMS", tables);
            Assert.Contains("EXPENSES", tables);
        }

        [Fact]
        public void Initialize_CreatesExpectedIndexes()
        {
            DatabaseInitializer.Initialize();

            var indexes = new List<string>();
            using (var conn = _db.OpenConnection())
            using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='index' ORDER BY name;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    indexes.Add(reader.GetString(0));
                }
            }

            Assert.Contains("idx_participants_hikeid", indexes);
            Assert.Contains("idx_equipment_hikeid", indexes);
            Assert.Contains("idx_expenses_hikeid", indexes);
        }

        [Fact]
        public void Initialize_CalledMultipleTimes_IdempotentAndDoesNotThrow()
        {
            DatabaseInitializer.Initialize();
            var ex = Record.Exception(() => DatabaseInitializer.Initialize());
            Assert.Null(ex);
        }

        [Fact]
        public void Initialize_SeedsDemoData_WhenFresh()
        {
            DatabaseInitializer.Initialize();

            using var conn = _db.OpenConnection();
            using var cmdRoutes = new SQLiteCommand("SELECT COUNT(*) FROM ROUTES;", conn);
            long routeCount = (long)cmdRoutes.ExecuteScalar();
            Assert.True(routeCount >= 3);

            using var cmdHikes = new SQLiteCommand("SELECT COUNT(*) FROM HIKES;", conn);
            long hikeCount = (long)cmdHikes.ExecuteScalar();
            Assert.True(hikeCount >= 1);
        }
    }
}
