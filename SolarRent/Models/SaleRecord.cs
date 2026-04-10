// Models/SaleRecord.cs
using System;
using System.Collections.Generic;

namespace SolarRent.Models
{
    public class SaleRecord
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Наличные";
        public int? ManagedByUserId { get; set; }
        public User? ManagedBy { get; set; }
        public string? Notes { get; set; }
        public ICollection<SaleItemRecord> Items { get; set; } = new List<SaleItemRecord>();
    }

    public class SaleItemRecord
    {
        public int Id { get; set; }
        public int SaleRecordId { get; set; }
        public SaleRecord? SaleRecord { get; set; }
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}