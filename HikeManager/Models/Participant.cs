namespace HikeManager.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public int HikeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Note { get; set; }

        public override string ToString() => FullName;
    }
}
