using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IHikeService
    {
        List<Hike> GetActive();
        List<Hike> GetHistory();
        Hike? GetById(int id);
        void Create(Hike hike);
        void Update(Hike hike);
        void Delete(int id);
        void RestoreHike(int id);
        void HardDelete(int id);
    }
}
