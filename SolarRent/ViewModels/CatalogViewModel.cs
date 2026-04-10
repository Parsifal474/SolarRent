using CommunityToolkit.Mvvm.Input;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SolarRent.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentService _equipmentService;

        // 🔥 Пагинация
        private const int PageSize = 8;
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

        // 🔥 Пагинация
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
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }  // 🔥 Новая команда редактирования

        public CatalogViewModel(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
            LoadCommand = new RelayCommand(async () => await LoadEquipmentAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            PrevPageCommand = new RelayCommand(PrevPage, () => CanGoPrev);
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);
            DeleteCommand = new RelayCommand<EquipmentItem>(async (item) => await DeleteEquipmentAsync(item));
            EditCommand = new RelayCommand<EquipmentItem>(async (item) => await EditEquipmentAsync(item));  // 🔥 Инициализация

            _ = LoadEquipmentAsync();
        }

        // 🔥 Метод редактирования оборудования
        public async Task EditEquipmentAsync(EquipmentItem item)
        {
            if (item == null) return;

            // Открываем окно редактирования
            var editWindow = new EditEquipmentWindow(_equipmentService, item);

            if (editWindow.ShowDialog() == true)
            {
                // Обновляем текущую страницу после успешного редактирования
                await LoadEquipmentAsync();
            }
        }

        // 🔥 Метод удаления оборудования
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
                var allEquipment = await _equipmentService.GetAvailableAsync();
                _totalCount = allEquipment.Count();

                var page = allEquipment
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
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1;
            IsLoading = true;
            try
            {
                var allEquipment = await _equipmentService.GetAvailableAsync();

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    allEquipment = allEquipment.Where(e =>
                        e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                }

                _totalCount = allEquipment.Count();

                var page = allEquipment
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

    // 🔥 ViewModel-модель для UI с поддержкой команд
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

        // 🔥 Команда редактирования
        public ICommand EditCommand => new RelayCommand(() =>
        {
            _parentViewModel?.EditCommand?.Execute(this);
        });

        // 🔥 Команда удаления
        public ICommand DeleteCommand => new RelayCommand(() =>
        {
            _parentViewModel?.DeleteCommand?.Execute(this);
        });

        // Вычисляемые свойства
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 🔥 RelayCommand с поддержкой параметра
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<object?> _executeWithParam;
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