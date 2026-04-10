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
        private Client? _editingClient;

        // Свойство для определения режима редактирования
        public Client? EditingClient
        {
            get => _editingClient;
            set
            {
                _editingClient = value;
                if (value != null)
                {
                    // Заполняем поля данными клиента
                    txtName.Text = value.FullName;
                    txtINN.Text = value.TaxId ?? "";
                    txtKPP.Text = ""; // KPP нет в модели, можно убрать или добавить в будущем
                    txtPhone.Text = value.Phone;
                    txtEmail.Text = value.Email;
                    txtAdress.Text = value.Address ?? "";
                    chkIsBlacklisted.IsChecked = value.IsBlacklisted;
                    Title = "Редактирование клиента";
                }
                else
                {
                    ClearFields();
                    Title = "Добавить клиента";
                }
            }
        }

        public AddClient(IClientService clientService)
        {
            InitializeComponent();
            _clientService = clientService;
        }

        // Конструктор для дизайнера
        public AddClient() : this(App.Services.GetRequiredService<IClientService>())
        {
        }

        private void ClearFields()
        {
            txtName.Text = "";
            txtINN.Text = "";
            txtKPP.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAdress.Text = "";
            chkIsBlacklisted.IsChecked = false;
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
                if (_editingClient == null)
                {
                    // Режим добавления
                    var client = new Client
                    {
                        FullName = txtName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Address = txtAdress.Text.Trim(),
                        TaxId = txtINN.Text.Trim(),
                        CompanyName = null,
                        Type = string.IsNullOrWhiteSpace(txtINN.Text) ? "Individual" : "Company",
                        IsBlacklisted = chkIsBlacklisted.IsChecked ?? false
                    };
                    await _clientService.AddClientAsync(client);
                    MessageBox.Show($"Клиент успешно добавлен!\n\nName: {client.FullName}\nPhone: {client.Phone}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Режим редактирования
                    _editingClient.FullName = txtName.Text.Trim();
                    _editingClient.Phone = txtPhone.Text.Trim();
                    _editingClient.Email = txtEmail.Text.Trim();
                    _editingClient.Address = txtAdress.Text.Trim();
                    _editingClient.TaxId = txtINN.Text.Trim();
                    _editingClient.Type = string.IsNullOrWhiteSpace(txtINN.Text) ? "Individual" : "Company";
                    _editingClient.IsBlacklisted = chkIsBlacklisted.IsChecked ?? false;
                    // CompanyName оставляем без изменений (можно добавить отдельное поле при необходимости)

                    await _clientService.UpdateClientAsync(_editingClient);
                    MessageBox.Show($"Клиент успешно обновлён!\n\nName: {_editingClient.FullName}\nPhone: {_editingClient.Phone}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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