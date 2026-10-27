using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class HikeRepository
    {
        // Єдиний формат дати, що використовується скрізь для запису і читання.
        private const string DateFormat = "yyyy-MM-dd";

        // ----------------------------------------------------------------
        // GetAll  — повертає всі походи з JOIN на ROUTES
        // ----------------------------------------------------------------
        public List<Hike> GetAll()
        {
            var hikes = new List<Hike>();

            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    SELECT
                        h.Id,
                        h.Name,
                        h.StartDate,
                        h.RouteId,
                        r.Name        AS RouteName,
                        r.DurationDays
                    FROM HIKES h
                    INNER JOIN ROUTES r ON r.Id = h.RouteId
                    ORDER BY h.Name ASC;";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        hikes.Add(MapRowToHike(reader));
                }
            }

            return hikes;
        }

        // ----------------------------------------------------------------
        // GetById — повертає один похід або null
        // ----------------------------------------------------------------
        public Hike? GetById(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    SELECT
                        h.Id,
                        h.Name,
                        h.StartDate,
                        h.RouteId,
                        r.Name        AS RouteName,
                        r.DurationDays
                    FROM HIKES h
                    INNER JOIN ROUTES r ON r.Id = h.RouteId
                    WHERE h.Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapRowToHike(reader);
                    }
                }
            }

            return null;
        }

        // ----------------------------------------------------------------
        // Insert — додає новий похід, встановлює Id у переданому об'єкті
        // ----------------------------------------------------------------
        public void Insert(Hike hike)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    INSERT INTO HIKES (Name, StartDate, RouteId)
                    VALUES (@Name, @StartDate, @RouteId);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", hike.Name);
                    cmd.Parameters.AddWithValue("@StartDate",
                        hike.StartDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@RouteId", hike.RouteId);

                    hike.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // ----------------------------------------------------------------
        // Update — оновлює існуючий запис
        // ----------------------------------------------------------------
        public void Update(Hike hike)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    UPDATE HIKES
                    SET Name      = @Name,
                        StartDate = @StartDate,
                        RouteId   = @RouteId
                    WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", hike.Id);
                    cmd.Parameters.AddWithValue("@Name", hike.Name);
                    cmd.Parameters.AddWithValue("@StartDate",
                        hike.StartDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@RouteId", hike.RouteId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Delete — видаляє похід за Id.
        //          Завдяки ON DELETE CASCADE у схемі БД (Учасник 1)
        //          автоматично видаляються учасники, спорядження та витрати.
        // ----------------------------------------------------------------
        public void Delete(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = "DELETE FROM HIKES WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------------------------------------------------------------
        // Приватний маппер рядка → Hike
        // ----------------------------------------------------------------
        private static Hike MapRowToHike(SQLiteDataReader reader)
        {
            return new Hike
            {
                Id           = Convert.ToInt32(reader["Id"]),
                Name         = reader["Name"]?.ToString() ?? string.Empty,
                StartDate    = DateTime.ParseExact(
                                   reader["StartDate"].ToString()!,
                                   DateFormat,
                                   CultureInfo.InvariantCulture),
                RouteId      = Convert.ToInt32(reader["RouteId"]),
                RouteName    = reader["RouteName"]?.ToString(),
                DurationDays = Convert.ToInt32(reader["DurationDays"])
            };
        }
    }
}
