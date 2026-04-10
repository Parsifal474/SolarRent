using Microsoft.Extensions.DependencyInjection;
using SolarRent.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent.Views.Pages
{
    public partial class Sale : Page
    {
        public Sale(SaleViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // Конструктор для DI
        public Sale() : this(App.Services?.GetRequiredService<SaleViewModel>())
        {
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = App.Services.GetRequiredService<SalesHistoryWindow>();
            historyWindow.Owner = Window.GetWindow(this);
            historyWindow.ShowDialog();
        }
    }
}