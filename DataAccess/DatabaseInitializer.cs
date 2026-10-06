using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace HikeManager.DataAccess
{
    public static class DatabaseInitializer
    {
        // Назва файлу бази даних
        private static readonly string DbFileName = "hikemanager.db";
        // Рядок підключення ADO.NET
        public static readonly string ConnectionString = $"Data Source={DbFileName};Version=3;";

        public static void Initialize()
        {
            try
            {
                // Якщо бази ще немає, створюємо її
                if (!File.Exists(DbFileName))
                {
                    SQLiteConnection.CreateFile(DbFileName);

                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();

                        // Таблиця Маршрути
                        string createRoutes = @"CREATE TABLE ROUTES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Location TEXT,
                            Difficulty TEXT NOT NULL,
                            DurationDays INTEGER NOT NULL,
                            LengthKm REAL
                        );";

                        // Таблиця Походи[cite: 33, 34]
                        string createHikes = @"CREATE TABLE HIKES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            StartDate TEXT NOT NULL,
                            RouteId INTEGER NOT NULL,
                            FOREIGN KEY(RouteId) REFERENCES ROUTES(Id)
                        );";

                        // Таблиця Учасники[cite: 33, 34]
                        string createParticipants = @"CREATE TABLE PARTICIPANTS (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            FullName TEXT NOT NULL,
                            Phone TEXT,
                            Note TEXT,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );";

                        // Таблиця Спорядження[cite: 33, 34]
                        string createEquipment = @"CREATE TABLE EQUIPMENT_ITEMS (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            Quantity INTEGER DEFAULT 1,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );";

                        // Таблиця Витрати[cite: 33, 34]
                        string createExpenses = @"CREATE TABLE EXPENSES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            Category TEXT NOT NULL,
                            Amount REAL NOT NULL,
                            Description TEXT,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );";

                        // Виконання SQL-запитів
                        ExecuteCommand(createRoutes, connection);
                        ExecuteCommand(createHikes, connection);
                        ExecuteCommand(createParticipants, connection);
                        ExecuteCommand(createEquipment, connection);
                        ExecuteCommand(createExpenses, connection);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при створенні бази даних: " + ex.Message);
            }
        }

        private static void ExecuteCommand(string query, SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}