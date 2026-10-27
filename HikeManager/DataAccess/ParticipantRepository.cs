using System;
using System.Collections.Generic;
using System.Data.SQLite;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class ParticipantRepository
    {
        public List<Participant> GetByHike(int hikeId)
        {
            var list = new List<Participant>();
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "SELECT Id, HikeId, FullName, Phone, Note FROM PARTICIPANTS WHERE HikeId = @hikeId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hikeId", hikeId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Participant
                            {
                                Id = reader.GetInt32(0),
                                HikeId = reader.GetInt32(1),
                                FullName = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Note = reader.IsDBNull(4) ? null : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Insert(Participant p)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "INSERT INTO PARTICIPANTS (HikeId, FullName, Phone, Note) VALUES (@hikeId, @fullName, @phone, @note)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hikeId", p.HikeId);
                    command.Parameters.AddWithValue("@fullName", p.FullName);
                    command.Parameters.AddWithValue("@phone", (object)p.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@note", (object)p.Note ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Participant p)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "UPDATE PARTICIPANTS SET FullName = @fullName, Phone = @phone, Note = @note WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fullName", p.FullName);
                    command.Parameters.AddWithValue("@phone", (object)p.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@note", (object)p.Note ?? DBNull.Value);
                    command.Parameters.AddWithValue("@id", p.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "DELETE FROM PARTICIPANTS WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int CountByHike(int hikeId)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "SELECT COUNT(*) FROM PARTICIPANTS WHERE HikeId = @hikeId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hikeId", hikeId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}