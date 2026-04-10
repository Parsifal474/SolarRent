using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SolarRent.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentService _equipmentService;

        // Пагинация
        private const int PageSize = 8;
        private int _currentPage = 1;
        private int _totalCount = 0;

        // Кэш всех данных
        private IEnumerable<Equipment>? _allEquipmentCache;
        private IEnumerable<Equipment>? _filteredEquipmentCache;

        // Данные
        private ObservableCollection<EquipmentItem> _equipmentList = new();
        private string _searchQuery = string.Empty;
        private bool _isLoading;

        // 🔥 Свойства для фильтрации
        private EquipmentType? _selectedType;
        private double? _maxPower;
        private decimal? _maxPrice;
        private string _selectedStatus = "Все";
        private bool _isFilterPanelVisible;

        public ObservableCollection<EquipmentItem> EquipmentList
        {
            get => _equipmentList;
            set { _equipmentList = value; OnPropertyChanged(); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanGoPrev)); OnPropertyChanged(nameof(CanGoNext)); OnPropertyChanged(nameof(PageInfo)); }
        }

        public int TotalPages => (_totalCount + PageSize - 1) / PageSize;
        public bool CanGoPrev => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;
        public string PageInfo => $"Страница {CurrentPage} из {TotalPages} (всего: {_totalCount})";

        // 🔥 Свойства фильтрации
        public EquipmentType? SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilterButtonText)); }
        }

        public double? MaxPower
        {
            get => _maxPower;
            set { _maxPower = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilterButtonText)); }
        }

        public decimal? MaxPrice
        {
            get => _maxPrice;
            set { _maxPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilterButtonText)); }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilterButtonText)); }
        }

        public bool IsFilterPanelVisible
        {
            get => _isFilterPanelVisible;
            set { _isFilterPanelVisible = value; OnPropertyChanged(); }
        }

        public string FilterButtonText => IsFilterPanelVisible ? "▲ Скрыть фильтры" : "▼ Показать фильтры";

        // Список статусов для ComboBox
        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string>
        {
            "Все", "В наличии", "В аренде", "На ремонте", "Списано"
        };

        // Список типов для ComboBox
        public ObservableCollection<KeyValuePair<EquipmentType?, string>> Types { get; } = new ObservableCollection<KeyValuePair<EquipmentType?, string>>
        {
            new KeyValuePair<EquipmentType?, string>(null, "Все типы"),
            new KeyValuePair<EquipmentType?, string>(EquipmentType.Panel, "Солнечные панели"),
            new KeyValuePair<EquipmentType?, string>(EquipmentType.Inverter, "Инверторы"),
            new KeyValuePair<EquipmentType?, string>(EquipmentType.Battery, "Аккумуляторы"),
            new KeyValuePair<EquipmentType?, string>(EquipmentType.Accessory, "Комплектующие")
        };

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        // 🔥 Команды фильтрации
        public ICommand ToggleFilterPanelCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        public CatalogViewModel(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;

            LoadCommand = new RelayCommand(async () => await LoadEquipmentAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            PrevPageCommand = new RelayCommand(PrevPage, () => CanGoPrev);
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);
            DeleteCommand = new RelayCommand<EquipmentItem>(async (item) => await DeleteEquipmentAsync(item));
            ExportToCsvCommand = new RelayCommand(ExportToCsv);

            // 🔥 Инициализация команд фильтрации
            ToggleFilterPanelCommand = new RelayCommand(ToggleFilterPanel);
            ApplyFiltersCommand = new RelayCommand(async () => await ApplyFiltersAsync());
            ResetFiltersCommand = new RelayCommand(async () => await ResetFiltersAsync());

            _ = LoadEquipmentAsync();
        }

        // 🔥 Показать/скрыть панель фильтров
        private void ToggleFilterPanel()
        {
            IsFilterPanelVisible = !IsFilterPanelVisible;
        }

        // 🔥 Применить фильтры
        private async Task ApplyFiltersAsync()
        {
            IsLoading = true;
            try
            {
                // Получаем все оборудование
                var allEquipment = await _equipmentService.GetAvailableAsync();
                _allEquipmentCache = allEquipment;

                // Применяем фильтры
                var filtered = allEquipment.AsEnumerable();

                // Фильтр по типу
                if (SelectedType.HasValue)
                {
                    filtered = filtered.Where(e => e.Type == SelectedType.Value);
                }

                // Фильтр по максимальной мощности
                if (MaxPower.HasValue && MaxPower.Value > 0)
                {
                    filtered = filtered.Where(e => e.Power <= MaxPower.Value);
                }

                // Фильтр по максимальной цене
                if (MaxPrice.HasValue && MaxPrice.Value > 0)
                {
                    filtered = filtered.Where(e => e.Price <= MaxPrice.Value);
                }

                // Фильтр по статусу
                if (SelectedStatus != "Все")
                {
                    string statusMap = SelectedStatus switch
                    {
                        "В наличии" => "InStock",
                        "В аренде" => "Rented",
                        "На ремонте" => "Repair",
                        "Списано" => "Disposed",
                        _ => SelectedStatus
                    };
                    filtered = filtered.Where(e => e.Status == statusMap);
                }

                // Фильтр по поисковому запросу
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    filtered = filtered.Where(e =>
                        e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                }

                _filteredEquipmentCache = filtered;
                _totalCount = _filteredEquipmentCache.Count();
                CurrentPage = 1;

                UpdateEquipmentList();

                if (_filteredEquipmentCache.Any())
                {
                    MessageBox.Show($"🔍 Найдено оборудований: {_totalCount}",
                        "Фильтрация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("🔍 По заданным критериям ничего не найдено",
                        "Фильтрация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 🔥 Сбросить фильтры
        private async Task ResetFiltersAsync()
        {
            SelectedType = null;
            MaxPower = null;
            MaxPrice = null;
            SelectedStatus = "Все";
            SearchQuery = string.Empty;

            await LoadEquipmentAsync();

            MessageBox.Show("✅ Все фильтры сброшены", "Фильтры",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateEquipmentList()
        {
            var source = _filteredEquipmentCache ?? _allEquipmentCache;
            if (source == null) return;

            var page = source
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            EquipmentList.Clear();
            foreach (var eq in page)
            {
                EquipmentList.Add(new EquipmentItem(eq, this));
            }

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfo));
        }

        private void ExportToCsv()
        {
            var source = _filteredEquipmentCache ?? _allEquipmentCache;

            if (source == null || !source.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Equipment_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID,Название,Тип,Мощность,Базовая цена (₽),Аренда/день (₽),Залог (₽),Статус,Описание");

                    foreach (var equipment in source)
                    {
                        string Escape(string? input) => input?.Contains(',') == true || input?.Contains('"') == true
                            ? $"\"{input?.Replace("\"", "\"\"")}\""
                            : input ?? "";

                        string typeDisplay = equipment.Type switch
                        {
                            EquipmentType.Panel => "Солнечная панель",
                            EquipmentType.Inverter => "Инвертор",
                            EquipmentType.Battery => "Аккумулятор",
                            EquipmentType.Accessory => "Комплектующее",
                            _ => equipment.Type.ToString()
                        };

                        string statusDisplay = equipment.Status switch
                        {
                            "InStock" => "В наличии",
                            "Rented" => "В аренде",
                            "Repair" => "На ремонте",
                            "Disposed" => "Списано",
                            _ => equipment.Status
                        };

                        string powerDisplay = equipment.Type switch
                        {
                            EquipmentType.Battery => $"{equipment.Power} кВт·ч",
                            _ => $"{equipment.Power} кВт"
                        };

                        var line = string.Join(",",
                            equipment.Id,
                            Escape(equipment.Name),
                            Escape(typeDisplay),
                            Escape(powerDisplay),
                            equipment.Price.ToString("N0"),
                            (equipment.Price * 0.01m).ToString("N0"),
                            (equipment.Price * 0.5m).ToString("N0"),
                            Escape(statusDisplay),
                            Escape(equipment.Description)
                        );
                        csv.AppendLine(line);
                    }

                    File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"✅ Экспорт успешно завершён!\n\nФайл сохранён: {dialog.FileName}\n\n" +
                        $"📊 Экспортировано записей: {source.Count()}",
                        "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeleteEquipmentAsync(EquipmentItem item)
        {
            if (item == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить '{item.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _equipmentService.DeleteAsync(item.Id);
                await LoadEquipmentAsync();
                MessageBox.Show("✅ Оборудование удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEquipmentAsync()
        {
            IsLoading = true;
            try
            {
                _allEquipmentCache = await _equipmentService.GetAvailableAsync();
                _filteredEquipmentCache = null;
                _totalCount = _allEquipmentCache.Count();
                CurrentPage = 1;
                UpdateEquipmentList();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            await ApplyFiltersAsync();
        }

        private void PrevPage()
        {
            if (CanGoPrev)
            {
                CurrentPage--;
                UpdateEquipmentList();
            }
        }

        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                UpdateEquipmentList();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // EquipmentItem и RelayCommand остаются без изменений...
    public class EquipmentItem : INotifyPropertyChanged
    {
        private readonly CatalogViewModel _parentViewModel;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EquipmentType Type { get; set; }
        public double Power { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }

        public EquipmentItem(Equipment equipment, CatalogViewModel parentViewModel)
        {
            Id = equipment.Id;
            Name = equipment.Name;
            Type = equipment.Type;
            Power = equipment.Power;
            Price = equipment.Price;
            Status = equipment.Status;
            Description = equipment.Description;
            _parentViewModel = parentViewModel;
        }

        public EquipmentItem() { }

        public ICommand DeleteCommand => new RelayCommand(() =>
        {
            _parentViewModel?.DeleteCommand?.Execute(this);
        });

        public string TypeDisplay => Type switch
        {
            EquipmentType.Panel => "🔆 Панель",
            EquipmentType.Inverter => "⚡ Инвертор",
            EquipmentType.Battery => "🔋 Аккумулятор",
            EquipmentType.Accessory => "🔧 Комплектующее",
            _ => Type.ToString()
        };

        public string PowerDisplay => Type switch
        {
            EquipmentType.Panel or EquipmentType.Inverter => $"{Power} кВт",
            EquipmentType.Battery => $"{Power} кВт·ч",
            _ => $"{Power} кВт"
        };

        public decimal PricePerDay => Price * 0.01m;
        public decimal Deposit => Price * 0.5m;
        public string StatusDisplay => Status == "InStock" ? "В наличии" : Status;
        public string DisplayName => $"{Name} ({PowerDisplay})";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<object?>? _executeWithParam;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object?> executeWithParam, Func<bool>? canExecute = null)
        {
            _executeWithParam = executeWithParam ?? throw new ArgumentNullException(nameof(executeWithParam));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
        {
            if (_executeWithParam != null)
                _executeWithParam(parameter);
            else
                _execute();
        }
    }
}