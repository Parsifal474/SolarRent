using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent
{
    public partial class AddEquipmentWindow : Window
    {
        private readonly AppDbContext _dbContext;

        public AddEquipmentWindow(AppDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            cmbType.SelectedIndex = 0; // По умолчанию "Панель"
            LoadEquipmentTypes();
        }

        // Загрузка типов оборудования из модели (опционально)
        private void LoadEquipmentTypes()
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
                    return;
                }

                if (cmbType.SelectedItem is not ComboBoxItem selectedTypeItem)
                {
                    MessageBox.Show("Выберите тип оборудования", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPower.Text))
                {
                    MessageBox.Show("Введите мощность", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPricePerDay.Text.Replace(" ", ""), out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtDeposit.Text.Replace(" ", ""), out decimal deposit) || deposit < 0)
                {
                    MessageBox.Show("Введите корректную сумму залога", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔹 Шаг 2: Создаём объект
                var equipmentType = (EquipmentType)(selectedTypeItem.Tag ?? EquipmentType.Panel);

                string powerText = txtPower.Text.Trim()
                    .Replace("кВт·ч", "").Replace("кВт", "").Replace(" ", "");
                if (!double.TryParse(powerText, out double power))
                {
                    power = 0;
                }

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

                // 🔹 Шаг 3: Проверка подключения к БД
                var connection = _dbContext.Database.GetDbConnection();
                string dbName = connection.Database;
                string connectionString = _dbContext.Database.GetConnectionString();

                MessageBox.Show(
                    $" Информация о БД:\n\n" +
                    $"База: {dbName}\n\n" +
                    $"Оборудование:\n" +
                    $"  Name: {equipment.Name}\n" +
                    $"  Type: {equipment.Type}\n" +
                    $"  Power: {equipment.Power}\n" +
                    $"  Price: {equipment.Price}\n\n" +
                    $"Нажмите ОК для сохранения...",
                    "Отладка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 🔹 Шаг 4: Сохранение
                _dbContext.Equipments.Add(equipment);
                int savedCount = _dbContext.SaveChanges();

                // 🔹 Шаг 5: Результат
                string resultMessage =
                    $"✅ ОБОРУДОВАНИЕ СОХРАНЕНО!\n\n" +
                    $"ID: {equipment.Id}\n" +
                    $"Название: {equipment.Name}\n" +
                    $"Тип: {equipment.TypeDisplay}\n" +
                    $"Мощность: {equipment.PowerDisplay}\n" +
                    $"Цена: ₽ {equipment.Price:N0}\n\n" +
                    $"Записей сохранено: {savedCount}\n\n" +
                    $"📋 Теперь откройте pgAdmin и проверьте:\n" +
                    $"База: {dbName}\n" +
                    $"Таблица: Equipments\n" +
                    $"Нажмите F5 для обновления!";

                MessageBox.Show(resultMessage, "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Console.WriteLine($"✅ УСПЕХ: ID={equipment.Id}, Name={equipment.Name}");

                this.DialogResult = true;
                this.Close();
            }
            catch (DbUpdateException dbEx)
            {
                string errorMsg = $"❌ ОШИБКА БАЗЫ ДАННЫХ:\n\n";
                errorMsg += $"Сообщение: {dbEx.Message}\n\n";

                if (dbEx.InnerException != null)
                {
                    errorMsg += $"Детали:\n{dbEx.InnerException.Message}\n\n";
                }

                errorMsg += $"Проверьте:\n";
                errorMsg += $"1. PostgreSQL запущен\n";
                errorMsg += $"2. Таблица Equipments существует\n";
                errorMsg += $"3. Права доступа есть";

                MessageBox.Show(errorMsg, "Ошибка сохранения",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Console.WriteLine($"❌ DB ERROR: {dbEx}");
            }
            catch (Exception ex)
            {
                string errorMsg = $"❌ НЕОЖИДАННАЯ ОШИБКА:\n\n{ex.Message}";

                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\nДетали:\n{ex.InnerException.Message}";
                }

                MessageBox.Show(errorMsg, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

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