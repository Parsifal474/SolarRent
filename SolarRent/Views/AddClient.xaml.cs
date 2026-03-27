using System.Windows;

namespace SolarRent
{
    public partial class AddClient : Window
    {
        public AddClient()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Поле Name обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Поле Phone обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return;
            }

            MessageBox.Show($"Клиент успешно добавлен!\n\nName: {txtName.Text}\nPhone: {txtPhone.Text}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}