using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;  // ← ВАЖНО: именно этот using
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SolarRent.ViewModels
{
    public partial class LeaseAcceptanceViewModel : ObservableObject
    {
        private readonly AppDbContext _context;

        [ObservableProperty]
        private string _searchOrderNumber = string.Empty;

        [ObservableProperty]
        private RentalOrder? _currentOrder;

        [ObservableProperty]
        private ObservableCollection<ReturnEquipmentItem> _equipmentItems = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasOrder;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private decimal _totalPenalty;

        // Используем IRelayCommand из CommunityToolkit
        public IRelayCommand SearchOrderCommand { get; }
        public IRelayCommand AcceptReturnCommand { get; }

        public LeaseAcceptanceViewModel(AppDbContext context)
        {
            _context = context;

            // Используем CommunityToolkit.Mvvm.Input.RelayCommand
            SearchOrderCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(async () => await SearchOrderAsync());
            AcceptReturnCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(async () => await AcceptReturnAsync(), () => HasOrder && !IsLoading);
        }

        private async Task SearchOrderAsync()
        {
            if (!int.TryParse(SearchOrderNumber, out int orderId))
            {
                StatusMessage = "❌ Введите корректный номер заказа";
                return;
            }

            IsLoading = true;
            StatusMessage = "🔍 Поиск заказа...";

            try
            {
                var order = await _context.RentalOrders
                    .Include(o => o.Client)
                    .Include(o => o.Equipment)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    StatusMessage = $"❌ Заказ #{orderId} не найден";
                    HasOrder = false;
                    CurrentOrder = null;
                    EquipmentItems.Clear();
                    return;
                }

                if (order.Status != "Active")
                {
                    StatusMessage = $"⚠️ Заказ #{orderId} не активен (статус: {order.Status}). Возврат невозможен.";
                    HasOrder = false;
                    CurrentOrder = null;
                    EquipmentItems.Clear();
                    return;
                }

                CurrentOrder = order;
                HasOrder = true;

                EquipmentItems.Clear();
                EquipmentItems.Add(new ReturnEquipmentItem
                {
                    EquipmentId = order.EquipmentId,
                    EquipmentName = order.Equipment?.Name ?? "Оборудование",
                    SerialNumber = GenerateSerialNumber(order),
                    ConditionAtIssue = "Отличное",
                    ConditionAtReturn = "Отличное",
                    IsDamaged = false,
                    DamageDescription = ""
                });

                CalculatePenalty();

                StatusMessage = $"✅ Заказ #{orderId} найден. Проверьте состояние оборудования.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GenerateSerialNumber(RentalOrder order)
        {
            return $"SN-{order.Id}-{order.StartDate:yyyyMMdd}";
        }

        private void CalculatePenalty()
        {
            if (CurrentOrder == null) return;

            var today = DateTime.UtcNow.Date;
            var endDate = CurrentOrder.EndDate.Date;

            if (today > endDate)
            {
                int daysOverdue = (today - endDate).Days;
                TotalPenalty = CurrentOrder.RentalPrice * 0.05m * daysOverdue;
                StatusMessage += $" ⚠️ Просрочка: {daysOverdue} дн. Пеня: {TotalPenalty:N0} ₽";
            }
            else
            {
                TotalPenalty = 0;
            }
        }

        private async Task AcceptReturnAsync()
        {
            if (CurrentOrder == null) return;

            var damagedItems = EquipmentItems.Where(i => i.IsDamaged).ToList();
            if (damagedItems.Any())
            {
                var result = MessageBox.Show(
                    $"Обнаружены повреждения оборудования:\n{string.Join("\n", damagedItems.Select(i => $"• {i.EquipmentName}: {i.DamageDescription}"))}\n\n" +
                    $"Списать залог? Сумма залога: {CurrentOrder.Deposit:N0} ₽",
                    "Повреждения оборудования",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentOrder.Deposit = 0;
                }
            }

            IsLoading = true;

            try
            {
                CurrentOrder.Status = "Returned";
                CurrentOrder.ActualReturnDate = DateTime.UtcNow;
                CurrentOrder.Penalty = TotalPenalty;

                var equipment = await _context.Equipments.FindAsync(CurrentOrder.EquipmentId);
                if (equipment != null)
                {
                    equipment.Status = "InStock";
                    _context.Equipments.Update(equipment);
                }

                foreach (var item in EquipmentItems.Where(i => i.IsDamaged))
                {
                    var defectRecord = new DefectRecord
                    {
                        EquipmentId = item.EquipmentId,
                        CheckDate = DateTime.UtcNow,
                        Description = item.DamageDescription,
                        Resolution = "Repair"
                    };
                    await _context.DefectRecords.AddAsync(defectRecord);
                }

                _context.RentalOrders.Update(CurrentOrder);
                await _context.SaveChangesAsync();

                MessageBox.Show(
                    $"✅ Возврат по заказу #{CurrentOrder.Id} оформлен!\n\n" +
                    $"Клиент: {CurrentOrder.Client?.FullName}\n" +
                    $"Оборудование: {CurrentOrder.Equipment?.Name}\n" +
                    $"Штраф: {TotalPenalty:N0} ₽\n" +
                    $"Залог: {CurrentOrder.Deposit:N0} ₽",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ResetForm();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка при сохранении: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetForm()
        {
            SearchOrderNumber = string.Empty;
            CurrentOrder = null;
            HasOrder = false;
            EquipmentItems.Clear();
            Notes = string.Empty;
            TotalPenalty = 0;
            StatusMessage = string.Empty;
        }
    }

    public partial class ReturnEquipmentItem : ObservableObject
    {
        [ObservableProperty]
        private int _equipmentId;

        [ObservableProperty]
        private string _equipmentName = string.Empty;

        [ObservableProperty]
        private string _serialNumber = string.Empty;

        [ObservableProperty]
        private string _conditionAtIssue = string.Empty;

        [ObservableProperty]
        private string _conditionAtReturn = string.Empty;

        [ObservableProperty]
        private bool _isDamaged;

        [ObservableProperty]
        private string _damageDescription = string.Empty;

        partial void OnConditionAtReturnChanged(string value)
        {
            IsDamaged = value == "Повреждено";
        }
    }
}