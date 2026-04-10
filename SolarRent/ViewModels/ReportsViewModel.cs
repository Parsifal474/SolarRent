// ============================================================
// ViewModel: Отчёты и аналитика
// ============================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarRent.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SolarRent.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        // 🔥 Основные показатели
        [ObservableProperty]
        private decimal _totalRevenue;

        [ObservableProperty]
        private int _totalOrders;

        [ObservableProperty]
        private int _newClients;

        [ObservableProperty]
        private decimal _averageCheck;

        // 🔥 Списки для отображения
        [ObservableProperty]
        private ObservableCollection<DebtorInfo> _debtors = new();

        [ObservableProperty]
        private ObservableCollection<ManagerStats> _managers = new();

        [ObservableProperty]
        private ObservableCollection<EquipmentPopularity> _popularEquipment = new();

        public ReportsViewModel(IReportService reportService)
        {
            _reportService = reportService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // Загружаем основные показатели
                TotalRevenue = await _reportService.GetTotalRevenueAsync(StartDate, EndDate);
                TotalOrders = await _reportService.GetTotalOrdersAsync(StartDate, EndDate);
                NewClients = await _reportService.GetNewClientsCountAsync(StartDate, EndDate);
                AverageCheck = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;

                // Загружаем списки
                await LoadDebtorsAsync();
                await LoadManagersAsync();
                await LoadPopularEquipmentAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDebtorsAsync()
        {
            var debtors = await _reportService.GetDebtorsAsync();
            Debtors.Clear();
            foreach (var d in debtors.Take(5)) // Показываем топ-5
                Debtors.Add(d);
        }

        private async Task LoadManagersAsync()
        {
            var managers = await _reportService.GetManagerStatsAsync(StartDate, EndDate);
            Managers.Clear();
            foreach (var m in managers.OrderByDescending(m => m.Revenue).Take(5))
                Managers.Add(m);
        }

        private async Task LoadPopularEquipmentAsync()
        {
            var popular = await _reportService.GetPopularEquipmentAsync(StartDate, EndDate, 5);
            PopularEquipment.Clear();
            foreach (var p in popular)
                PopularEquipment.Add(p);
        }

        [RelayCommand]
        private async Task ApplyDateFilterAsync()
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await LoadDataAsync();
        }

        [RelayCommand]
        private void ExportToExcel()
        {
            // TODO: Реализовать экспорт в Excel
            MessageBox.Show("Экспорт в Excel будет добавлен в следующей версии",
                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ShowAllDebtors()
        {
            var window = new Views.AllDebtorsWindow(Debtors.ToList());
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }

    // 🔥 DTO-классы для отчётов
    public class DebtorInfo
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal DebtAmount { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime DueDate { get; set; }
        public string Phone { get; set; } = string.Empty;
    }

    public class ManagerStats
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
        public int ClientsCount { get; set; }
    }

    public class EquipmentPopularity
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeDisplay { get; set; } = string.Empty;
        public int RentCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}