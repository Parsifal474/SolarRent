using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Services;

namespace SolarRent.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = App.Services.GetRequiredService<RegisterWindow>();
            registerWindow.Owner = Window.GetWindow(this);
            registerWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Открываем окно входа
                var loginWindow = App.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                // Закрываем текущее главное окно
                Window.GetWindow(this)?.Close();
            }
        }
    }
}