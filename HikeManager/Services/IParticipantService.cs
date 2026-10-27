using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IParticipantService
    {
        List<Participant> GetByHike(int hikeId);
        int CountByHike(int hikeId);
        void Add(Participant p);
        void Update(Participant p);
        void Delete(int id);
    }
}
