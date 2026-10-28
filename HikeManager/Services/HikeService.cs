using System;
using System.Collections.Generic;
using System.Linq;
using HikeManager.DataAccess;
using HikeManager.Models;

namespace HikeManager.Services
{
    /// <summary>
    /// Сервіс бізнес-логіки для сутності «Похід».
    /// Залежить від HikeRepository (доступ до БД) та IRouteService (валідація маршруту).
    /// </summary>
    public class HikeService : IHikeService
    {
        private readonly HikeRepository _hikeRepository;
        private readonly IRouteService  _routeService;

        public HikeService(HikeRepository? hikeRepository = null,
                           IRouteService?  routeService   = null)
        {
            _hikeRepository = hikeRepository ?? new HikeRepository();
            _routeService   = routeService   ?? new RouteService();
        }

        /// <summary>
        /// Активні походи: EndDate >= сьогодні. Сортування за зростанням дати початку.
        /// </summary>
        public List<Hike> GetActive()
        {
            var today = DateTime.Today;
            return _hikeRepository
                .GetAll()
                .Where(h => h.EndDate >= today)
                .OrderBy(h => h.StartDate)
                .ToList();
        }

        /// <summary>
        /// Архів: EndDate < сьогодні. Сортування за спаданням (найновіші першими).
        /// </summary>
        public List<Hike> GetHistory()
        {
            var today = DateTime.Today;
            return _hikeRepository
                .GetAll()
                .Where(h => h.EndDate < today)
                .OrderByDescending(h => h.StartDate)
                .ToList();
        }

        /// <summary>Повертає похід за Id або null.</summary>
        public Hike? GetById(int id)
        {
            if (id <= 0) return null;
            return _hikeRepository.GetById(id);
        }

        /// <summary>Валідує та створює новий похід.</summary>
        public void Create(Hike hike)
        {
            Validate(hike);
            _hikeRepository.Insert(hike);
        }

        /// <summary>Валідує та оновлює існуючий похід.</summary>
        public void Update(Hike hike)
        {
            if (hike.Id <= 0)
                throw new ValidationException("Некоректний ідентифікатор походу для оновлення.");

            if (_hikeRepository.GetById(hike.Id) == null)
                throw new ValidationException("Похід із вказаним ID не знайдено.");

            Validate(hike);
            _hikeRepository.Update(hike);
        }

        /// <summary>Видаляє похід за Id (каскад на учасників/спорядження/витрати).</summary>
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ValidationException("Некоректний ідентифікатор походу.");

            if (_hikeRepository.GetById(id) == null)
                throw new ValidationException("Похід не знайдено.");

            _hikeRepository.Delete(id);
        }

        private void Validate(Hike hike)
        {
            if (hike == null)
                throw new ValidationException("Дані походу не можуть бути порожніми.");

            // 1. Назва є обов'язковою
            if (string.IsNullOrWhiteSpace(hike.Name))
                throw new ValidationException("Назва походу є обов'язковою для заповнення.");

            // 2. Дата не може бути «пустим» значенням DateTime
            if (hike.StartDate == default)
                throw new ValidationException("Дата початку походу є обов'язковою для заповнення.");

            // 3. Маршрут повинен бути обраний і реально існувати в БД
            if (hike.RouteId <= 0)
                throw new ValidationException("Необхідно обрати маршрут для походу.");

            if (_routeService.GetById(hike.RouteId) == null)
                throw new ValidationException(
                    $"Маршрут з ID {hike.RouteId} не знайдено. Оберіть коректний маршрут.");
        }
    }
}
