using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent
{
    public partial class NewRental : Window
    {
        private decimal _pricePerDay = 1500;
        private decimal _deposit = 3000;

        public NewRental()
        {
            InitializeComponent();
            dpStartDate.SelectedDate = DateTime.Today;
            UpdateEquipmentInfo();
        }

        private void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateEndDate();
            CalculateTotal();
        }

        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
        }

        private void cmbEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEquipmentInfo();
        }

        private void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void UpdateEquipmentInfo()
        {
            if (cmbEquipment.SelectedItem is ComboBoxItem item)
            {
                string equipmentName = item.Content.ToString();

                switch (equipmentName)
                {
                    case "Солнечная панель 300W":
                        _pricePerDay = 1500;
                        _deposit = 3000;
                        break;
                    case "Солнечная панель 490W":
                        _pricePerDay = 2000;
                        _deposit = 4000;
                        break;
                    case "Инвертор 5kW":
                        _pricePerDay = 2500;
                        _deposit = 5000;
                        break;
                    case "Аккумулятор 10kWh":
                        _pricePerDay = 1800;
                        _deposit = 3500;
                        break;
                    default:
                        _pricePerDay = 1500;
                        _deposit = 3000;
                        break;
                }

                lblPricePerDay.Text = $"₽ {_pricePerDay:N0}";
                lblDeposit.Text = $"₽ {_deposit:N0}";
                CalculateTotal();
            }
        }

        private void CalculateEndDate()
        {
            if (dpStartDate.SelectedDate.HasValue && int.TryParse(txtDays.Text, out int days))
            {
                DateTime endDate = dpStartDate.SelectedDate.Value.AddDays(days);
                txtEndDate.Text = endDate.ToString("dd.MM.yyyy", new CultureInfo("ru-RU"));
            }
            else
            {
                txtEndDate.Text = "";
            }
        }

        private void CalculateTotal()
        {
            if (int.TryParse(txtDays.Text, out int days) && days > 0)
            {
                decimal total = _pricePerDay * days;

                if (decimal.TryParse(txtDiscount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discount))
                {
                    if (discount > 0 && discount <= 100)
                    {
                        total = total - (total * discount / 100);
                    }
                }

                lblTotal.Text = $"₽ {total:N0}";
            }
            else
            {
                lblTotal.Text = "₽ 0";
            }
        }

        private void CreateRentalButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbClient.Focus();
                return;
            }

            if (cmbEquipment.SelectedItem == null)
            {
                MessageBox.Show("Выберите оборудование", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbEquipment.Focus();
                return;
            }

            if (!int.TryParse(txtDays.Text, out int days) || days <= 0)
            {
                MessageBox.Show("Введите корректное количество дней", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDays.Focus();
                return;
            }

            if (!dpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату начала", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return;
            }

            string clientName = (cmbClient.SelectedItem as ComboBoxItem)?.Content.ToString();
            string equipmentName = (cmbEquipment.SelectedItem as ComboBoxItem)?.Content.ToString();

            MessageBox.Show($"Аренда успешно создана!\n\n" +
                          $"Клиент: {clientName}\n" +
                          $"Оборудование: {equipmentName}\n" +
                          $"Количество дней: {days}\n" +
                          $"Дата начала: {dpStartDate.SelectedDate.Value:dd.MM.yyyy}\n" +
                          $"Сумма: {lblTotal.Text}",
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