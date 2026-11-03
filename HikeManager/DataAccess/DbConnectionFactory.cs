using System;
using System.Data.SQLite;
using System.IO;

namespace HikeManager.DataAccess
{
    public static class DbConnectionFactory
    {
        private static readonly string DefaultDbPath = Path.Combine(AppContext.BaseDirectory, "hikemanager.db");
        private static string? _customDbPath;
        private static string? _customConnectionString;

        public static string DatabasePath => _customDbPath ?? DefaultDbPath;
        public static string ConnectionString => _customConnectionString ?? $"Data Source={DatabasePath};Version=3;";

        public static void SetCustomDatabasePath(string path)
        {
            _customDbPath = path;
            _customConnectionString = $"Data Source={path};Version=3;";
        }

        public static void ResetToDefault()
        {
            _customDbPath = null;
            _customConnectionString = null;
        }

        public static SQLiteConnection CreateOpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }
    }
}
