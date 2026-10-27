using System;

namespace HikeManager.Models
{
    public class Hike
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public int RouteId { get; set; }

        // Додаткове поле для зручного відображення назви маршруту в UI
        public string? RouteName { get; set; }

        public override string ToString() => $"{Name} ({StartDate:yyyy-MM-dd})";
    }
}
