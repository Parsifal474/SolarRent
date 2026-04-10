using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarRent.Models;
using SolarRent.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SolarRent.ViewModels
{
    public partial class ClientsViewModel : ObservableObject
    {
        private readonly IClientService _clientService;

        [ObservableProperty]
        private ObservableCollection<Client> _clients = new();

        [ObservableProperty]
        private Client? _selectedClient;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        // Статистика
        [ObservableProperty]
        private int _totalOrders;

        [ObservableProperty]
        private decimal _totalRevenue;

        [ObservableProperty]
        private int _overdueCount;

        public ClientsViewModel(IClientService clientService)
        {
            _clientService = clientService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                Clients.Clear();
                foreach (var c in clients)
                    Clients.Add(c);

                TotalOrders = await _clientService.GetTotalOrdersCountAsync();
                TotalRevenue = await _clientService.GetTotalRevenueAsync();
                OverdueCount = await _clientService.GetOverdueCountAsync();

                if (Clients.Count > 0 && SelectedClient == null)
                    SelectedClient = Clients[0];
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            IsLoading = true;
            try
            {
                var clients = await _clientService.SearchClientsAsync(SearchQuery);
                Clients.Clear();
                foreach (var c in clients)
                    Clients.Add(c);

                if (Clients.Count > 0)
                    SelectedClient = Clients[0];
                else
                    SelectedClient = null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddClientAsync()
        {
            var addWindow = App.Services.GetRequiredService<AddClient>();
            if (addWindow.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        [RelayCommand]
        private async Task EditClientAsync(Client? client)
        {
            if (client == null) return;
            // Открываем окно редактирования (можно использовать то же AddClient с параметрами)
            // Пока пропустим
            MessageBox.Show("Редактирование клиента (в разработке)");
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ToggleBlacklistAsync(Client? client)
        {
            if (client == null) return;
            await _clientService.ToggleBlacklistAsync(client.Id);
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task ShowOrderHistoryAsync(Client? client)
        {
            if (client == null) return;
            var orders = await _clientService.GetClientOrdersAsync(client.Id);
            var historyWindow = new Views.ClientOrdersWindow(client, orders);
            historyWindow.Owner = Application.Current.MainWindow;
            historyWindow.ShowDialog();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            _ = LoadDataAsync();
        }
    }
}