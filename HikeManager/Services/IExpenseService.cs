using System.Collections.Generic;
using HikeManager.Models;

namespace HikeManager.Services
{
    public interface IExpenseService
    {
        List<Expense> GetByHike(int hikeId);
        void Add(Expense e);
        void Update(Expense e);
        void Delete(int id);
    }
}
