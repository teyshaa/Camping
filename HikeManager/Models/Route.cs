namespace HikeManager.Models
{
    public class Route
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Difficulty { get; set; } = "Легкий";
        public int DurationDays { get; set; } = 1;
        public double? LengthKm { get; set; }

        public override string ToString() => Name;
    }
}
