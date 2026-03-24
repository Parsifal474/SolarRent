using System.Collections.Generic;
using SolarRent.Core.Models;

namespace SolarRent.SolarRent.Core.Models
{
    public enum EquipmentType
    {
        Panel,
        Inverter,
        Battery,
        Accessory
    }

    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EquipmentType Type { get; set; }
        public double Power { get; set; } // в кВт
        public decimal Price { get; set; }
        public string Status { get; set; } = "InStock"; // InStock, Rented, Repair, Disposed
        public string? Description { get; set; }
        public ICollection<RentalOrder> Rentals { get; set; } = new List<RentalOrder>();
        public ICollection<DefectRecord> Defects { get; set; } = new List<DefectRecord>();
    }
}