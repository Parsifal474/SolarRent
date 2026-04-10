// Services/SaleService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent.Services
{
    public class SaleService : ISaleService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public SaleService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IEnumerable<SaleRecord>> GetAllSalesAsync()
        {
            return await _context.SaleRecords
                .Include(s => s.Client)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Equipment)
                .Include(s => s.ManagedBy)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleRecord>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            return await _context.SaleRecords
                .Include(s => s.Client)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Equipment)
                .Include(s => s.ManagedBy)
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<SaleRecord?> GetSaleByIdAsync(int id)
        {
            return await _context.SaleRecords
                .Include(s => s.Client)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Equipment)
                .Include(s => s.ManagedBy)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SaleRecord> CreateSaleAsync(SaleRecord sale, List<SaleItemRecord> items)
        {
            sale.SaleDate = DateTime.UtcNow;
            sale.ManagedByUserId = _authService.CurrentUser?.Id;

            await _context.SaleRecords.AddAsync(sale);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                item.SaleRecordId = sale.Id;
                await _context.SaleItemRecords.AddAsync(item);

                // Обновляем статус оборудования
                var equipment = await _context.Equipments.FindAsync(item.EquipmentId);
                if (equipment != null)
                {
                    equipment.Status = "Sold";
                    _context.Equipments.Update(equipment);
                }
            }

            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task DeleteSaleAsync(int id)
        {
            var sale = await _context.SaleRecords.FindAsync(id);
            if (sale != null)
            {
                _context.SaleRecords.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SaleRecords.AsQueryable();

            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(s => s.SaleDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
                query = query.Where(s => s.SaleDate <= end);
            }

            return await query.SumAsync(s => s.TotalAmount);
        }
    }
}