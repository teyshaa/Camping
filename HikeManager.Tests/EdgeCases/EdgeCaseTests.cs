using System;
using System.Collections.Generic;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.EdgeCases
{
    [Collection("DatabaseCollection")]
    public class EdgeCaseTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly RouteService _routeService;
        private readonly HikeService _hikeService;
        private readonly ParticipantService _partService;
        private readonly EquipmentService _equipService;
        private readonly ExpenseService _expenseService;

        public EdgeCaseTests()
        {
            _db = new SqliteTestDatabase();
            var routeRepo = new RouteRepository();
            var hikeRepo = new HikeRepository();
            var partRepo = new ParticipantRepository();
            var equipRepo = new EquipmentRepository();
            var expRepo = new ExpenseRepository();

            _routeService = new RouteService(routeRepo);
            _hikeService = new HikeService(hikeRepo, _routeService);
            _partService = new ParticipantService(partRepo);
            _equipService = new EquipmentService(equipRepo);
            _expenseService = new ExpenseService(expRepo);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        [Trait("Category", "EdgeCase")]
        public void Edge01_RouteName_EmptyOrWhitespaceString_ThrowsValidationException(string whitespaceName)
        {
            var route = TestDataFactory.CreateValidRoute(name: whitespaceName);
            Assert.Throws<ValidationException>(() => _routeService.Add(route));
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge02_ParticipantName_ExtremelyLongStringOver1000Chars_ThrowsValidationException()
        {
            string hugeName = new string('К', 1050);
            var participant = new Participant { HikeId = 1, FullName = hugeName };
            Assert.Throws<ValidationException>(() => _partService.Add(participant));
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(int.MaxValue, true)]
        [Trait("Category", "EdgeCase")]
        public void Edge03_RouteDurationDays_BoundaryValuesZeroOneAndMaxInt(int duration, bool expectedValid)
        {
            var route = TestDataFactory.CreateValidRoute(durationDays: duration);

            if (expectedValid)
            {
                _routeService.Add(route);
                Assert.True(route.Id > 0);
            }
            else
            {
                Assert.Throws<ValidationException>(() => _routeService.Add(route));
            }
        }

        [Theory]
        [InlineData(0.0, true)]
        [InlineData(null, true)]
        [InlineData(-0.01, false)]
        [InlineData(-100.0, false)]
        [Trait("Category", "EdgeCase")]
        public void Edge04_RouteLengthKm_ZeroNullAndNegativeValues(double? length, bool expectedValid)
        {
            var route = TestDataFactory.CreateValidRoute(lengthKm: length);

            if (expectedValid)
            {
                _routeService.Add(route);
                Assert.True(route.Id > 0);
            }
            else
            {
                Assert.Throws<ValidationException>(() => _routeService.Add(route));
            }
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge05_HikeDate_LeapYearFebruary29_AcceptedAndStoredAccurately()
        {
            var route = TestDataFactory.CreateValidRoute("Високосний маршрут");
            _routeService.Add(route);

            var leapDate = new DateTime(2028, 2, 29);
            var hike = TestDataFactory.CreateValidHike(route.Id, "Похід 29 лютого", leapDate);
            _hikeService.Create(hike);

            var saved = _hikeService.GetById(hike.Id);
            Assert.NotNull(saved);
            Assert.Equal(leapDate, saved!.StartDate);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge06_BudgetCalculator_ZeroParticipants_ReturnsZeroCostPerPersonWithoutDivisionByZero()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Харчування", Amount = 500m }
            };

            var result = BudgetCalculator.Calculate(expenses, participantsCount: 0);

            Assert.Equal(500m, result.TotalBudget);
            Assert.Equal(0m, result.CostPerPerson);
            Assert.True(result.HasNoParticipants);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge07_BudgetCalculator_EmptyExpensesAndEmptyParticipants_HandlesGracefully()
        {
            var result = BudgetCalculator.Calculate(new List<Expense>(), 0);

            Assert.Equal(0m, result.TotalBudget);
            Assert.Equal(0m, result.CostPerPerson);
            Assert.True(result.HasNoParticipants);
            Assert.Empty(result.CategoryTotals);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge08_BudgetCalculator_OneHundredDividedByThree_PrecisionTwoDecimalsRoundingAwayFromZero()
        {
            var expenses = new List<Expense>
            {
                new() { Category = "Проїзд", Amount = 100.00m }
            };

            var result = BudgetCalculator.Calculate(expenses, 3);

            Assert.Equal(33.33m, result.CostPerPerson);
        }

        [Theory]
        [InlineData("0,001", 0.001)]
        [InlineData("1 250,50", 1250.50)]
        [InlineData("0.05", 0.05)]
        [Trait("Category", "EdgeCase")]
        public void Edge09_Expense_AmountWithCommaAndFractions_ParsedCorrectly(string input, decimal expected)
        {
            decimal parsed = ExpenseService.ParseAmount(input);
            Assert.Equal(expected, parsed);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-500)]
        [Trait("Category", "EdgeCase")]
        public void Edge10_Expense_ZeroOrNegativeAmount_ThrowsValidationException(decimal invalidAmount)
        {
            var expense = TestDataFactory.CreateValidExpense(1, "Харчування", invalidAmount);
            Assert.Throws<ValidationException>(() => _expenseService.Add(expense));
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge11_Equipment_CaseInsensitiveDuplicates_SumsQuantities()
        {
            var route = TestDataFactory.CreateValidRoute();
            _routeService.Add(route);
            var hike = TestDataFactory.CreateValidHike(route.Id);
            _hikeService.Create(hike);

            _equipService.Add(new EquipmentItem { HikeId = hike.Id, Name = "ЛІХТАРИК", Quantity = 1 });
            _equipService.Add(new EquipmentItem { HikeId = hike.Id, Name = "ліхтарик", Quantity = 4 });

            var items = _equipService.GetByHike(hike.Id);
            var matching = items.FindAll(i => string.Equals(i.Name, "ліхтарик", StringComparison.OrdinalIgnoreCase));

            Assert.Single(matching);
            Assert.Equal(5, matching[0].Quantity);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1000, true)]
        [InlineData(0, false)]
        [InlineData(1001, false)]
        [Trait("Category", "EdgeCase")]
        public void Edge12_EquipmentQuantity_BoundariesOneAndThousand(int qty, bool expectedValid)
        {
            var item = new EquipmentItem { HikeId = 1, Name = "Компас", Quantity = qty };

            if (expectedValid)
            {
                var ex = Record.Exception(() => _equipService.Add(item));
                Assert.Null(ex);
            }
            else
            {
                Assert.Throws<ValidationException>(() => _equipService.Add(item));
            }
        }

        [Theory]
        [InlineData("123456789", false)]
        [InlineData("1234567890", true)]
        [InlineData("123456789012345", true)]
        [InlineData("1234567890123456", false)]
        [Trait("Category", "EdgeCase")]
        public void Edge13_ParticipantPhone_DigitCountExactBoundaries(string phone, bool expectedValid)
        {
            var participant = new Participant
            {
                HikeId = 1,
                FullName = "Перевірка Телефону",
                Phone = phone
            };

            if (expectedValid)
            {
                var ex = Record.Exception(() => _partService.Add(participant));
                Assert.Null(ex);
            }
            else
            {
                Assert.Throws<ValidationException>(() => _partService.Add(participant));
            }
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void Edge14_SqlInjectionSafety_StoredAsLiteralTextWithoutExecuting()
        {
            string payload = "Robert'); DROP TABLE EXPENSES;--";
            var participant = new Participant
            {
                HikeId = 1,
                FullName = payload,
                Phone = "+380509998877"
            };

            _partService.Add(participant);

            var list = _partService.GetByHike(1);
            Assert.Contains(list, p => p.FullName == payload);

            var expRepo = new ExpenseRepository();
            var ex = Record.Exception(() => expRepo.GetByHike(1));
            Assert.Null(ex);
        }
    }
}

