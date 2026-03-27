using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SolarRent.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EquipmentItem> _equipmentList;

        public ObservableCollection<EquipmentItem> EquipmentList
        {
            get => _equipmentList;
            set
            {
                _equipmentList = value;
                OnPropertyChanged();
            }
        }

        public CatalogViewModel()
        {
            LoadEquipment();
        }

        private void LoadEquipment()
        {
            EquipmentList = new ObservableCollection<EquipmentItem>
            {
                new EquipmentItem
                {
                    Name = "Солнечная панель 300w",
                    Type = "Панель",
                    Power = "300w",
                    PricePerDay = 1500,
                    Deposit = 3000,
                    Status = "В наличии"
                },
                new EquipmentItem
                {
                    Name = "Инвертор 5kW",
                    Type = "Панель",
                    Power = "5kW",
                    PricePerDay = 1500,
                    Deposit = 3000,
                    Status = "В наличии"
                },
                new EquipmentItem
                {
                    Name = "Аккумулятор 10kWh",
                    Type = "Панель",
                    Power = "10kWh",
                    PricePerDay = 1500,
                    Deposit = 3000,
                    Status = "В наличии"
                },
                new EquipmentItem
                {
                    Name = "Солнечная панель 490w",
                    Type = "Панель",
                    Power = "300w",
                    PricePerDay = 1500,
                    Deposit = 3000,
                    Status = "В наличии"
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EquipmentItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Power { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal Deposit { get; set; }
        public string Status { get; set; }
    }
}