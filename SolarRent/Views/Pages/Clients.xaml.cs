using System.Windows.Controls;
using SolarRent.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace SolarRent.Views.Pages
{
    public partial class Clients : Page
    {
        public Clients()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ClientsViewModel>();
        }
    }
}