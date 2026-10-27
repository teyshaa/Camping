using System;
using System.Data.SQLite;
using System.IO;

namespace HikeManager.DataAccess
{
    public static class DbConnectionFactory
    {
        private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "hikemanager.db");
        public static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static string DatabasePath => DbPath;

        public static SQLiteConnection CreateOpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // SQLite за замовчуванням вимикає перевірку зовнішніх ключів.
            // Виконуємо PRAGMA foreign_keys = ON, щоб працював ON DELETE CASCADE.
            using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }
    }
}
