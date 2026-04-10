using Microsoft.Extensions.DependencyInjection;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SolarRent
{
    public partial class AddEquipmentWindow : Window
    {
        private readonly IEquipmentService _equipmentService;

        public AddEquipmentWindow(IEquipmentService equipmentService)
        {
            InitializeComponent();
            _equipmentService = equipmentService;
            InitializeForm();
        }

        public AddEquipmentWindow() : this(App.Services?.GetRequiredService<IEquipmentService>())
        {
        }

        private void InitializeForm()
        {
            cmbType.Items.Clear();
            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                string displayName = type switch
                {
                    EquipmentType.Panel => "🔆 Панель",
                    EquipmentType.Inverter => "⚡ Инвертор",
                    EquipmentType.Battery => "🔋 Аккумулятор",
                    EquipmentType.Accessory => "🔧 Комплектующее",
                    _ => type.ToString()
                };
                cmbType.Items.Add(new ComboBoxItem
                {
                    Content = displayName,
                    Tag = type
                });
            }
            cmbType.SelectedIndex = 0;
            txtPrice.TextChanged += TxtPrice_TextChanged;
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

        // 🔥 ИСПРАВЛЕНО: async void + await вместо GetAwaiter().GetResult()
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Блокируем интерфейс на время сохранения
            var saveButton = sender as Button;
            if (saveButton != null) saveButton.IsEnabled = false;
            this.Cursor = Cursors.Wait;

            try
            {
                // 🔹 Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название оборудования", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtName.Focus();
                    return;
                }

                if (cmbType.SelectedItem is not ComboBoxItem selectedTypeItem || selectedTypeItem.Tag is not EquipmentType equipmentType)
                {
                    MessageBox.Show("Выберите тип оборудования", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPower.Text))
                {
                    MessageBox.Show("Введите мощность", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPower.Focus();
                    return;
                }

                if (!double.TryParse(txtPower.Text.Trim().Replace("кВт·ч", "").Replace("кВт", "").Replace(" ", ""), out double power) || power <= 0)
                {
                    MessageBox.Show("Введите корректную мощность (число)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPower.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text.Replace(" ", "").Replace("₽", ""), out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную базовую цену", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPrice.Focus();
                    return;
                }

                // 🔹 Создаём объект
                var equipment = new Equipment
                {
                    Name = txtName.Text.Trim(),
                    Type = equipmentType,
                    Power = power,
                    Price = price,
                    Status = "InStock",
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                        ? null
                        : txtDescription.Text.Trim()
                };

                // 🔥 АСИНХРОННОЕ сохранение (не блокирует UI!)
                await _equipmentService.AddEquipmentAsync(equipment);

                MessageBox.Show(
                    $"✅ Оборудование сохранено!\n\n" +
                    $"📦 {equipment.Name}\n" +
                    $"🔧 Тип: {equipment.TypeDisplay}\n" +
                    $"⚡ Мощность: {equipment.PowerDisplay}\n" +
                    $"💰 Цена: ₽ {equipment.Price:N0}\n" +
                    $"📊 Аренда/день: ₽ {equipment.RentalPricePerDay:N0}\n" +
                    $"🔒 Залог: ₽ {equipment.DepositAmount:N0}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Ошибка сохранения:\n\n{ex.Message}\n\n" +
                    $"Проверьте:\n" +
                    $"• Подключение к базе данных\n" +
                    $"• Корректность введённых данных",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Console.WriteLine($"❌ ERROR: {ex}");
            }
            finally
            {
                // 🔥 Всегда разблокируем интерфейс
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