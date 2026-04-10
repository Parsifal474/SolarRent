using Microsoft.Extensions.DependencyInjection;
using SolarRent.ViewModels;
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
    }
}