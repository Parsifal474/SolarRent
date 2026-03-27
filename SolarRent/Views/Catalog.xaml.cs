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
    }
}