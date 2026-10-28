using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HikeManager.DataAccess
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            try
            {
                using (var connection = DbConnectionFactory.CreateOpenConnection())
                {
                    // 1. Створення таблиць
                    string createTables = @"
                        CREATE TABLE IF NOT EXISTS ROUTES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Location TEXT,
                            Difficulty TEXT NOT NULL,
                            DurationDays INTEGER NOT NULL,
                            LengthKm REAL,
                            IsDeleted INTEGER NOT NULL DEFAULT 0
                        );

                        CREATE TABLE IF NOT EXISTS HIKES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            StartDate TEXT NOT NULL,
                            RouteId INTEGER NOT NULL,
                            IsDeleted INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY(RouteId) REFERENCES ROUTES(Id)
                        );

                        CREATE TABLE IF NOT EXISTS PARTICIPANTS (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            FullName TEXT NOT NULL,
                            Phone TEXT,
                            Note TEXT,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );

                        CREATE TABLE IF NOT EXISTS EQUIPMENT_ITEMS (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            Quantity INTEGER DEFAULT 1,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );

                        CREATE TABLE IF NOT EXISTS EXPENSES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            HikeId INTEGER NOT NULL,
                            Category TEXT NOT NULL,
                            Amount REAL NOT NULL,
                            Description TEXT,
                            FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
                        );

                        -- 2. Індекси для прискорення вибірки за HikeId
                        CREATE INDEX IF NOT EXISTS idx_participants_hikeid ON PARTICIPANTS(HikeId);
                        CREATE INDEX IF NOT EXISTS idx_equipment_hikeid ON EQUIPMENT_ITEMS(HikeId);
                        CREATE INDEX IF NOT EXISTS idx_expenses_hikeid ON EXPENSES(HikeId);
                    ";

                    ExecuteCommand(createTables, connection);

                    // Міграція для вже існуючих БД: додаємо стовпчик IsDeleted, якщо його ще немає
                    EnsureColumnExists(connection, "HIKES", "IsDeleted", "INTEGER NOT NULL DEFAULT 0");
                    EnsureColumnExists(connection, "ROUTES", "IsDeleted", "INTEGER NOT NULL DEFAULT 0");

                    // 3. Засівання демо-маршрутів, якщо таблиця ROUTES порожня
                    SeedDemoRoutesIfEmpty(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації бази даних: " + ex.Message, "Помилка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnsureColumnExists(SQLiteConnection connection, string table, string column, string columnDef)
        {
            try
            {
                using (var cmd = new SQLiteCommand($"PRAGMA table_info({table});", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
                            return; // Стовпчик уже існує
                    }
                }
                ExecuteCommand($"ALTER TABLE {table} ADD COLUMN {column} {columnDef};", connection);
            }
            catch
            {
                // Якщо помилка (наприклад, колонка вже є) — не ламаємо запуск
            }
        }

        private static void SeedDemoRoutesIfEmpty(SQLiteConnection connection)
        {
            using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM ROUTES;", connection))
            {
                long count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    string insertSeed = @"
                        INSERT INTO ROUTES (Name, Location, Difficulty, DurationDays, LengthKm) VALUES
                        ('Чорногірський хребет', 'Карпати, Ворохта', 'Складний', 3, 35.5),
                        ('Стежки Ґорґан', 'Івано-Франківська обл., Яремче', 'Середній', 2, 22.0),
                        ('Полонина Боржава', 'Закарпаття, Воловець', 'Легкий', 1, 12.0);
                    ";
                    ExecuteCommand(insertSeed, connection);
                }
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