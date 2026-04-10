using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllClientsAsync();

            return await _context.Clients
                .Where(c => c.FullName.Contains(query) || c.Phone.Contains(query))
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddClientAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClientAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleBlacklistAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsBlacklisted = !client.IsBlacklisted;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.RentalOrders.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.RentalOrders.SumAsync(o => o.RentalPrice);
        }

        public async Task<int> GetOverdueCountAsync()
        {
            return await _context.RentalOrders
                .CountAsync(o => o.Status == "Overdue");
        }

        public async Task<IEnumerable<RentalOrder>> GetClientOrdersAsync(int clientId)
        {
            return await _context.RentalOrders
                .Include(o => o.Equipment)
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.StartDate)
                .ToListAsync();
        }
    }
}