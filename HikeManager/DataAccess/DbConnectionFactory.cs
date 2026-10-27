using System;
using System.Data.SQLite;
using System.IO;

namespace HikeManager.DataAccess
{
    public static class DbConnectionFactory
    {
        public static SQLiteConnection CreateOpenConnection()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hikemanager.db");
            var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }
            return connection;
        }
    }
}