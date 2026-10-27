using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IEquipmentService
    {
        List<EquipmentItem> GetByHike(int hikeId);
        void Add(EquipmentItem i);
        void Update(EquipmentItem i);
        void Delete(int id);
    }
}
