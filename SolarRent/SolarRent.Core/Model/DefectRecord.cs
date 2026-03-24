using System;
using SolarRent.Core.Models;

namespace SolarRent.Core.Models
{
    public class DefectRecord
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public DateTime CheckDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty; // Repaired, Replaced, WriteOff
    }
}