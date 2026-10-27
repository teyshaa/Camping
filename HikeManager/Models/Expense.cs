namespace HikeManager.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int HikeId { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        public override string ToString() => $"{Category}: {Amount:N2} грн";
    }
}
