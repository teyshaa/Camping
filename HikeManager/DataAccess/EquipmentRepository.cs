using System;
using System.Collections.Generic;
using System.Data.SQLite;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class EquipmentRepository
    {
        public List<EquipmentItem> GetByHike(int hikeId)
        {
            var list = new List<EquipmentItem>();
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "SELECT Id, HikeId, Name, Quantity FROM EQUIPMENT_ITEMS WHERE HikeId = @hikeId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hikeId", hikeId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EquipmentItem
                            {
                                Id = reader.GetInt32(0),
                                HikeId = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Quantity = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Insert(EquipmentItem item)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "INSERT INTO EQUIPMENT_ITEMS (HikeId, Name, Quantity) VALUES (@hikeId, @name, @quantity)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hikeId", item.HikeId);
                    command.Parameters.AddWithValue("@name", item.Name);
                    command.Parameters.AddWithValue("@quantity", item.Quantity);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(EquipmentItem item)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "UPDATE EQUIPMENT_ITEMS SET Name = @name, Quantity = @quantity WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", item.Name);
                    command.Parameters.AddWithValue("@quantity", item.Quantity);
                    command.Parameters.AddWithValue("@id", item.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = DbConnectionFactory.CreateOpenConnection())
            {
                string query = "DELETE FROM EQUIPMENT_ITEMS WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}