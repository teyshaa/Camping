using System;
using System.Collections.Generic;
using System.Data.SQLite;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class RouteRepository
    {
        public List<Route> GetAll()
        {
            var routes = new List<Route>();

            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = "SELECT Id, Name, Location, Difficulty, DurationDays, LengthKm FROM ROUTES ORDER BY Name ASC;";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        routes.Add(MapRowToRoute(reader));
                    }
                }
            }

            return routes;
        }

        public Route? GetById(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = "SELECT Id, Name, Location, Difficulty, DurationDays, LengthKm FROM ROUTES WHERE Id = @Id;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRowToRoute(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void Insert(Route route)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = @"
                    INSERT INTO ROUTES (Name, Location, Difficulty, DurationDays, LengthKm)
                    VALUES (@Name, @Location, @Difficulty, @DurationDays, @LengthKm);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", route.Name);
                    command.Parameters.AddWithValue("@Location", (object?)route.Location ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Difficulty", route.Difficulty);
                    command.Parameters.AddWithValue("@DurationDays", route.DurationDays);
                    command.Parameters.AddWithValue("@LengthKm", (object?)route.LengthKm ?? DBNull.Value);

                    route.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Update(Route route)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = @"
                    UPDATE ROUTES
                    SET Name = @Name,
                        Location = @Location,
                        Difficulty = @Difficulty,
                        DurationDays = @DurationDays,
                        LengthKm = @LengthKm
                    WHERE Id = @Id;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", route.Id);
                    command.Parameters.AddWithValue("@Name", route.Name);
                    command.Parameters.AddWithValue("@Location", (object?)route.Location ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Difficulty", route.Difficulty);
                    command.Parameters.AddWithValue("@DurationDays", route.DurationDays);
                    command.Parameters.AddWithValue("@LengthKm", (object?)route.LengthKm ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = "DELETE FROM ROUTES WHERE Id = @Id;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int CountHikesUsingRoute(int routeId)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string sql = "SELECT COUNT(*) FROM HIKES WHERE RouteId = @RouteId;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RouteId", routeId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private static Route MapRowToRoute(SQLiteDataReader reader)
        {
            return new Route
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Location = reader["Location"] == DBNull.Value ? null : reader["Location"]?.ToString(),
                Difficulty = reader["Difficulty"]?.ToString() ?? "Легкий",
                DurationDays = Convert.ToInt32(reader["DurationDays"]),
                LengthKm = reader["LengthKm"] == DBNull.Value ? null : Convert.ToDouble(reader["LengthKm"])
            };
        }
    }
}
