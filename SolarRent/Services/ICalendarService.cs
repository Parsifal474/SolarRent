using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface ICalendarService
    {
        Task<Dictionary<DateTime, List<CalendarEvent>>> GetMonthEventsAsync(int year, int month);
    }

    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // "Аренда" или "Продажа"
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}