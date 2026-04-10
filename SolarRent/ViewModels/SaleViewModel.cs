using CommunityToolkit.Mvvm.Input;
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
    public class SaleViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentService _equipmentService;

        // 🔥 Пагинация
        private const int PageSize = 8;
        private int _currentPage = 1;
        private int _totalCount = 0;

        // Данные
        private ObservableCollection<SaleItem> _equipmentList = new();
        private ObservableCollection<SaleItem> _cart = new();
        private string _searchQuery = string.Empty;
        private EquipmentType? _selectedType;

        // 🔥 Свойства для привязки
        public ObservableCollection<SaleItem> EquipmentList
        {
            get => _equipmentList;
            set { _equipmentList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SaleItem> Cart
        {
            get => _cart;
            set { _cart = value; OnPropertyChanged(); OnPropertyChanged(nameof(CartTotal)); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); }
        }

        public EquipmentType? SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
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
        public string PageInfo => $"Страница {CurrentPage} из {TotalPages}";

        // 🔥 Итого в корзине
        public decimal CartTotal => Cart.Sum(item => item.Price * item.Quantity);

        // 🔥 Команды
        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand CheckoutCommand { get; }

        public SaleViewModel(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
            LoadCommand = new RelayCommand(async () => await LoadEquipmentAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            AddToCartCommand = new RelayCommand<SaleItem>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<SaleItem>(RemoveFromCart);
            PrevPageCommand = new RelayCommand(PrevPage, () => CanGoPrev);
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);
            CheckoutCommand = new RelayCommand(Checkout);

            _ = LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            var allEquipment = await _equipmentService.GetAvailableAsync();
            _totalCount = allEquipment.Count();

            var page = allEquipment
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            EquipmentList.Clear();
            foreach (var eq in page)
            {
                EquipmentList.Add(new SaleItem(eq));
            }

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfo));
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1;
            var allEquipment = await _equipmentService.GetAvailableAsync();

            // Фильтрация по поиску
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                allEquipment = allEquipment.Where(e =>
                    e.Name.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по типу
            if (SelectedType.HasValue)
            {
                allEquipment = allEquipment.Where(e => e.Type == SelectedType.Value);
            }

            _totalCount = allEquipment.Count();

            var page = allEquipment
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            EquipmentList.Clear();
            foreach (var eq in page)
            {
                EquipmentList.Add(new SaleItem(eq));
            }

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfo));
        }

        // 🔥 Добавить в корзину
        private void AddToCart(SaleItem item)
        {
            if (item == null) return;

            var existing = Cart.FirstOrDefault(c => c.Id == item.Id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                Cart.Add(new SaleItem(item) { Quantity = 1 });
            }

            OnPropertyChanged(nameof(CartTotal));
        }

        // 🔥 Удалить из корзины
        private void RemoveFromCart(SaleItem item)
        {
            if (item == null) return;

            var existing = Cart.FirstOrDefault(c => c.Id == item.Id);
            if (existing != null)
            {
                Cart.Remove(existing);
                OnPropertyChanged(nameof(CartTotal));
            }
        }

        // 🔥 Оформление продажи
        private void Checkout()
        {
            if (Cart.Count == 0)
            {
                System.Windows.MessageBox.Show("Корзина пуста!", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Оформить продажу на сумму {CartTotal:N0} ₽?\n\nТоваров: {Cart.Count}",
                "Подтверждение",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // TODO: Сохранить продажу в БД
                System.Windows.MessageBox.Show($"✅ Продажа оформлена!\nСумма: {CartTotal:N0} ₽", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                Cart.Clear();
                OnPropertyChanged(nameof(CartTotal));
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

    // 🔥 Модель для UI продажи
    public class SaleItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EquipmentType Type { get; set; }
        public double Power { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }

        // 🔥 Для корзины
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public SaleItem(Equipment equipment)
        {
            Id = equipment.Id;
            Name = equipment.Name;
            Type = equipment.Type;
            Power = equipment.Power;
            Price = equipment.Price;
            Status = equipment.Status;
            Description = equipment.Description;
        }

        // Копирование для корзины
        public SaleItem(SaleItem other)
        {
            Id = other.Id;
            Name = other.Name;
            Type = other.Type;
            Power = other.Power;
            Price = other.Price;
            Status = other.Status;
            Description = other.Description;
            Quantity = other.Quantity;
        }

        public SaleItem() { }

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

        public string StatusDisplay => Status == "InStock" ? "В наличии" : "Нет в наличии";
        public string DisplayName => $"{Name} ({PowerDisplay})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
    
}