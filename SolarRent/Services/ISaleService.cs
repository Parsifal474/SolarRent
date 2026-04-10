// Services/ISaleService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleRecord>> GetAllSalesAsync();
        Task<IEnumerable<SaleRecord>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<SaleRecord?> GetSaleByIdAsync(int id);
        Task<SaleRecord> CreateSaleAsync(SaleRecord sale, List<SaleItemRecord> items);
        Task DeleteSaleAsync(int id);
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}