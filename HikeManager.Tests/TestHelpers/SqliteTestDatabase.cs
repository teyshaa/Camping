using System;
using System.Data.SQLite;
using System.IO;
using HikeManager.DataAccess;

namespace HikeManager.Tests.TestHelpers
{
    public class SqliteTestDatabase : IDisposable
    {
        public string DatabasePath { get; }

        public SqliteTestDatabase(bool initializeSchema = true)
        {
            DatabasePath = Path.Combine(Path.GetTempPath(), $"hikemanager_test_{Guid.NewGuid():N}.db");
            DbConnectionFactory.SetCustomDatabasePath(DatabasePath);

            if (initializeSchema)
            {
                DatabaseInitializer.Initialize();
            }
        }

        public SQLiteConnection OpenConnection()
        {
            return DbConnectionFactory.CreateOpenConnection();
        }

        public void Dispose()
        {
            try
            {
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                DbConnectionFactory.ResetToDefault();

                if (File.Exists(DatabasePath))
                {
                    File.Delete(DatabasePath);
                }
            }
            catch
            {
            }
        }
    }
}
