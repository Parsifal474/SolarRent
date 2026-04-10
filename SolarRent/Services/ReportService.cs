using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;
using SolarRent.ViewModels;

namespace SolarRent.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            // Приводим даты к UTC для корректного сравнения с PostgreSQL
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            return await _context.RentalOrders
                .Where(o => o.StartDate >= start && o.StartDate <= end)
                .SumAsync(o => o.RentalPrice);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            return await _context.RentalOrders
                .Where(o => o.StartDate >= start && o.StartDate <= end)
                .CountAsync();
        }

        public async Task<int> GetNewClientsCountAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            // У клиентов нет даты создания, используем дату первого заказа
            var newClients = await _context.RentalOrders
                .Where(o => o.StartDate >= start && o.StartDate <= end)
                .Select(o => o.ClientId)
                .Distinct()
                .CountAsync();

            return newClients;
        }

        public async Task<List<DebtorInfo>> GetDebtorsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var overdueOrders = await _context.RentalOrders
                .Include(o => o.Client)
                .Where(o => o.Status == "Overdue" ||
                           (o.Status == "Active" && o.EndDate < today))
                .OrderByDescending(o => o.EndDate)
                .ToListAsync();

            return overdueOrders.Select(o => new DebtorInfo
            {
                ClientId = o.ClientId,
                ClientName = o.Client?.FullName ?? "Неизвестный клиент",
                OrderId = o.Id,
                DebtAmount = o.RentalPrice + o.Penalty - o.Deposit,
                DaysOverdue = (int)(today - o.EndDate.Date).TotalDays,
                DueDate = o.EndDate,
                Phone = o.Client?.Phone ?? string.Empty
            }).ToList();
        }

        public async Task<List<ManagerStats>> GetManagerStatsAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            var stats = await _context.RentalOrders
                .Include(o => o.ManagedBy)
                .Where(o => o.StartDate >= start && o.StartDate <= end && o.ManagedByUserId != null)
                .GroupBy(o => new { o.ManagedByUserId, o.ManagedBy.FullName })
                .Select(g => new ManagerStats
                {
                    UserId = g.Key.ManagedByUserId.Value,
                    FullName = g.Key.FullName,
                    OrdersCount = g.Count(),
                    Revenue = g.Sum(o => o.RentalPrice),
                    ClientsCount = g.Select(o => o.ClientId).Distinct().Count()
                })
                .ToListAsync();

            return stats;
        }

        public async Task<List<EquipmentPopularity>> GetPopularEquipmentAsync(
            DateTime startDate, DateTime endDate, int topCount)
        {
            var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            var popular = await _context.RentalOrders
                .Include(o => o.Equipment)
                .Where(o => o.StartDate >= start && o.StartDate <= end)
                .GroupBy(o => new { o.EquipmentId, o.Equipment.Name, o.Equipment.Type })
                .Select(g => new EquipmentPopularity
                {
                    EquipmentId = g.Key.EquipmentId,
                    Name = g.Key.Name,
                    TypeDisplay = g.Key.Type.ToString(),
                    RentCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.RentalPrice)
                })
                .OrderByDescending(e => e.RentCount)
                .Take(topCount)
                .ToListAsync();

            return popular;
        }
    }
}