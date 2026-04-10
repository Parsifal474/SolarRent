// Views/SaleDetailsWindow.xaml.cs
using System.Windows;
using SolarRent.Models;

namespace SolarRent.Views
{
    public partial class SaleDetailsWindow : Window
    {
        public SaleDetailsWindow(SaleRecord sale)
        {
            InitializeComponent();
            DataContext = sale;
        }
    }
}