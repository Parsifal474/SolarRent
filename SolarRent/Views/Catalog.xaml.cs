using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Data;

namespace SolarRent
{
    public partial class Catalog : Window
    {
        public Catalog()
        {
            InitializeComponent();
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            // 🔹 Правильное обращение к статическому свойству
            var dbContext = App.Services.GetRequiredService<AppDbContext>();
            var addEquipmentWindow = new AddEquipmentWindow(dbContext);
            addEquipmentWindow.ShowDialog();
        }
    }
}