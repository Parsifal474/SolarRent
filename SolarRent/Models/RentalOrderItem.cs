namespace SolarRent.Models
{
    public class RentalOrderItem
    {
        public int Id { get; set; }
        public int RentalOrderId { get; set; }
        public RentalOrder RentalOrder { get; set; } = null!;
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public string? SerialNumber { get; set; }
        public string? ConditionNote { get; set; }
    }
}