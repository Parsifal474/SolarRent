using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Models;
using SolarRent.Services;

namespace SolarRent
{
    public partial class AddClient : Window
    {
        private readonly IClientService _clientService;
        private Client? _editingClient;

        public Client? EditingClient
        {
            get => _editingClient;
            set
            {
                _editingClient = value;
                if (value != null)
                {
                    txtName.Text = value.FullName;
                    txtCompanyName.Text = value.CompanyName ?? "";
                    txtINN.Text = value.TaxId ?? "";
                    txtPhone.Text = FormatPhone(value.Phone); // форматируем при загрузке
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
            txtPhone.Text = "+7"; // начальное значение
        }

        public AddClient() : this(App.Services.GetRequiredService<IClientService>())
        {
        }

        private void ClearFields()
        {
            txtName.Text = "";
            txtCompanyName.Text = "";
            txtINN.Text = "";
            txtPhone.Text = "+7";
            txtEmail.Text = "";
            txtAdress.Text = "";
            chkIsBlacklisted.IsChecked = false;
        }

        private void Phone_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
                txtPhone.Text = "+7";
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            var textBox = (TextBox)sender;
            string currentText = textBox.Text;
            int caret = textBox.CaretIndex;

            // Удаляем всё кроме цифр
            string digits = new string(currentText.Where(char.IsDigit).ToArray());

            // Добавляем новую цифру в позицию каретки (упрощённо - в конец)
            // Более точное управление кареткой требует сложной логики, пока вставляем в конец
            if (digits.Length >= 11) // максимум 11 цифр (+7 и 10 цифр номера)
            {
                e.Handled = true;
                return;
            }

            // Формируем новый номер
            string newDigits = digits + e.Text;
            string formatted = FormatPhoneFromDigits(newDigits);

            textBox.Text = formatted;
            textBox.CaretIndex = formatted.Length;
            e.Handled = true;
        }

        private string FormatPhoneFromDigits(string digits)
        {
            if (digits.Length == 0) return "+7";
            if (digits.Length == 1) return $"+7({digits}";
            if (digits.Length <= 4)
                return $"+7({digits.Substring(1, digits.Length - 1)}";
            if (digits.Length <= 7)
                return $"+7({digits.Substring(1, 3)})-{digits.Substring(4, digits.Length - 4)}";
            if (digits.Length <= 9)
                return $"+7({digits.Substring(1, 3)})-{digits.Substring(4, 3)}-{digits.Substring(7, digits.Length - 7)}";
            // 10 или 11 цифр
            return $"+7({digits.Substring(1, 3)})-{digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, Math.Min(2, digits.Length - 9))}";
        }

        private string FormatPhone(string rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone)) return "+7";
            string digits = new string(rawPhone.Where(char.IsDigit).ToArray());
            return FormatPhoneFromDigits(digits);
        }

        private string ExtractDigits(string formattedPhone)
        {
            return new string(formattedPhone.Where(char.IsDigit).ToArray());
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Поле \"ФИО / Контактное лицо\" обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text == "+7")
            {
                MessageBox.Show("Поле \"Телефон\" обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return;
            }

            // Валидация ИНН (если заполнен)
            string inn = txtINN.Text.Trim();
            if (!string.IsNullOrEmpty(inn))
            {
                if (!Regex.IsMatch(inn, @"^\d{10}$|^\d{12}$"))
                {
                    MessageBox.Show("ИНН должен содержать 10 или 12 цифр", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtINN.Focus();
                    return;
                }
            }

            // Валидация телефона (должен быть полный номер 11 цифр)
            string phoneDigits = ExtractDigits(txtPhone.Text);
            if (phoneDigits.Length != 11)
            {
                MessageBox.Show("Введите полный номер телефона (11 цифр, начиная с 7)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return;
            }

            try
            {
                if (_editingClient == null)
                {
                    var client = new Client
                    {
                        FullName = txtName.Text.Trim(),
                        CompanyName = string.IsNullOrWhiteSpace(txtCompanyName.Text) ? null : txtCompanyName.Text.Trim(),
                        TaxId = inn,
                        Phone = phoneDigits, // сохраняем только цифры
                        Email = txtEmail.Text.Trim(),
                        Address = txtAdress.Text.Trim(),
                        Type = string.IsNullOrWhiteSpace(inn) ? "Individual" : "Company",
                        IsBlacklisted = chkIsBlacklisted.IsChecked ?? false
                    };
                    await _clientService.AddClientAsync(client);
                    MessageBox.Show($"Клиент успешно добавлен!\n\n{client.FullName}\nТелефон: {txtPhone.Text}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _editingClient.FullName = txtName.Text.Trim();
                    _editingClient.CompanyName = string.IsNullOrWhiteSpace(txtCompanyName.Text) ? null : txtCompanyName.Text.Trim();
                    _editingClient.TaxId = inn;
                    _editingClient.Phone = phoneDigits;
                    _editingClient.Email = txtEmail.Text.Trim();
                    _editingClient.Address = txtAdress.Text.Trim();
                    _editingClient.Type = string.IsNullOrWhiteSpace(inn) ? "Individual" : "Company";
                    _editingClient.IsBlacklisted = chkIsBlacklisted.IsChecked ?? false;

                    await _clientService.UpdateClientAsync(_editingClient);
                    MessageBox.Show($"Клиент успешно обновлён!\n\n{_editingClient.FullName}\nТелефон: {txtPhone.Text}",
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