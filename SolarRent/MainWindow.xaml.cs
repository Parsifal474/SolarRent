using System.Windows;
using System.Windows.Controls;

namespace SolarRent
{
    public partial class MainWindow // или LoginWindow
    {
        public MainWindow() // или LoginWindow()
        {
            InitializeComponent();
            cmbRole.SelectedIndex = 0; // Выбираем первую роль по умолчанию
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = pwdPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Простая валидация
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

            // Здесь будет ваша логика авторизации
            MessageBox.Show($"Вход выполнен!\nЛогин: {login}\nРоль: {role}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}