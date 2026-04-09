using Microsoft.Extensions.DependencyInjection;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent
{
    public partial class AddEquipmentWindow : Window
    {
        private readonly IEquipmentService _equipmentService;

        // 🔥 Конструктор с внедрением сервиса (для DI)
        public AddEquipmentWindow(IEquipmentService equipmentService)
        {
            InitializeComponent();
            _equipmentService = equipmentService;
            InitializeForm();
        }

        // 🔥 Конструктор для дизайнера / ручного запуска (опционально)
        public AddEquipmentWindow() : this(App.Services?.GetRequiredService<IEquipmentService>())
        {
        }

        private void InitializeForm()
        {
            // Заполняем ComboBox типами оборудования
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

            // 🔥 Подписка на изменение цены для авто-расчёта
            txtPrice.TextChanged += TxtPrice_TextChanged;
        }


        // 🔥 Авто-расчёт при изменении цены
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 🔹 Шаг 1: Валидация
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

                // 🔹 Шаг 2: Создаём объект модели
                var equipment = new Equipment
                {
                    Name = txtName.Text.Trim(),
                    Type = equipmentType,
                    Power = power,
                    Price = price,  // 🔥 Базовая цена, от которой считаются производные
                    Status = "InStock",
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                        ? null
                        : txtDescription.Text.Trim()
                };

                // 🔹 Шаг 3: Сохранение через сервис (не через DbContext!)
                _equipmentService.AddEquipmentAsync(equipment).GetAwaiter().GetResult();

                // 🔹 Шаг 4: Успешный результат
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
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}