using System;

namespace HikeManager.Models
{
    public class Hike
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public int RouteId { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Додаткові властивості для UI — не зберігаються напряму в БД,
        // але заповнюються через JOIN у HikeRepository.GetAll() та GetById().
        public string? RouteName { get; set; }
        public int DurationDays { get; set; } = 1;

        // Обчислювана властивість: кінець походу = початок + (тривалість - 1 день).
        public DateTime EndDate => StartDate.AddDays(DurationDays - 1);

        public override string ToString() => $"{Name} ({StartDate:yyyy-MM-dd})";
    }
}
