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
            // Используем UTC для совместимости с PostgreSQL timestamp with time zone
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var rentals = await _context.RentalOrders
                .Include(r => r.Equipment)
                .Include(r => r.Client)
                .Where(r => r.StartDate <= endDate && r.EndDate >= startDate)
                .ToListAsync();

            var eventsByDay = new Dictionary<DateTime, List<CalendarEvent>>();

            foreach (var rental in rentals)
            {
                var current = rental.StartDate.Date;
                while (current <= rental.EndDate.Date)
                {
                    if (!eventsByDay.ContainsKey(current))
                        eventsByDay[current] = new List<CalendarEvent>();

                    eventsByDay[current].Add(new CalendarEvent
                    {
                        Id = rental.Id,
                        Title = $"Аренда #{rental.Id}",
                        EquipmentName = rental.Equipment?.Name ?? "—",
                        ClientName = rental.Client?.FullName ?? "—",
                        EventType = "Аренда",
                        Date = current,
                        Amount = rental.RentalPrice,
                        Status = rental.Status
                    });

                    current = current.AddDays(1);
                }
            }

            return eventsByDay;
        }
    }
}