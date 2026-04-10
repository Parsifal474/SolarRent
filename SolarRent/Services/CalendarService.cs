using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly AppDbContext _context;

        public CalendarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<DateTime, List<CalendarEvent>>> GetMonthEventsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Загружаем аренды за период
            var rentals = await _context.RentalOrders
                .Include(r => r.Equipment)
                .Include(r => r.Client)
                .Where(r => r.StartDate <= endDate && r.EndDate >= startDate)
                .ToListAsync();

            // Загружаем продажи (если есть отдельная таблица SaleOrder, пока используем аренды со статусом "Sold"? 
            // Допустим, продажи пока не реализованы, оставим только аренды. Можно расширить позже.
            // Для демонстрации создадим словарь событий по дням.

            var eventsByDay = new Dictionary<DateTime, List<CalendarEvent>>();

            foreach (var rental in rentals)
            {
                // Для каждого дня аренды добавляем событие
                for (var date = rental.StartDate.Date; date <= rental.EndDate.Date; date = date.AddDays(1))
                {
                    if (!eventsByDay.ContainsKey(date))
                        eventsByDay[date] = new List<CalendarEvent>();

                    eventsByDay[date].Add(new CalendarEvent
                    {
                        Id = rental.Id,
                        Title = $"Аренда #{rental.Id}",
                        EquipmentName = rental.Equipment?.Name ?? "—",
                        ClientName = rental.Client?.FullName ?? "—",
                        EventType = "Аренда",
                        Date = date,
                        Amount = rental.RentalPrice,
                        Status = rental.Status
                    });
                }
            }

            // TODO: добавить продажи, если будет модель

            return eventsByDay;
        }
    }
}