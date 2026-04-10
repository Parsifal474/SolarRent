using System.Windows.Controls;
using SolarRent.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace SolarRent.Views.Pages
{
    public partial class Catalog : Page
    {
        public Catalog()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<CatalogViewModel>();
        }
    }
}