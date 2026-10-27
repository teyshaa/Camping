using System;
using System.Collections.Generic;
using HikeManager.Models;
using HikeManager.DataAccess;

namespace HikeManager.Services
{
    public class EquipmentService
    {
        private readonly EquipmentRepository _repository;

        public EquipmentService()
        {
            _repository = new EquipmentRepository();
        }

        public List<EquipmentItem> GetByHike(int hikeId)
        {
            return _repository.GetByHike(hikeId);
        }

        public void Save(EquipmentItem item)
        {
            Validate(item);

            if (item.Id == 0)
                _repository.Insert(item);
            else
                _repository.Update(item);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        private void Validate(EquipmentItem item)
        {
            item.Name = item.Name?.Trim();
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ValidationException("Назва спорядження є обов'язковою.");
            }

            if (item.Quantity < 1)
            {
                throw new ValidationException("Кількість повинна бути більшою або рівною 1.");
            }
        }
    }
}