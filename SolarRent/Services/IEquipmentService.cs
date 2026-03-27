// ============================================================
// Интерфейс: Сервис для работы с оборудованием
// Разработчик: Яковчук В.П. (Team Lead)
// ============================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface IEquipmentService
    {
        Task<IEnumerable<Equipment>> GetAvailableAsync();
        Task AddEquipmentAsync(Equipment equipment);
        Task UpdateStatusAsync(int equipmentId, string newStatus);
        Task<IEnumerable<Equipment>> FilterAsync(EquipmentType? type, double? maxPower, decimal? maxPrice);
    }
}