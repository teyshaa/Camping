using System;
using System.Collections.Generic;
using HikeManager.Models;
using HikeManager.DataAccess;

namespace HikeManager.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly EquipmentRepository _repository;

        // Статичний перелік базового набору спорядження (бонус)
        public static readonly List<(string Name, int Quantity)> DefaultBasicKit = new()
        {
            ("Намет туристичний", 1),
            ("Спальний мішок", 1),
            ("Килимок туристичний (каремат)", 1),
            ("Аптечка групова", 1),
            ("Ліхтарик налобний", 1),
            ("Газовий пальник та казанок", 1),
            ("Дощовик / пончо", 1),
            ("Ніж складний туристичний", 1)
        };

        public EquipmentService(EquipmentRepository? repository = null)
        {
            _repository = repository ?? new EquipmentRepository();
        }

        public List<EquipmentItem> GetByHike(int hikeId)
        {
            return _repository.GetByHike(hikeId);
        }

        public void Add(EquipmentItem item)
        {
            Validate(item);

            // Правило для дублікатів: сумування кількості
            var existingItems = _repository.GetByHike(item.HikeId);
            var duplicate = existingItems.Find(x => string.Equals(x.Name?.Trim(), item.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
            {
                duplicate.Quantity += item.Quantity;
                _repository.Update(duplicate);
                item.Id = duplicate.Id;
                item.Quantity = duplicate.Quantity;
            }
            else
            {
                _repository.Insert(item);
            }
        }

        public void Update(EquipmentItem item)
        {
            Validate(item);

            var existingItems = _repository.GetByHike(item.HikeId);
            var duplicate = existingItems.Find(x => x.Id != item.Id && string.Equals(x.Name?.Trim(), item.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
            {
                throw new ValidationException($"Спорядження з назвою «{item.Name}» уже існує у списку цього походу.");
            }

            _repository.Update(item);
        }

        public void Save(EquipmentItem item)
        {
            if (item.Id == 0)
                Add(item);
            else
                Update(item);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        /// <summary>
        /// Додає базовий набір спорядження, якщо таких найменувань ще немає в списку походу.
        /// Повертає кількість доданих нових позицій.
        /// </summary>
        public int AddBasicKit(int hikeId)
        {
            var existing = _repository.GetByHike(hikeId);
            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in existing)
            {
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    existingNames.Add(item.Name.Trim());
                }
            }

            int addedCount = 0;
            foreach (var (name, qty) in DefaultBasicKit)
            {
                if (!existingNames.Contains(name))
                {
                    _repository.Insert(new EquipmentItem
                    {
                        HikeId = hikeId,
                        Name = name,
                        Quantity = qty
                    });
                    existingNames.Add(name);
                    addedCount++;
                }
            }

            return addedCount;
        }

        private void Validate(EquipmentItem item)
        {
            item.Name = item.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ValidationException("Назва спорядження є обов'язковою.");
            }

            if (item.Name.Length > 100)
            {
                throw new ValidationException("Назва спорядження не може перевищувати 100 символів.");
            }

            if (item.Quantity < 1)
            {
                throw new ValidationException("Кількість повинна бути більшою або рівною 1.");
            }

            if (item.Quantity > 1000)
            {
                throw new ValidationException("Кількість не може перевищувати 1000.");
            }
        }
    }
}