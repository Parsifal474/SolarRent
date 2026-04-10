using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SolarRent.Services;

namespace SolarRent
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;

        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            cmbRole.SelectedIndex = 0;
        }

        public LoginWindow() : this(App.Services?.GetRequiredService<IAuthService>())
        {
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = pwdPassword.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Аутентификация (теперь без хешей)
            bool isAuthenticated = await _authService.AuthenticateAsync(login, password);

            if (!isAuthenticated)
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Открываем главное окно
            var mainWindow = App.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }
    }
}