using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarRent.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SolarRent.ViewModels
{
    public partial class RentalCalendarViewModel : ObservableObject
    {
        private readonly ICalendarService _calendarService;

        [ObservableProperty]
        private DateTime _currentMonth;

        [ObservableProperty]
        private ObservableCollection<CalendarDay> _days = new();

        [ObservableProperty]
        private bool _isLoading;

        public RentalCalendarViewModel(ICalendarService calendarService)
        {
            _calendarService = calendarService;
            CurrentMonth = DateTime.Today;
            _ = LoadMonthAsync();
        }

        [RelayCommand]
        private async Task LoadMonthAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var events = await _calendarService.GetMonthEventsAsync(CurrentMonth.Year, CurrentMonth.Month);
                GenerateDays(events);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            _ = LoadMonthAsync();
        }

        [RelayCommand]
        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            _ = LoadMonthAsync();
        }

        [RelayCommand]
        private void DayClick(CalendarDay day)
        {
            if (day?.Events == null || day.Events.Count == 0) return;

            var detailsWindow = new Views.DayEventsWindow(day.Date, day.Events);
            detailsWindow.Owner = Application.Current.MainWindow;
            detailsWindow.ShowDialog();
        }

        private void GenerateDays(Dictionary<DateTime, List<CalendarEvent>> events)
        {
            Days.Clear();
            var firstOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int startOffset = ((int)firstOfMonth.DayOfWeek + 6) % 7; // Пн = 0
            var startDate = firstOfMonth.AddDays(-startOffset);

            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                var day = new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == CurrentMonth.Month,
                    Events = events.ContainsKey(date) ? events[date] : new List<CalendarEvent>()
                };
                Days.Add(day);
            }
        }
    }

    public partial class CalendarDay : ObservableObject
    {
        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private bool _isCurrentMonth;

        [ObservableProperty]
        private List<CalendarEvent> _events = new();

        public string DayNumber => Date.Day.ToString();
        public bool HasEvents => Events.Count > 0;
        public string TooltipText => HasEvents
            ? string.Join("\n", Events.Select(e => $"{e.EventType}: {e.EquipmentName}"))
            : "Нет событий";
    }
}