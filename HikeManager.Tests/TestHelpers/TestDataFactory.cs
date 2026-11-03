using System;
using HikeManager.Models;

namespace HikeManager.Tests.TestHelpers
{
    public static class TestDataFactory
    {
        public static Route CreateValidRoute(
            string name = "Тестовий маршрут",
            string? location = "Карпати, с. Ясіня",
            string difficulty = "Середній",
            int durationDays = 3,
            double? lengthKm = 25.5,
            bool isDeleted = false)
        {
            return new Route
            {
                Name = name,
                Location = location,
                Difficulty = difficulty,
                DurationDays = durationDays,
                LengthKm = lengthKm,
                IsDeleted = isDeleted
            };
        }

        public static Hike CreateValidHike(
            int routeId,
            string name = "Осінній похід Чорногорою",
            DateTime? startDate = null,
            bool isDeleted = false,
            int durationDays = 3)
        {
            return new Hike
            {
                Name = name,
                StartDate = startDate ?? new DateTime(2026, 10, 15),
                RouteId = routeId,
                IsDeleted = isDeleted,
                DurationDays = durationDays
            };
        }

        public static Participant CreateValidParticipant(
            int hikeId,
            string fullName = "Тарас Шевченко",
            string? phone = "+380501234567",
            string? note = "Досвідчений турист")
        {
            return new Participant
            {
                HikeId = hikeId,
                FullName = fullName,
                Phone = phone,
                Note = note
            };
        }

        public static EquipmentItem CreateValidEquipmentItem(
            int hikeId,
            string name = "Намет туристичний 2-місний",
            int quantity = 2)
        {
            return new EquipmentItem
            {
                HikeId = hikeId,
                Name = name,
                Quantity = quantity
            };
        }

        public static Expense CreateValidExpense(
            int hikeId,
            string category = "Харчування",
            decimal amount = 850.50m,
            string? description = "Закупівля провіанту")
        {
            return new Expense
            {
                HikeId = hikeId,
                Category = category,
                Amount = amount,
                Description = description
            };
        }
    }
}

