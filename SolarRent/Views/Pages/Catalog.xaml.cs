using System.Windows.Controls;
using SolarRent.ViewModels;

namespace SolarRent.Views.Pages
{
    public partial class Catalog : Page
    {
        public Catalog(CatalogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}