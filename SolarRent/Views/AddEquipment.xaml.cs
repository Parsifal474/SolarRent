using System;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent
{
    public partial class AddEquipmentWindow : Window
    {
        public AddEquipmentWindow()
        {
            InitializeComponent();
            cmbType.SelectedIndex = 0; // По умолчанию "Панель"
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название оборудования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип оборудования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbType.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPower.Text))
            {
                MessageBox.Show("Введите мощность", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPower.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPricePerDay.Text))
            {
                MessageBox.Show("Введите цену за день", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPricePerDay.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDeposit.Text))
            {
                MessageBox.Show("Введите сумму залога", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeposit.Focus();
                return;
            }

            // Здесь добавьте код для сохранения оборудования в базу данных
            // Например:
            // var equipment = new Equipment
            // {
            //     Name = txtName.Text,
            //     Type = (cmbType.SelectedItem as ComboBoxItem).Content.ToString(),
            //     Power = txtPower.Text,
            //     PricePerDay = decimal.Parse(txtPricePerDay.Text),
            //     Deposit = decimal.Parse(txtDeposit.Text),
            //     Description = txtDescription.Text
            // };
            // dbContext.Equipment.Add(equipment);
            // await dbContext.SaveChangesAsync();

            MessageBox.Show($"Оборудование успешно добавлено!\n\n" +
                          $"Название: {txtName.Text}\n" +
                          $"Тип: {(cmbType.SelectedItem as ComboBoxItem).Content}\n" +
                          $"Мощность: {txtPower.Text}\n" +
                          $"Цена/день: ₽ {txtPricePerDay.Text}\n" +
                          $"Залог: ₽ {txtDeposit.Text}",
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