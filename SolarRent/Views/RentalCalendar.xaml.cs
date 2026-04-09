using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace SolarRent
{
    public partial class RentalCalendar : Window
    {
        public RentalCalendar()
        {
            InitializeComponent();
        }
        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = App.Services.GetRequiredService<AddEquipmentWindow>();
            addWindow.ShowDialog();
        }
    }
}