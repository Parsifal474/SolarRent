using System.Collections.Generic;
using System.Windows;
using SolarRent.Models;

namespace SolarRent.Views
{
    public partial class ClientOrdersWindow : Window
    {
        public ClientOrdersWindow(Client client, IEnumerable<RentalOrder> orders)
        {
            InitializeComponent();
            DataContext = new { ClientName = client.FullName, Orders = orders };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}