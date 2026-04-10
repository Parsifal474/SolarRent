using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarRent.Services.Navigation;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace SolarRent.ViewModels
{
    public partial class NavigationViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

        public NavigationViewModel(INavigationService navigation)
        {
            _navigation = navigation;
        }

        [RelayCommand]
        private void Navigate(string pageKey)
        {
            _navigation.NavigateTo(pageKey);
        }

        [RelayCommand]
        private void Logout()
        {
            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}