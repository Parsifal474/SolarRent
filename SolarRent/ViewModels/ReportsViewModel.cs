using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SolarRent.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

        [ObservableProperty]
        private decimal _totalRevenue;

        [ObservableProperty]
        private int _totalOrders;

        [ObservableProperty]
        private int _newClients;

        [ObservableProperty]
        private decimal _averageCheck;

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
                TotalRevenue = await _reportService.GetTotalRevenueAsync(StartDate, EndDate);
                TotalOrders = await _reportService.GetTotalOrdersAsync(StartDate, EndDate);
                NewClients = await _reportService.GetNewClientsCountAsync(StartDate, EndDate);
                AverageCheck = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;

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
            foreach (var d in debtors.Take(5))
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

        // 🔥 ЭКСПОРТ В CSV (как в ClientsViewModel)
        [RelayCommand]
        private void ExportToCsv()
        {
            if (Debtors == null || Debtors.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Отчёт_SolarRent_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();

                    // ===== ЗАГОЛОВОК ОТЧЁТА =====
                    csv.AppendLine($"ОТЧЁТ ПО АРЕНДЕ ОБОРУДОВАНИЯ");
                    csv.AppendLine($"Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}");
                    csv.AppendLine();

                    // ===== ОСНОВНЫЕ ПОКАЗАТЕЛИ =====
                    csv.AppendLine("ОСНОВНЫЕ ПОКАЗАТЕЛИ");
                    csv.AppendLine($"Общая выручка:;{TotalRevenue:N0} ₽");
                    csv.AppendLine($"Количество заказов:;{TotalOrders}");
                    csv.AppendLine($"Новых клиентов:;{NewClients}");
                    csv.AppendLine($"Средний чек:;{AverageCheck:N0} ₽");
                    csv.AppendLine();
                    csv.AppendLine();

                    // ===== ДОЛЖНИКИ =====
                    csv.AppendLine("ДОЛЖНИКИ");
                    csv.AppendLine("Клиент;Телефон;Заказ №;Сумма долга;Просрочка (дней);Дата возврата");

                    foreach (var debtor in Debtors)
                    {
                        csv.AppendLine($"{Escape(debtor.ClientName)};" +
                                      $"{Escape(debtor.Phone)};" +
                                      $"{debtor.OrderId};" +
                                      $"{debtor.DebtAmount:N0} ₽;" +
                                      $"{debtor.DaysOverdue};" +
                                      $"{debtor.DueDate:dd.MM.yyyy}");
                    }
                    csv.AppendLine();
                    csv.AppendLine();

                    // ===== ЭФФЕКТИВНОСТЬ МЕНЕДЖЕРОВ =====
                    csv.AppendLine("ЭФФЕКТИВНОСТЬ МЕНЕДЖЕРОВ");
                    csv.AppendLine("Менеджер;Заказов;Выручка;Клиентов");

                    foreach (var manager in Managers)
                    {
                        csv.AppendLine($"{Escape(manager.FullName)};" +
                                      $"{manager.OrdersCount};" +
                                      $"{manager.Revenue:N0} ₽;" +
                                      $"{manager.ClientsCount}");
                    }
                    csv.AppendLine();
                    csv.AppendLine();

                    // ===== ПОПУЛЯРНОЕ ОБОРУДОВАНИЕ =====
                    csv.AppendLine("ПОПУЛЯРНОЕ ОБОРУДОВАНИЕ");
                    csv.AppendLine("Название;Тип;Кол-во аренд;Выручка");

                    foreach (var eq in PopularEquipment)
                    {
                        csv.AppendLine($"{Escape(eq.Name)};" +
                                      $"{Escape(eq.TypeDisplay)};" +
                                      $"{eq.RentCount};" +
                                      $"{eq.TotalRevenue:N0} ₽");
                    }

                    File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);

                    MessageBox.Show($"✅ Экспорт успешно завершён.\n\nФайл сохранён:\n{dialog.FileName}",
                        "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Вспомогательный метод для экранирования CSV
        private string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
                return $"\"{input.Replace("\"", "\"\"")}\"";
            return input;
        }

        [RelayCommand]
        private void ShowAllDebtors()
        {
            var allDebtors = Debtors.ToList();
            var window = new Views.AllDebtorsWindow(allDebtors);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }

    // DTO-классы остаются без изменений
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