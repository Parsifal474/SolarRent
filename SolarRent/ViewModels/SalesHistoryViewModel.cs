// ViewModels/SalesHistoryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SolarRent.Models;
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
    public partial class SalesHistoryViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<SaleRecord> _sales = new();

        [ObservableProperty]
        private SaleRecord? _selectedSale;

        [ObservableProperty]
        private decimal _totalSales;

        [ObservableProperty]
        private int _totalCount;
        [ObservableProperty]
        private decimal _averageCheck;

        [ObservableProperty]
        private string _searchQuery = string.Empty;
   

        public SalesHistoryViewModel(ISaleService saleService)
        {
            _saleService = saleService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var sales = await _saleService.GetSalesByDateRangeAsync(StartDate, EndDate);

                Sales.Clear();
                foreach (var sale in sales)
                    Sales.Add(sale);

                TotalSales = await _saleService.GetTotalSalesAsync(StartDate, EndDate);
                TotalCount = Sales.Count;
                AverageCheck = TotalCount > 0 ? TotalSales / TotalCount : 0;

                if (Sales.Count > 0 && SelectedSale == null)
                    SelectedSale = Sales[0];
            }
            finally
            {
                IsLoading = false;
            }
        
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
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                await LoadDataAsync();
                return;
            }

            IsLoading = true;
            try
            {
                var allSales = await _saleService.GetSalesByDateRangeAsync(StartDate, EndDate);
                var filtered = allSales.Where(s =>
                    s.Id.ToString().Contains(SearchQuery) ||
                    (s.Client?.FullName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Client?.Phone?.Contains(SearchQuery) ?? false)
                ).ToList();

                Sales.Clear();
                foreach (var sale in filtered)
                    Sales.Add(sale);

                TotalSales = filtered.Sum(s => s.TotalAmount);
                TotalCount = filtered.Count;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task ViewSaleDetailsAsync(SaleRecord? sale)
        {
            if (sale == null) return;

            var detailsWindow = new Views.SaleDetailsWindow(sale);
            detailsWindow.Owner = Application.Current.MainWindow;
            detailsWindow.ShowDialog();
        }

        [RelayCommand]
        private async Task DeleteSaleAsync(SaleRecord? sale)
        {
            if (sale == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить продажу №{sale.Id}?\n" +
                $"Клиент: {sale.Client?.FullName}\n" +
                $"Сумма: {sale.TotalAmount:N0} ₽",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _saleService.DeleteSaleAsync(sale.Id);
                    await LoadDataAsync();
                    MessageBox.Show("Продажа успешно удалена.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private void ExportToCsv()
        {
            if (Sales.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Sales_History_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"ИСТОРИЯ ПРОДАЖ");
                    csv.AppendLine($"Период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}");
                    csv.AppendLine($"Всего продаж: {TotalCount}");
                    csv.AppendLine($"Общая сумма: {TotalSales:N0} ₽");
                    csv.AppendLine();
                    csv.AppendLine("ID;Дата;Клиент;Телефон;Количество товаров;Сумма;Способ оплаты;Менеджер");

                    foreach (var sale in Sales)
                    {
                        var line = string.Join(";",
                            sale.Id,
                            sale.SaleDate.ToString("dd.MM.yyyy HH:mm"),
                            Escape(sale.Client?.FullName),
                            Escape(sale.Client?.Phone),
                            sale.Items?.Count ?? 0,
                            sale.TotalAmount.ToString("N0") + " ₽",
                            Escape(sale.PaymentMethod),
                            Escape(sale.ManagedBy?.FullName)
                        );
                        csv.AppendLine(line);
                    }

                    File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"✅ Экспорт успешно завершён.\nФайл: {dialog.FileName}",
                        "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
                return $"\"{input.Replace("\"", "\"\"")}\"";
            return input;
        }
    }
}