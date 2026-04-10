using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<IEnumerable<Client>> SearchClientsAsync(string query);
        Task<Client?> GetClientByIdAsync(int id);
        Task AddClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(int id);
        Task ToggleBlacklistAsync(int id);
        Task<int> GetTotalOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetOverdueCountAsync();
        Task<IEnumerable<RentalOrder>> GetClientOrdersAsync(int clientId);
    }
}