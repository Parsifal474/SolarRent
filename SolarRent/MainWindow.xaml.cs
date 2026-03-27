using System.Windows;

namespace SolarRent
{
    public partial class MainDashboardWindow : Window
    {
        private Catalog _catalogWindow;

        public MainDashboardWindow()
        {
            InitializeComponent();
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            // Если окно каталога еще не создано или закрыто
            if (_catalogWindow == null || !_catalogWindow.IsVisible)
            {
                _catalogWindow = new Catalog();
                _catalogWindow.Show();
            }
            else
            {
                // Если окно уже открыто - активируем его
                _catalogWindow.Activate();
                _catalogWindow.WindowState = WindowState.Normal;
                _catalogWindow.Focus();
            }
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            var addClientWindow = new AddClient();
            addClientWindow.ShowDialog();
        }
    }
}