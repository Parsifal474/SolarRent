using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Data;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent.Views.Pages
{
    public partial class Lease_issue : Page
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private Equipment? _selectedEquipment;
        private Client? _selectedClient;

        public Lease_issue()
        {
            InitializeComponent();
            _context = App.Services.GetRequiredService<AppDbContext>();
            _authService = App.Services.GetRequiredService<IAuthService>();

            Loaded += async (s, e) => await LoadDataAsync();

            dpStartDate.SelectedDate = DateTime.Today;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Загружаем клиентов
                var clients = await _context.Clients
                    .OrderBy(c => c.FullName)
                    .ToListAsync();

                cmbClient.Items.Clear();
                foreach (var client in clients)
                {
                    cmbClient.Items.Add(client);
                }

                // Загружаем доступное оборудование (только InStock)
                var equipment = await _context.Equipments
                    .Where(e => e.Status == "InStock")
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                cmbEquipment.Items.Clear();
                foreach (var eq in equipment)
                {
                    cmbEquipment.Items.Add(eq);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClient = cmbClient.SelectedItem as Client;

            // Проверка чёрного списка
            if (_selectedClient != null && _selectedClient.IsBlacklisted)
            {
                warningBlacklist.Visibility = Visibility.Visible;
                btnCreateRental.IsEnabled = false;
            }
            else
            {
                warningBlacklist.Visibility = Visibility.Collapsed;
                btnCreateRental.IsEnabled = true;
            }
        }

        private void CmbEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEquipment = cmbEquipment.SelectedItem as Equipment;
            UpdatePriceInfo();
            CalculateTotal();
        }

        private void UpdatePriceInfo()
        {
            if (_selectedEquipment != null)
            {
                lblPricePerDay.Text = $"₽ {_selectedEquipment.RentalPricePerDay:N0}";
                lblDeposit.Text = $"₽ {_selectedEquipment.DepositAmount:N0}";
            }
            else
            {
                lblPricePerDay.Text = "₽ 0";
                lblDeposit.Text = "₽ 0";
            }
        }

        private void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateEndDate();
            CalculateTotal();
        }

        private void TxtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateEndDate();
            CalculateTotal();
        }

        private void TxtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateEndDate()
        {
            if (dpStartDate.SelectedDate.HasValue && int.TryParse(txtDays.Text, out int days) && days > 0)
            {
                DateTime endDate = dpStartDate.SelectedDate.Value.AddDays(days);
                txtEndDate.Text = endDate.ToString("dd.MM.yyyy");
            }
            else
            {
                txtEndDate.Text = "";
            }
        }

        private void CalculateTotal()
        {
            if (_selectedEquipment == null)
            {
                lblTotal.Text = "₽ 0";
                return;
            }

            if (!int.TryParse(txtDays.Text, out int days) || days <= 0)
            {
                lblTotal.Text = "₽ 0";
                return;
            }

            decimal total = _selectedEquipment.RentalPricePerDay * days;

            // Применяем скидку
            if (decimal.TryParse(txtDiscount.Text, out decimal discount) && discount > 0 && discount <= 100)
            {
                total = total - (total * discount / 100);
            }

            lblTotal.Text = $"₽ {total:N0}";
        }

        private async void CreateRentalButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (_selectedClient == null)
            {
                ShowStatusMessage("❌ Выберите клиента", true);
                cmbClient.Focus();
                return;
            }

            if (_selectedClient.IsBlacklisted)
            {
                ShowStatusMessage("❌ Клиент в чёрном списке! Выдача аренды невозможна.", true);
                return;
            }

            if (_selectedEquipment == null)
            {
                ShowStatusMessage("❌ Выберите оборудование", true);
                cmbEquipment.Focus();
                return;
            }

            if (!int.TryParse(txtDays.Text, out int days) || days <= 0)
            {
                ShowStatusMessage("❌ Введите корректное количество дней", true);
                txtDays.Focus();
                return;
            }

            if (!dpStartDate.SelectedDate.HasValue)
            {
                ShowStatusMessage("❌ Выберите дату начала", true);
                dpStartDate.Focus();
                return;
            }

            var startDate = dpStartDate.SelectedDate.Value;
            var endDate = startDate.AddDays(days);

            // Проверка на прошедшую дату
            if (startDate.Date < DateTime.Today)
            {
                ShowStatusMessage("❌ Дата начала не может быть раньше сегодняшнего дня", true);
                return;
            }

            // Проверка на пересечение с другими арендами
            bool isOverlapping = await _context.RentalOrders
                .AnyAsync(r => r.EquipmentId == _selectedEquipment.Id &&
                               r.Status == "Active" &&
                               r.StartDate < endDate &&
                               r.EndDate > startDate);

            if (isOverlapping)
            {
                ShowStatusMessage($"❌ Оборудование \"{_selectedEquipment.Name}\" уже занято на выбранные даты", true);
                return;
            }

            // Блокируем UI
            btnCreateRental.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            try
            {
                // Расчёт суммы со скидкой
                decimal totalPrice = _selectedEquipment.RentalPricePerDay * days;
                if (decimal.TryParse(txtDiscount.Text, out decimal discount) && discount > 0 && discount <= 100)
                {
                    totalPrice = totalPrice - (totalPrice * discount / 100);
                }

                var rentalOrder = new RentalOrder
                {
                    ClientId = _selectedClient.Id,
                    Client = _selectedClient,
                    EquipmentId = _selectedEquipment.Id,
                    Equipment = _selectedEquipment,
                    StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                    RentalPrice = totalPrice,
                    Deposit = _selectedEquipment.DepositAmount,
                    Penalty = 0,
                    Status = "Active",
                    ManagedByUserId = _authService.CurrentUser?.Id,
                    ManagedBy = _authService.CurrentUser,
                    ActualReturnDate = null
                };

                // Обновляем статус оборудования
                _selectedEquipment.Status = "Rented";
                _context.Equipments.Update(_selectedEquipment);

                // Сохраняем заказ
                await _context.RentalOrders.AddAsync(rentalOrder);
                await _context.SaveChangesAsync();

                MessageBox.Show(
                    $"✅ Аренда успешно оформлена!\n\n" +
                    $"Заказ №{rentalOrder.Id}\n" +
                    $"Клиент: {_selectedClient.FullName}\n" +
                    $"Оборудование: {_selectedEquipment.Name}\n" +
                    $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}\n" +
                    $"Сумма: {totalPrice:N0} ₽\n" +
                    $"Залог: {_selectedEquipment.DepositAmount:N0} ₽",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Сбрасываем форму
                ResetForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"❌ Ошибка при сохранении: {ex.Message}", true);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreateRental.IsEnabled = true;
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetForm()
        {
            cmbClient.SelectedItem = null;
            cmbEquipment.SelectedItem = null;
            txtDays.Text = "";
            txtDiscount.Text = "";
            txtNote.Text = "";
            dpStartDate.SelectedDate = DateTime.Today;
            _selectedClient = null;
            _selectedEquipment = null;
            txtStatusMessage.Visibility = Visibility.Collapsed;
            warningBlacklist.Visibility = Visibility.Collapsed;
            lblTotal.Text = "₽ 0";
            lblPricePerDay.Text = "₽ 0";
            lblDeposit.Text = "₽ 0";
        }

        private void ShowStatusMessage(string message, bool isError = false)
        {
            txtStatusMessage.Text = message;
            txtStatusMessage.Foreground = isError ?
                (System.Windows.Media.Brush)FindResource("DangerRed") :
                (System.Windows.Media.Brush)FindResource("WarningOrange");
            txtStatusMessage.Visibility = Visibility.Visible;

            // Скрываем сообщение через 5 секунд
            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (s, e) => Dispatcher.Invoke(() =>
            {
                txtStatusMessage.Visibility = Visibility.Collapsed;
                timer.Stop();
                timer.Dispose();
            });
            timer.Start();
        }
    }
}