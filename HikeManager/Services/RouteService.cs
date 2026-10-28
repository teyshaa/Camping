using System;
using System.Collections.Generic;
using HikeManager.DataAccess;
using HikeManager.Models;

namespace HikeManager.Services
{
    public class RouteService : IRouteService
    {
        private readonly RouteRepository _repository;
        private static readonly HashSet<string> AllowedDifficulties = new(StringComparer.OrdinalIgnoreCase)
        {
            "Легкий",
            "Середній",
            "Складний"
        };

        public RouteService(RouteRepository? repository = null)
        {
            _repository = repository ?? new RouteRepository();
        }

        public List<Route> GetAll()
        {
            return _repository.GetAll().FindAll(r => !r.IsDeleted);
        }

        public List<Route> GetHistory()
        {
            return _repository.GetAll().FindAll(r => r.IsDeleted);
        }

        public Route? GetById(int id)
        {
            if (id <= 0) return null;
            return _repository.GetById(id);
        }

        public void Add(Route route)
        {
            Validate(route);
            _repository.Insert(route);
        }

        public void Update(Route route)
        {
            if (route.Id <= 0)
            {
                throw new ValidationException("Некоректний ідентифікатор маршруту для оновлення.");
            }

            Validate(route);

            var existing = _repository.GetById(route.Id);
            if (existing == null)
            {
                throw new ValidationException("Маршрут із вказаним ID не знайдено.");
            }

            _repository.Update(route);
        }

        public void Delete(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Некоректний ідентифікатор маршруту.");
            }

            var existing = _repository.GetById(id);
            if (existing == null)
            {
                throw new ValidationException("Маршрут не знайдено.");
            }

            // М'яке видалення: переносимо в історію
            _repository.SoftDelete(id);
        }

        public void Restore(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Некоректний ідентифікатор маршруту.");
            }

            var existing = _repository.GetById(id);
            if (existing == null)
            {
                throw new ValidationException("Маршрут не знайдено.");
            }

            _repository.Restore(id);
        }

        public void HardDelete(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Некоректний ідентифікатор маршруту.");
            }

            var existing = _repository.GetById(id);
            if (existing == null)
            {
                throw new ValidationException("Маршрут не знайдено.");
            }

            int countHikes = _repository.CountHikesUsingRoute(id);
            if (countHikes > 0)
            {
                throw new ValidationException($"Неможливо остаточно видалити маршрут «{existing.Name}», оскільки він використовується у {countHikes} активному поході(ах).");
            }

            _repository.Delete(id);
        }

        private static void Validate(Route route)
        {
            if (route == null)
            {
                throw new ValidationException("Дані маршруту не можуть бути порожніми.");
            }

            if (string.IsNullOrWhiteSpace(route.Name))
            {
                throw new ValidationException("Назва маршруту є обов'язковою для заповнення.");
            }

            if (string.IsNullOrWhiteSpace(route.Difficulty) || !AllowedDifficulties.Contains(route.Difficulty.Trim()))
            {
                throw new ValidationException("Складність повинна бути однією з наступних: «Легкий», «Середній», «Складний».");
            }

            if (route.DurationDays < 1)
            {
                throw new ValidationException("Тривалість маршруту повинна бути не менше 1 дня.");
            }

            if (route.LengthKm.HasValue && route.LengthKm.Value < 0)
            {
                throw new ValidationException("Довжина маршруту не може бути від'ємною.");
            }
        }
    }
}
