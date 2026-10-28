using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class HikeRepository
    {
        private const string DateFormat = "yyyy-MM-dd";

        // GetAll — повертає всі походи з LEFT JOIN на ROUTES
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
                        h.IsDeleted,
                        COALESCE(r.Name, '(Маршрут видалено)') AS RouteName,
                        COALESCE(r.DurationDays, 1)            AS DurationDays
                    FROM HIKES h
                    LEFT JOIN ROUTES r ON r.Id = h.RouteId
                    ORDER BY h.StartDate ASC, h.Name ASC;";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        hikes.Add(MapRowToHike(reader));
                }
            }

            return hikes;
        }

        // GetById — повертає один похід або null
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
                        h.IsDeleted,
                        COALESCE(r.Name, '(Маршрут видалено)') AS RouteName,
                        COALESCE(r.DurationDays, 1)            AS DurationDays
                    FROM HIKES h
                    LEFT JOIN ROUTES r ON r.Id = h.RouteId
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

        // Insert — додає новий похід, встановлює Id у переданому об'єкті
        public void Insert(Hike hike)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    INSERT INTO HIKES (Name, StartDate, RouteId, IsDeleted)
                    VALUES (@Name, @StartDate, @RouteId, @IsDeleted);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", hike.Name);
                    cmd.Parameters.AddWithValue("@StartDate",
                        hike.StartDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@RouteId", hike.RouteId);
                    cmd.Parameters.AddWithValue("@IsDeleted", hike.IsDeleted ? 1 : 0);

                    hike.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Update — оновлює існуючий запис
        public void Update(Hike hike)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = @"
                    UPDATE HIKES
                    SET Name      = @Name,
                        StartDate = @StartDate,
                        RouteId   = @RouteId,
                        IsDeleted = @IsDeleted
                    WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", hike.Id);
                    cmd.Parameters.AddWithValue("@Name", hike.Name);
                    cmd.Parameters.AddWithValue("@StartDate",
                        hike.StartDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@RouteId", hike.RouteId);
                    cmd.Parameters.AddWithValue("@IsDeleted", hike.IsDeleted ? 1 : 0);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // SoftDelete — позначає похід як видалений (переносить в історію)
        public void SoftDelete(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = "UPDATE HIKES SET IsDeleted = 1 WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Restore — відновлює похід з історії у активні
        public void Restore(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                const string sql = "UPDATE HIKES SET IsDeleted = 0 WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete — остаточно видаляє похід за Id з БД
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

        // Приватний маппер рядка → Hike
        private static Hike MapRowToHike(SQLiteDataReader reader)
        {
            bool isDeleted = false;
            if (reader["IsDeleted"] != DBNull.Value)
            {
                isDeleted = Convert.ToInt32(reader["IsDeleted"]) == 1;
            }

            return new Hike
            {
                Id           = Convert.ToInt32(reader["Id"]),
                Name         = reader["Name"]?.ToString() ?? string.Empty,
                StartDate    = DateTime.ParseExact(
                                   reader["StartDate"].ToString()!,
                                   DateFormat,
                                   CultureInfo.InvariantCulture),
                RouteId      = Convert.ToInt32(reader["RouteId"]),
                IsDeleted    = isDeleted,
                RouteName    = reader["RouteName"]?.ToString(),
                DurationDays = reader["DurationDays"] != DBNull.Value ? Convert.ToInt32(reader["DurationDays"]) : 1
            };
        }
    }
}
