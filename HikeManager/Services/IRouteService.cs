using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IRouteService
    {
        List<Route> GetAll();
        List<Route> GetHistory();
        Route? GetById(int id);
        void Add(Route route);
        void Update(Route route);
        void Delete(int id);
        void Restore(int id);
        void HardDelete(int id);
    }
}
