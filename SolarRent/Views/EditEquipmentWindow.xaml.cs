using SolarRent.Models;
using SolarRent.Services;
using SolarRent.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SolarRent
{
    public partial class EditEquipmentWindow : Window
    {
        private readonly IEquipmentService _equipmentService;
        private readonly EquipmentItem _originalItem;

        public EditEquipmentWindow(IEquipmentService equipmentService, EquipmentItem item)
        {
            InitializeComponent();
            _equipmentService = equipmentService;
            _originalItem = item;
            LoadEquipmentData();
            txtPrice.TextChanged += TxtPrice_TextChanged;
        }

        private void LoadEquipmentData()
        {
            txtId.Text = _originalItem.Id.ToString();
            txtName.Text = _originalItem.Name;
            txtTypeDisplay.Text = _originalItem.TypeDisplay;
            txtPower.Text = _originalItem.Power.ToString();
            txtPrice.Text = _originalItem.Price.ToString();
            txtDescription.Text = _originalItem.Description ?? string.Empty;
            TxtPrice_TextChanged(null, null);
        }

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtPrice.Text.Replace(" ", "").Replace("₽", ""), out decimal price) && price > 0)
            {
                lblRentalPricePerDay.Text = $"₽ {(price * 0.01m):N0}/день";
                lblDepositAmount.Text = $"₽ {(price * 0.5m):N0}";
            }
            else
            {
                lblRentalPricePerDay.Text = "₽ 0/день";
                lblDepositAmount.Text = "₽ 0";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveButton = sender as Button;
            if (saveButton != null) saveButton.IsEnabled = false;
            this.Cursor = Cursors.Wait;

            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(txtPower.Text.Trim().Replace("кВт", "").Replace(" ", ""), out double power) || power <= 0)
                {
                    MessageBox.Show("Введите корректную мощность", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text.Replace(" ", "").Replace("₽", ""), out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedEquipment = new Equipment
                {
                    Id = _originalItem.Id,
                    Name = txtName.Text.Trim(),
                    Type = _originalItem.Type,
                    Power = power,
                    Price = price,
                    Status = _originalItem.Status,
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim()
                };

                await _equipmentService.UpdateAsync(updatedEquipment);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (saveButton != null) saveButton.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}