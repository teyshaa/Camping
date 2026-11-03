using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class EquipmentServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly EquipmentService _service;
        private readonly int _validHikeId;

        public EquipmentServiceTests()
        {
            _db = new SqliteTestDatabase();
            var repo = new EquipmentRepository();
            _service = new EquipmentService(repo);

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
        public void Add_ValidItem_SavesSuccessfully()
        {
            var item = TestDataFactory.CreateValidEquipmentItem(_validHikeId, "Казанок 5л", 1);

            _service.Add(item);

            var list = _service.GetByHike(_validHikeId);
            Assert.Contains(list, x => x.Name == "Казанок 5л" && x.Quantity == 1);
        }

        [Fact]
        public void Add_DuplicateNameDifferentCase_SumsQuantities()
        {
            var item1 = TestDataFactory.CreateValidEquipmentItem(_validHikeId, "Термос 1л", 2);
            var item2 = TestDataFactory.CreateValidEquipmentItem(_validHikeId, "термос 1л", 3);

            _service.Add(item1);
            _service.Add(item2);

            var list = _service.GetByHike(_validHikeId);
            var thermoses = list.FindAll(x => string.Equals(x.Name, "Термос 1л", StringComparison.OrdinalIgnoreCase));

            Assert.Single(thermoses);
            Assert.Equal(5, thermoses[0].Quantity);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_EmptyOrWhitespaceName_ThrowsValidationException(string? invalidName)
        {
            var item = TestDataFactory.CreateValidEquipmentItem(_validHikeId, invalidName!);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(item));
            Assert.Contains("Назва спорядження є обов'язковою", ex.Message);
        }

        [Fact]
        public void Add_NameOver100Chars_ThrowsValidationException()
        {
            string longName = new string('E', 101);
            var item = TestDataFactory.CreateValidEquipmentItem(_validHikeId, longName);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(item));
            Assert.Contains("не може перевищувати 100 символів", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Add_QuantityLessThanOne_ThrowsValidationException(int qty)
        {
            var item = TestDataFactory.CreateValidEquipmentItem(_validHikeId, "Ліхтарик", qty);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(item));
            Assert.Contains("більшою або рівною 1", ex.Message);
        }

        [Fact]
        public void Add_QuantityOver1000_ThrowsValidationException()
        {
            var item = TestDataFactory.CreateValidEquipmentItem(_validHikeId, "Сірники", 1001);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(item));
            Assert.Contains("не може перевищувати 1000", ex.Message);
        }

        [Fact]
        public void AddBasicKit_WhenEmptyHike_AddsAllBasicKitItems()
        {
            int added = _service.AddBasicKit(_validHikeId);

            Assert.Equal(EquipmentService.DefaultBasicKit.Count, added);
            var list = _service.GetByHike(_validHikeId);
            Assert.Equal(EquipmentService.DefaultBasicKit.Count, list.Count);
        }

        [Fact]
        public void AddBasicKit_WhenSomeItemsAlreadyExist_AddsOnlyMissingItems()
        {
            _service.Add(new EquipmentItem { HikeId = _validHikeId, Name = "Намет туристичний", Quantity = 1 });

            int added = _service.AddBasicKit(_validHikeId);

            Assert.Equal(EquipmentService.DefaultBasicKit.Count - 1, added);
        }
    }
}
