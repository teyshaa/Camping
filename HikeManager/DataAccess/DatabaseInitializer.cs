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
                            LengthKm REAL
                        );

                        CREATE TABLE IF NOT EXISTS HIKES (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            StartDate TEXT NOT NULL,
                            RouteId INTEGER NOT NULL,
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

                    // 3. Засівання демо-маршрутів, якщо таблиця ROUTES порожня
                    SeedDemoRoutesIfEmpty(connection);

                    // 4. Засівання демо-походів, учасників та спорядження, якщо таблиця HIKES порожня
                    SeedDemoHikesIfEmpty(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації бази даних: " + ex.Message, "Помилка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static void SeedDemoHikesIfEmpty(SQLiteConnection connection)
        {
            using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM HIKES;", connection))
            {
                long count = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (count == 0)
                {
                    // Отримуємо реальні Id маршрутів із бази для запобігання помилки Foreign Key
                    var routeIds = new List<int>();
                    using (var routeCmd = new SQLiteCommand("SELECT Id FROM ROUTES ORDER BY Id ASC LIMIT 2;", connection))
                    using (var reader = routeCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            routeIds.Add(reader.GetInt32(0));
                        }
                    }

                    if (routeIds.Count > 0)
                    {
                        int routeId1 = routeIds[0];
                        string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                        using (var insertHike = new SQLiteCommand(
                            "INSERT INTO HIKES (Name, StartDate, RouteId) VALUES (@name, @date, @routeId);", connection))
                        {
                            insertHike.Parameters.AddWithValue("@name", "Осінній підйом на Чорногору");
                            insertHike.Parameters.AddWithValue("@date", todayStr);
                            insertHike.Parameters.AddWithValue("@routeId", routeId1);
                            insertHike.ExecuteNonQuery();
                        }

                        long hikeId = connection.LastInsertRowId;

                        // Якщо є другий маршрут, додамо й архівний похід
                        if (routeIds.Count > 1)
                        {
                            string pastStr = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
                            using (var insertPastHike = new SQLiteCommand(
                                "INSERT INTO HIKES (Name, StartDate, RouteId) VALUES (@name, @date, @routeId);", connection))
                            {
                                insertPastHike.Parameters.AddWithValue("@name", "Літній похід на Боржаву");
                                insertPastHike.Parameters.AddWithValue("@date", pastStr);
                                insertPastHike.Parameters.AddWithValue("@routeId", routeIds[1]);
                                insertPastHike.ExecuteNonQuery();
                            }
                        }

                        using (var cmd = new SQLiteCommand(connection))
                        {
                            cmd.CommandText = @"
                                INSERT INTO PARTICIPANTS (HikeId, FullName, Phone, Note) VALUES
                                (@hId, 'Христина Комар', '+380501234567', 'Керівник туристичної групи'),
                                (@hId, 'Анастасія Юркевич', '+380679876543', 'Штурман'),
                                (@hId, 'Анастасія Петричкович', '+380931112233', 'Відповідальна за аптечку'),
                                (@hId, 'Анастасія Решетівська', '+380503334455', 'Фінансист походу');

                                INSERT INTO EQUIPMENT_ITEMS (HikeId, Name, Quantity) VALUES
                                (@hId, 'Намет туристичний 3-місний', 2),
                                (@hId, 'Казанок армійський 5л', 1),
                                (@hId, 'Газовий пальник', 2),
                                (@hId, 'Аптечка групова', 1);

                                INSERT INTO EXPENSES (HikeId, Category, Amount, Description) VALUES
                                (@hId, 'Транспорт', 1200, 'Трансфер Івано-Франківськ — Ворохта'),
                                (@hId, 'Харчування', 1800, 'Продукти харчування на 3 дні'),
                                (@hId, 'Рекреаційний збір', 400, 'Вхід до Карпатського національного парку');
                            ";
                            cmd.Parameters.AddWithValue("@hId", hikeId);
                            cmd.ExecuteNonQuery();
                        }
                    }
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