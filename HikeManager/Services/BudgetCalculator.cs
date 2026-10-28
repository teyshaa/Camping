
using System;
using System.Collections.Generic;
using System.Linq;
using HikeManager.Models;

namespace HikeManager.Services
{
    public class BudgetResult
    {
        public decimal TotalBudget { get; set; }
        public decimal CostPerPerson { get; set; }
        public bool HasNoParticipants { get; set; }
        public Dictionary<string, decimal> CategoryTotals { get; set; }
            = new Dictionary<string, decimal>();
    }

    public static class BudgetCalculator
    {
        public static BudgetResult Calculate(
            IEnumerable<Expense> expenses,
            int participantsCount)
        {
            if (expenses == null)
                throw new ArgumentNullException(nameof(expenses));

            if (participantsCount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(participantsCount));

            var expenseList = expenses.ToList();

            decimal total = expenseList.Sum(e => e.Amount);

            decimal perPerson = participantsCount > 0
                ? total / participantsCount
                : 0m;

            var categoryTotals = expenseList
                .GroupBy(e => e.Category)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(
                        g.Sum(e => e.Amount),
                        2,
                        MidpointRounding.AwayFromZero));

            return new BudgetResult
            {
                TotalBudget = Math.Round(
                    total, 2, MidpointRounding.AwayFromZero),

                CostPerPerson = Math.Round(
                    perPerson, 2, MidpointRounding.AwayFromZero),

                HasNoParticipants = participantsCount == 0,

                CategoryTotals = categoryTotals
            };
        }
    }
}
