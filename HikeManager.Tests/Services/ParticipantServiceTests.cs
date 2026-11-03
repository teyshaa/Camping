using System;
using HikeManager.DataAccess;
using HikeManager.Models;
using HikeManager.Services;
using HikeManager.Tests.TestHelpers;
using Xunit;

namespace HikeManager.Tests.Services
{
    [Collection("DatabaseCollection")]
    public class ParticipantServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db;
        private readonly ParticipantService _service;
        private readonly int _validHikeId;

        public ParticipantServiceTests()
        {
            _db = new SqliteTestDatabase();
            var participantRepo = new ParticipantRepository();
            _service = new ParticipantService(participantRepo);

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
        public void Add_ValidParticipant_SavesSuccessfully()
        {
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, "Олександр Коваленко", "+380671112233");

            _service.Add(p);

            var list = _service.GetByHike(_validHikeId);
            Assert.Contains(list, x => x.FullName == "Олександр Коваленко");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_EmptyOrWhitespaceFullName_ThrowsValidationException(string? invalidName)
        {
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, invalidName!);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(p));
            Assert.Contains("ПІБ учасника є обов'язковим", ex.Message);
        }

        [Fact]
        public void Add_FullNameOver100Chars_ThrowsValidationException()
        {
            string longName = new string('А', 101);
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, longName);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(p));
            Assert.Contains("не може перевищувати 100 символів", ex.Message);
        }

        [Theory]
        [InlineData("123456789")]
        [InlineData("+38093")]
        public void Add_PhoneWithLessThan10Digits_ThrowsValidationException(string shortPhone)
        {
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, "Тест", phone: shortPhone);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(p));
            Assert.Contains("від 10 до 15 цифр", ex.Message);
        }

        [Fact]
        public void Add_PhoneWithMoreThan15Digits_ThrowsValidationException()
        {
            string longPhone = "+38012345678901234";
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, "Тест", phone: longPhone);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(p));
            Assert.Contains("від 10 до 15 цифр", ex.Message);
        }

        [Theory]
        [InlineData("+3805012345abc")]
        [InlineData("+38050@123456")]
        [InlineData("phone#1234567890")]
        public void Add_PhoneWithInvalidCharacters_ThrowsValidationException(string invalidPhone)
        {
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, "Тест", phone: invalidPhone);

            var ex = Assert.Throws<ValidationException>(() => _service.Add(p));
            Assert.Contains("Некоректний формат телефону", ex.Message);
        }

        [Theory]
        [InlineData("+380 (50) 123-45-67")]
        [InlineData("0501234567")]
        [InlineData("+38-050-123-45-67")]
        public void Add_ValidPhoneFormats_Succeeds(string validPhone)
        {
            var p = TestDataFactory.CreateValidParticipant(_validHikeId, $"Учасник {validPhone}", phone: validPhone);

            _service.Add(p);

            var list = _service.GetByHike(_validHikeId);
            Assert.Contains(list, x => x.Phone == validPhone);
        }

        [Fact]
        public void CountByHike_ReturnsExactNumberOfParticipants()
        {
            int before = _service.CountByHike(_validHikeId);

            _service.Add(TestDataFactory.CreateValidParticipant(_validHikeId, "Учасник 1", "+380501111111"));
            _service.Add(TestDataFactory.CreateValidParticipant(_validHikeId, "Учасник 2", "+380502222222"));

            int after = _service.CountByHike(_validHikeId);
            Assert.Equal(before + 2, after);
        }
    }
}
