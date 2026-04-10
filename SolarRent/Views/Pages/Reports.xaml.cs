using System.Windows.Controls;
using SolarRent.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace SolarRent.Views.Pages
{
    public partial class Reports : Page
    {
        public Reports()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ReportsViewModel>();
        }
    }
}