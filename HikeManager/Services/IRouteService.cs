using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IRouteService
    {
        List<Route> GetAll();
        Route? GetById(int id);
        void Add(Route route);
        void Update(Route route);
        void Delete(int id);
    }
}
