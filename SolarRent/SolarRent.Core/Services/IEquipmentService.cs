using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Core.Models;
using SolarRent.SolarRent.Core.Models;

namespace SolarRent.Core.Services
{
    public interface IEquipmentService
    {
        Task<IEnumerable<Equipment>> GetAvailableAsync();
        Task AddEquipmentAsync(Equipment equipment);
        Task UpdateStatusAsync(int equipmentId, string newStatus);
        Task<IEnumerable<Equipment>> FilterAsync(EquipmentType? type, double? maxPower, decimal? maxPrice);
    }
}