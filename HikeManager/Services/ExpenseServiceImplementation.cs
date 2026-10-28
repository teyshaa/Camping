

using System;
using System.Collections.Generic;
using System.Globalization;
using HikeManager.DataAccess;
using HikeManager.Models;

namespace HikeManager.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseRepository repository = new ExpenseRepository();

        public static readonly string[] Categories =
        {
            "Проїзд",
            "Харчування",
            "Спорядження",
            "Страхування"
        };

        public List<Expense> GetByHike(int hikeId)
        {
            return repository.GetByHike(hikeId);
        }

        public void Add(Expense expense)
        {
            Insert(expense);
        }

        public void Insert(Expense expense)
        {
            Validate(expense);
            repository.Insert(expense);
        }

        public void Update(Expense expense)
        {
            if (expense.Id <= 0)
                throw new ValidationException("Некоректний ID витрати.");

            Validate(expense);
            repository.Update(expense);
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ValidationException("Некоректний ID витрати.");

            repository.Delete(id);
        }

        public static decimal ParseAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ValidationException("Введіть суму витрати.");

            string normalized = text.Trim()
                .Replace(" ", "")
                .Replace(',', '.');

            if (!decimal.TryParse(
                normalized,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal amount))
            {
                throw new ValidationException(
                    "Некоректна сума. Введіть число, наприклад 150,50.");
            }

            return amount;
        }

        private static void Validate(Expense expense)
        {
            if (expense.HikeId <= 0)
                throw new ValidationException("Не вибрано похід.");

            if (Array.IndexOf(Categories, expense.Category) < 0)
                throw new ValidationException(
                    "Виберіть категорію зі списку.");

            if (expense.Amount <= 0)
                throw new ValidationException(
                    "Сума витрати повинна бути більшою за нуль.");
        }
    }
}
