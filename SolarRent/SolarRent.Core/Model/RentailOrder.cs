using SolarRent.Core.Models;
using System;

namespace SolarRent.Core.Models
{
    public class RentalOrder
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public decimal RentalPrice { get; set; }
        public decimal Deposit { get; set; }
        public decimal Penalty { get; set; }
        public string Status { get; set; } = "Active"; // Active, Returned, Overdue
        public int? ManagedByUserId { get; set; }
        public User? ManagedBy { get; set; }
    }
}