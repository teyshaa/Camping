namespace HikeManager.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }
        public int HikeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        public override string ToString() => $"{Name} x{Quantity}";
    }
}
