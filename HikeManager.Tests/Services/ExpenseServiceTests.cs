using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class ExpenseServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly ExpenseService _service;
        private readonly int _validHikeId;

        public ExpenseServiceTests()
        {
            _db = new SqliteTestDatabase();
            var repo = new ExpenseRepository();
            _service = new ExpenseService(repo);

            var routeRepo = new RouteRepository();
            var route = TestDataFactory.CreateValidRoute();
            routeRepo.Insert(route);

            var hikeRepo = new HikeRepository();
            var hike = TestDataFactory.CreateValidHike(route.Id);
            hikeRepo.Insert(hike);
            _validHikeId = hike.Id;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Add_ValidExpense_SavesSuccessfully()
        {
            var expense = TestDataFactory.CreateValidExpense(_validHikeId, "Харчування", 450m);

            _service.Add(expense);

            var list = _service.GetByHike(_validHikeId);
            Assert.Contains(list, x => x.Category == "Харчування" && x.Amount == 450m);
        }

        [Fact]
        public void Add_InvalidCategory_ThrowsValidationException()
        {
            var expense = TestDataFactory.CreateValidExpense(_validHikeId, "Розваги", 200m);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(expense));
            Assert.Contains("категорію зі списку", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Add_ZeroOrNegativeAmount_ThrowsValidationException(decimal invalidAmount)
        {
            var expense = TestDataFactory.CreateValidExpense(_validHikeId, "Проїзд", invalidAmount);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(expense));
            Assert.Contains("більшою за нуль", ex.Message);
        }

        [Fact]
        public void Add_InvalidHikeId_ThrowsValidationException()
        {
            var expense = TestDataFactory.CreateValidExpense(0, "Харчування", 100m);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(expense));
            Assert.Contains("Не вибрано похід", ex.Message);
        }

        [Theory]
        [InlineData("150,50", 150.50)]
        [InlineData("150.50", 150.50)]
        [InlineData(" 1 200,75 ", 1200.75)]
        [InlineData("500", 500.0)]
        public void ParseAmount_ValidStrings_ParsesToDecimalCorrectly(string input, decimal expected)
        {
            decimal actual = ExpenseService.ParseAmount(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ParseAmount_EmptyInput_ThrowsValidationException(string? input)
        {
            var ex = Assert.Throws<ValidationException>(() => ExpenseService.ParseAmount(input!));
            Assert.Contains("Введіть суму", ex.Message);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.34.56")]
        [InlineData("100 грн")]
        public void ParseAmount_InvalidFormat_ThrowsValidationException(string input)
        {
            var ex = Assert.Throws<ValidationException>(() => ExpenseService.ParseAmount(input));
            Assert.Contains("Некоректна сума", ex.Message);
        }
    }
}

