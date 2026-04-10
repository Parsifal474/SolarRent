using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.ViewModels;

namespace SolarRent.Views.Pages
{
    public partial class Lease_acceptance : Page
    {
        public Lease_acceptance()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<LeaseAcceptanceViewModel>();
        }
    }
}