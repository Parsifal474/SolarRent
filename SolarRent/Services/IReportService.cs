using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.ViewModels;

namespace SolarRent.Services
{
    public interface IReportService
    {
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewClientsCountAsync(DateTime startDate, DateTime endDate);
        Task<List<DebtorInfo>> GetDebtorsAsync();
        Task<List<ManagerStats>> GetManagerStatsAsync(DateTime startDate, DateTime endDate);
        Task<List<EquipmentPopularity>> GetPopularEquipmentAsync(DateTime startDate, DateTime endDate, int topCount);
    }
}