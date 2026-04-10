using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SolarRent.Models;
using SolarRent.Services;

namespace SolarRent
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthService _authService;

        public RegisterWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            cmbRole.SelectedIndex = 0;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pwdPassword.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть не менее 4 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(pwdDirectorPassword.Password))
            {
                MessageBox.Show("Введите пароль директора для подтверждения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем роль
            var selectedItem = cmbRole.SelectedItem as ComboBoxItem;
            if (selectedItem == null || !Enum.TryParse(selectedItem.Tag?.ToString(), out Role role))
            {
                MessageBox.Show("Ошибка выбора роли", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаём пользователя
            var newUser = new User
            {
                FullName = txtFullName.Text.Trim(),
                Login = txtLogin.Text.Trim(),
                PasswordHash = pwdPassword.Password, // AuthService захеширует
                Role = role
            };

            // Пытаемся создать (только директор может)
            bool success = await _authService.CreateUserAsync(newUser, pwdDirectorPassword.Password);

            if (success)
            {
                MessageBox.Show(
                    $"✅ Аккаунт создан!\n\n" +
                    $"Сотрудник: {newUser.FullName}\n" +
                    $"Логин: {newUser.Login}\n" +
                    $"Роль: {role}\n\n" +
                    $"Теперь сотрудник может войти в систему.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "❌ Ошибка создания аккаунта\n\n" +
                    "Возможные причины:\n" +
                    "• Неверный пароль директора\n" +
                    "• У вас нет прав (требуется роль Director)\n" +
                    "• Логин уже занят",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}