using System;
using System.Collections.Generic;

namespace SolarRent.Models
{
    public class RentalOrderProcessing
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Issued, Returned

        public List<OrderEquipmentItem> EquipmentItems { get; set; } = new();
        public List<string> PhotoPaths { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    public class OrderEquipmentItem
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Condition { get; set; } = "Good";
    }
}