using System;
using System.Collections.Generic;
using System.Linq;
using HikeManager.DataAccess;
using HikeManager.Models;

namespace HikeManager.Services
{
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

        public List<Hike> GetActive()
        {
            return _hikeRepository
                .GetAll()
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.StartDate)
                .ToList();
        }

        public List<Hike> GetHistory()
        {
            return _hikeRepository
                .GetAll()
                .Where(h => h.IsDeleted)
                .OrderByDescending(h => h.StartDate)
                .ToList();
        }

        public Hike? GetById(int id)
        {
            if (id <= 0) return null;
            return _hikeRepository.GetById(id);
        }

        public void Create(Hike hike)
        {
            Validate(hike);
            _hikeRepository.Insert(hike);
        }

        public void Update(Hike hike)
        {
            if (hike.Id <= 0)
                throw new ValidationException("Некоректний ідентифікатор походу для оновлення.");

            if (_hikeRepository.GetById(hike.Id) == null)
                throw new ValidationException("Похід із вказаним ID не знайдено.");

            Validate(hike);
            _hikeRepository.Update(hike);
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ValidationException("Некоректний ідентифікатор походу.");

            if (_hikeRepository.GetById(id) == null)
                throw new ValidationException("Похід не знайдено.");

            _hikeRepository.SoftDelete(id);
        }

        public void RestoreHike(int id)
        {
            if (id <= 0)
                throw new ValidationException("Некоректний ідентифікатор походу.");

            if (_hikeRepository.GetById(id) == null)
                throw new ValidationException("Похід не знайдено.");

            _hikeRepository.Restore(id);
        }

        public void HardDelete(int id)
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

            if (string.IsNullOrWhiteSpace(hike.Name))
                throw new ValidationException("Назва походу є обов'язковою для заповнення.");

            if (hike.StartDate == default)
                throw new ValidationException("Дата початку походу є обов'язковою для заповнення.");

            if (hike.RouteId <= 0)
                throw new ValidationException("Необхідно обрати маршрут для походу.");

            if (_routeService.GetById(hike.RouteId) == null)
                throw new ValidationException(
                    $"Маршрут з ID {hike.RouteId} не знайдено. Оберіть коректний маршрут.");
        }
    }
}
