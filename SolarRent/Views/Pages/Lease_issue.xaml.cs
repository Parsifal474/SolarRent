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
            if (_selectedClient == null) { /* ... */ return; }
            if (_selectedClient.IsBlacklisted) { /* ... */ return; }
            if (_selectedEquipment == null) { /* ... */ return; }
            if (!int.TryParse(txtDays.Text, out int days) || days <= 0) { /* ... */ return; }
            if (!dpStartDate.SelectedDate.HasValue) { /* ... */ return; }

            // Получаем локальные даты
            DateTime startDateLocal = dpStartDate.SelectedDate.Value;
            DateTime endDateLocal = startDateLocal.AddDays(days);

            // Преобразуем в UTC
            DateTime startDateUtc = startDateLocal.ToUniversalTime();
            DateTime endDateUtc = endDateLocal.ToUniversalTime();

            // Проверка на прошедшую дату (сравниваем в UTC)
            if (startDateLocal.Date < DateTime.UtcNow.Date)
            {
                ShowStatusMessage("❌ Дата начала не может быть раньше сегодняшнего дня", true);
                return;
            }

            // Проверка пересечения (сравниваем UTC)
            bool isOverlapping = await _context.RentalOrders
                .AnyAsync(r => r.EquipmentId == _selectedEquipment.Id &&
                               r.Status == "Active" &&
                               r.StartDate < endDateUtc &&
                               r.EndDate > startDateUtc);

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
                // Базовая стоимость
                decimal totalPrice = _selectedEquipment.RentalPricePerDay * days;

                // Применяем скидку из поля txtDiscount
                if (decimal.TryParse(txtDiscount.Text, out decimal discountPercent) && discountPercent > 0 && discountPercent <= 100)
                {
                    totalPrice -= totalPrice * discountPercent / 100;
                }

                var rentalOrder = new RentalOrder
                {
                    ClientId = _selectedClient.Id,
                    EquipmentId = _selectedEquipment.Id,
                    StartDate = startDateUtc,        // UTC
                    EndDate = endDateUtc,            // UTC
                    RentalPrice = totalPrice,
                    Deposit = _selectedEquipment.DepositAmount,
                    Penalty = 0,
                    Status = "Active",
                    ManagedByUserId = _authService.CurrentUser?.Id
                };

                // Обновляем статус оборудования
                _selectedEquipment.Status = "Rented";
                _context.Equipments.Update(_selectedEquipment);

                await _context.RentalOrders.AddAsync(rentalOrder);
                await _context.SaveChangesAsync();

                MessageBox.Show($"✅ Аренда оформлена!\nЗаказ №{rentalOrder.Id}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"❌ Ошибка: {ex.Message}", true);
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