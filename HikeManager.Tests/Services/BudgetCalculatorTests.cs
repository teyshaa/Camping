using System;
using System.Collections.Generic;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    public class BudgetCalculatorTests
    {
        [Fact]
        public void Calculate_NullExpenses_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BudgetCalculator.Calculate(null!, 3));
        }

        [Fact]
        public void Calculate_NegativeParticipants_ThrowsArgumentOutOfRangeException()
        {
            var expenses = new List<Expense>();
            Assert.Throws<ArgumentOutOfRangeException>(() => BudgetCalculator.Calculate(expenses, -1));
        }

        [Fact]
        public void Calculate_EmptyExpenses_ReturnsZeroBudgetAndZeroCostPerPerson()
        {
            var expenses = new List<Expense>();

            var result = BudgetCalculator.Calculate(expenses, 4);

            Assert.Equal(0m, result.TotalBudget);
            Assert.Equal(0m, result.CostPerPerson);
            Assert.False(result.HasNoParticipants);
            Assert.Empty(result.CategoryTotals);
        }

        [Fact]
        public void Calculate_ZeroParticipants_AvoidsDivisionByZeroAndSetsFlag()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Харчування", Amount = 1500m },
                new() { Category = "Проїзд", Amount = 600m }
            };

            var result = BudgetCalculator.Calculate(expenses, 0);

            Assert.Equal(2100m, result.TotalBudget);
            Assert.Equal(0m, result.CostPerPerson);
            Assert.True(result.HasNoParticipants);
        }

        [Fact]
        public void Calculate_SingleParticipant_CostPerPersonEqualsTotalBudget()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Спорядження", Amount = 1250.75m }
            };

            var result = BudgetCalculator.Calculate(expenses, 1);

            Assert.Equal(1250.75m, result.TotalBudget);
            Assert.Equal(1250.75m, result.CostPerPerson);
            Assert.False(result.HasNoParticipants);
        }

        [Fact]
        public void Calculate_OneHundredDividedByThree_RoundsToThirtyThreePointThirtyThree()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Харчування", Amount = 100m }
            };

            var result = BudgetCalculator.Calculate(expenses, 3);

            Assert.Equal(100m, result.TotalBudget);
            Assert.Equal(33.33m, result.CostPerPerson);
        }

        [Fact]
        public void Calculate_MultipleCategories_CalculatesCategoryTotalsCorrectly()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Харчування", Amount = 400.25m },
                new() { Category = "Харчування", Amount = 200.75m },
                new() { Category = "Проїзд", Amount = 500m },
                new() { Category = "Страхування", Amount = 150m }
            };

            var result = BudgetCalculator.Calculate(expenses, 2);

            Assert.Equal(1251.00m, result.TotalBudget);
            Assert.Equal(625.50m, result.CostPerPerson);
            Assert.Equal(3, result.CategoryTotals.Count);
            Assert.Equal(601.00m, result.CategoryTotals["Харчування"]);
            Assert.Equal(500.00m, result.CategoryTotals["Проїзд"]);
            Assert.Equal(150.00m, result.CategoryTotals["Страхування"]);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Calculate_LargeAmounts_ComputesWithoutPrecisionLoss()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Транспорт", Amount = 1_000_000_000.45m },
                new() { Category = "Спорядження", Amount = 2_000_000_000.54m }
            };

            var result = BudgetCalculator.Calculate(expenses, 3);

            Assert.Equal(3_000_000_000.99m, result.TotalBudget);
            Assert.Equal(1_000_000_000.33m, result.CostPerPerson);
        }
    }
}

