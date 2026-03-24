using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace SolarRent.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;

        public MainViewModel()
        {
            // Здесь можно инициализировать сервисы через DI
        }

        [RelayCommand]
        private void OpenCatalog() => CurrentView = new CatalogView();

        [RelayCommand]
        private void OpenRental() => CurrentView = new RentalView();

        [RelayCommand]
        private void OpenClients() => CurrentView = new ClientsView();

        [RelayCommand]
        private void OpenReports() => CurrentView = new ReportsView();
    }
}