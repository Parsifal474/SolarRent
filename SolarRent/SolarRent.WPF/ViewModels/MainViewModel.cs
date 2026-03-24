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
            // Пока без сервисов
        }

        [RelayCommand]
        private void OpenCatalog()
        {
            // Временная заглушка
            CurrentView = new TextBlock { Text = "Каталог (в разработке)" };
        }

        [RelayCommand]
        private void OpenRental()
        {
            CurrentView = new TextBlock { Text = "Аренда (в разработке)" };
        }

        [RelayCommand]
        private void OpenClients()
        {
            CurrentView = new TextBlock { Text = "Клиенты (в разработке)" };
        }

        [RelayCommand]
        private void OpenReports()
        {
            CurrentView = new TextBlock { Text = "Отчеты (в разработке)" };
        }
    }
}