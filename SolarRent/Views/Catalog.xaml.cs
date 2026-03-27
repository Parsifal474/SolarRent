using System.Windows;
using SolarRent.ViewModels;

namespace SolarRent
{
    public partial class Catalog : Window
    {
        public Catalog()
        {
            InitializeComponent();
            DataContext = new CatalogViewModel();
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var addEquipmentWindow = new AddEquipmentWindow();
            var result = addEquipmentWindow.ShowDialog();

            if (result == true)
            {
                // Оборудование добавлено, можно обновить список
                MessageBox.Show("Оборудование добавлено!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}