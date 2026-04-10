using System;
using System.Collections.Generic;
using System.Windows;
using SolarRent.Services;

namespace SolarRent.Views
{
    public partial class DayEventsWindow : Window
    {
        public DayEventsWindow(DateTime date, List<CalendarEvent> events)
        {
            InitializeComponent();
            TitleText.Text = date.ToString("dd MMMM yyyy");
            EventsItemsControl.ItemsSource = events;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}