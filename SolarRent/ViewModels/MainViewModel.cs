// ============================================================
// ViewModel: Главное окно приложения
// Разработчик: Дюмин С.А. (DevOps, тестировщик)
// ============================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace SolarRent.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;

        [RelayCommand]
        private void OpenCatalog()
        {
            CurrentView = new TextBlock { Text = "Каталог оборудования (в разработке)" };
        }

        [RelayCommand]
        private void OpenRental()
        {
            CurrentView = new TextBlock { Text = "Модуль аренды (в разработке)" };
        }

        [RelayCommand]
        private void OpenClients()
        {
            CurrentView = new TextBlock { Text = "База клиентов (в разработке)" };
        }

        [RelayCommand]
        private void OpenReports()
        {
            CurrentView = new TextBlock { Text = "Отчёты и аналитика (в разработке)" };
        }
    }
}