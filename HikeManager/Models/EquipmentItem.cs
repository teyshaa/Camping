namespace HikeManager.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }
        public int HikeId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}