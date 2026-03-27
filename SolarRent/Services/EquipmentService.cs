// ============================================================
// Реализация: Сервис для работы с оборудованием
// Разработчик: Яковчук В.П. (Team Lead)
// ============================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarRent.Models;
using SolarRent.Data.Repositories;

namespace SolarRent.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IRepository<Equipment> _equipmentRepo;

        public EquipmentService(IRepository<Equipment> equipmentRepo)
        {
            _equipmentRepo = equipmentRepo;
        }

        public async Task<IEnumerable<Equipment>> GetAvailableAsync()
        {
            var all = await _equipmentRepo.GetAllAsync();
            return all.Where(e => e.Status == "InStock");
        }

        public async Task AddEquipmentAsync(Equipment equipment)
        {
            await _equipmentRepo.AddAsync(equipment);
            await _equipmentRepo.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int equipmentId, string newStatus)
        {
            var eq = await _equipmentRepo.GetByIdAsync(equipmentId);
            if (eq != null)
            {
                eq.Status = newStatus;
                _equipmentRepo.Update(eq);
                await _equipmentRepo.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Equipment>> FilterAsync(EquipmentType? type, double? maxPower, decimal? maxPrice)
        {
            var all = await _equipmentRepo.GetAllAsync();
            if (type.HasValue)
                all = all.Where(e => e.Type == type.Value);
            if (maxPower.HasValue)
                all = all.Where(e => e.Power <= maxPower.Value);
            if (maxPrice.HasValue)
                all = all.Where(e => e.Price <= maxPrice.Value);
            return all;
        }
    }
}