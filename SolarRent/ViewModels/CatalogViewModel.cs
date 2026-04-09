using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SolarRent.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentService _equipmentService;

        // 🔥 Пагинация
        private const int PageSize = 8;  // ← 8 карточек на странице!
        private int _currentPage = 1;
        private int _totalCount = 0;

        // Данные
        private ObservableCollection<EquipmentItem> _equipmentList = new();
        private string _searchQuery = string.Empty;
        private bool _isLoading;

        // 🔥 Свойства для привязки
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

        // 🔥 Пагинация: свойства для UI
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanGoPrev)); OnPropertyChanged(nameof(CanGoNext)); OnPropertyChanged(nameof(PageInfo)); }
        }

        public int TotalPages => (_totalCount + PageSize - 1) / PageSize;

        public bool CanGoPrev => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public string PageInfo => $"Страница {CurrentPage} из {TotalPages} (всего: {_totalCount})";

        // 🔥 Команды
        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public CatalogViewModel(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
            LoadCommand = new RelayCommand(async () => await LoadEquipmentAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            PrevPageCommand = new RelayCommand(PrevPage, () => CanGoPrev);
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);

            _ = LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            IsLoading = true;
            try
            {
                // 🔥 Загружаем ВСЕ доступные (для простоты), потом пагинируем в памяти
                // Для продакшена лучше делать пагинацию на уровне БД
                var allEquipment = await _equipmentService.GetAvailableAsync();
                _totalCount = allEquipment.Count();

                // 🔥 Пагинация: Skip + Take
                var page = allEquipment
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize);

                EquipmentList.Clear();
                foreach (var eq in page)
                {
                    EquipmentList.Add(new EquipmentItem(eq));
                }

                // 🔥 Обновляем свойства пагинации
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(PageInfo));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1; // Сброс на первую страницу при поиске
            IsLoading = true;
            try
            {
                var allEquipment = await _equipmentService.GetAvailableAsync();

                // Фильтрация
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    allEquipment = allEquipment.Where(e =>
                        e.Name.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase));
                }

                _totalCount = allEquipment.Count();

                // Пагинация
                var page = allEquipment
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize);

                EquipmentList.Clear();
                foreach (var eq in page)
                {
                    EquipmentList.Add(new EquipmentItem(eq));
                }

                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(PageInfo));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PrevPage()
        {
            if (CanGoPrev)
            {
                CurrentPage--;
                _ = LoadEquipmentAsync();
            }
        }

        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                _ = LoadEquipmentAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel-модель для UI
    public class EquipmentItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EquipmentType Type { get; set; }
        public double Power { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }

        public EquipmentItem(Equipment equipment)
        {
            Id = equipment.Id;
            Name = equipment.Name;
            Type = equipment.Type;
            Power = equipment.Power;
            Price = equipment.Price;
            Status = equipment.Status;
            Description = equipment.Description;
        }

        public EquipmentItem() { }

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
    }

    // Простая реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }
}