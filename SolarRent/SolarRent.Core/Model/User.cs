using System.ComponentModel.DataAnnotations;

namespace SolarRent.SolarRent.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; } // Manager, Warehouse, Director, Accountant
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum Role
    {
        Manager,
        Warehouse,
        Director,
        Accountant
    }
}