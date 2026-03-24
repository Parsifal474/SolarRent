using SolarRent.Core.Models;
using System.Collections.Generic;
using System.Text;

namespace SolarRent.Core.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Individual"; // Individual, Company
        public string FullName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsBlacklisted { get; set; }
        public ICollection<RentalOrder> Orders { get; set; } = new List<RentalOrder>();
    }
}