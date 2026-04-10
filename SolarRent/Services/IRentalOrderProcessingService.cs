using System.Collections.Generic;
using System.Threading.Tasks;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface IRentalOrderProcessingService
    {
        Task<RentalOrderProcessing?> GetOrderByIdAsync(int orderId);
        Task<bool> IssueEquipmentAsync(int orderId, string notes);
        Task<bool> ReturnEquipmentAsync(int orderId, string notes, List<string> photoPaths);
        Task<bool> GenerateRentalAgreementAsync(int orderId, string outputPath);
        Task<bool> GenerateAcceptanceCertificateAsync(int orderId, string outputPath);
    }
}