using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Models;
using SolarRent.Services;

namespace SolarRent
{
    public partial class AddClient : Window
    {
        private readonly IClientService _clientService;

        // Конструктор с DI
        public AddClient(IClientService clientService)
        {
            InitializeComponent();
            _clientService = clientService;
        }

        // Конструктор для дизайнера (если нужен)
        public AddClient() : this(App.Services.GetRequiredService<IClientService>())
        {
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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

            try
            {
                var client = new Client
                {
                    FullName = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAdress.Text.Trim(),
                    TaxId = txtINN.Text.Trim(),
                    CompanyName = null, // можно добавить отдельное поле в UI
                    Type = string.IsNullOrWhiteSpace(txtINN.Text) ? "Individual" : "Company",
                    IsBlacklisted = chkIsBlacklisted.IsChecked ?? false
                };

                await _clientService.AddClientAsync(client);

                MessageBox.Show($"Клиент успешно добавлен!\n\nName: {client.FullName}\nPhone: {client.Phone}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}