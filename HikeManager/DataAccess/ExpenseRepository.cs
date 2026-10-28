
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using HikeManager.Models;

namespace HikeManager.DataAccess
{
    public class ExpenseRepository
    {
        public List<Expense> GetByHike(int hikeId)
        {
            var expenses = new List<Expense>();

            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var command = new SQLiteCommand(
                "SELECT Id, HikeId, Category, Amount, Description " +
                "FROM EXPENSES WHERE HikeId = @hikeId ORDER BY Id",
                connection);

            command.Parameters.AddWithValue("@hikeId", hikeId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                expenses.Add(new Expense
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    HikeId = Convert.ToInt32(reader["HikeId"]),
                    Category = Convert.ToString(reader["Category"]) ?? "",
                    Amount = Convert.ToDecimal(
                        reader["Amount"], CultureInfo.InvariantCulture),
                    Description = reader["Description"] == DBNull.Value
                        ? null
                        : Convert.ToString(reader["Description"])
                });
            }

            return expenses;
        }

        public void Insert(Expense expense)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var command = new SQLiteCommand(
                "INSERT INTO EXPENSES (HikeId, Category, Amount, Description) " +
                "VALUES (@hikeId, @category, @amount, @description)",
                connection);

            AddParameters(command, expense);
            command.ExecuteNonQuery();
        }

        public void Update(Expense expense)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var command = new SQLiteCommand(
                "UPDATE EXPENSES SET Category = @category, " +
                "Amount = @amount, Description = @description " +
                "WHERE Id = @id AND HikeId = @hikeId",
                connection);

            AddParameters(command, expense);
            command.Parameters.AddWithValue("@id", expense.Id);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var command = new SQLiteCommand(
                "DELETE FROM EXPENSES WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(SQLiteCommand command, Expense expense)
        {
            command.Parameters.AddWithValue("@hikeId", expense.HikeId);
            command.Parameters.AddWithValue("@category", expense.Category);
            command.Parameters.AddWithValue(
                "@amount", Convert.ToDouble(expense.Amount));
            command.Parameters.AddWithValue(
                "@description",
                (object?)expense.Description ?? DBNull.Value);
        }
    }
}

