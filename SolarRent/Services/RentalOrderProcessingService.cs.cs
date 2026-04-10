using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent.Services
{
    public class RentalOrderProcessingService : IRentalOrderProcessingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RentalOrderProcessingService> _logger;

        public RentalOrderProcessingService(AppDbContext context, ILogger<RentalOrderProcessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RentalOrderProcessing?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _context.RentalOrders
                    .Include(r => r.Client)
                    .Include(r => r.Equipment)
                    .FirstOrDefaultAsync(r => r.Id == orderId);

                if (order == null) return null;

                var processing = new RentalOrderProcessing
                {
                    Id = order.Id,
                    OrderId = order.Id,
                    OrderNumber = $"#{order.Id}",
                    CreatedAt = order.StartDate,
                    ClientName = order.Client.FullName,
                    ClientType = order.Client.Type == "Individual" ? "Физ. лицо" : "Юр. лицо",
                    StartDate = order.StartDate,
                    EndDate = order.EndDate,
                    TotalAmount = order.RentalPrice + order.Deposit,
                    Status = order.Status,
                    Notes = "",
                    EquipmentItems = new List<OrderEquipmentItem>
                    {
                        new OrderEquipmentItem
                        {
                            EquipmentId = order.EquipmentId,
                            Name = order.Equipment.Name,
                            SerialNumber = $"SN: {order.EquipmentId}-{DateTime.Now.Year}",
                            Type = order.Equipment.Type.ToString(),
                            Condition = "Отличное"
                        }
                    }
                };

                return processing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказа {OrderId}", orderId);
                return null;
            }
        }

        public async Task<bool> IssueEquipmentAsync(int orderId, string notes)
        {
            try
            {
                var order = await _context.RentalOrders.FindAsync(orderId);
                if (order == null) return false;

                order.Status = "Active";
                order.ManagedByUserId = 1; // TODO: получить текущего пользователя

                var equipment = await _context.Equipments.FindAsync(order.EquipmentId);
                if (equipment != null)
                {
                    equipment.Status = "Rented";
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Оборудование выдано по заказу {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выдаче оборудования заказа {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> ReturnEquipmentAsync(int orderId, string notes, List<string> photoPaths)
        {
            try
            {
                var order = await _context.RentalOrders.FindAsync(orderId);
                if (order == null) return false;

                order.Status = "Returned";
                order.ActualReturnDate = DateTime.Now;

                var equipment = await _context.Equipments.FindAsync(order.EquipmentId);
                if (equipment != null)
                {
                    equipment.Status = "InStock";
                }

                // Сохраняем фото в базу или файловую систему
                foreach (var photoPath in photoPaths)
                {
                    // TODO: сохранить путь к фото
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Оборудование возвращено по заказу {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при возврате оборудования заказа {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> GenerateRentalAgreementAsync(int orderId, string outputPath)
        {
            try
            {
                var order = await _context.RentalOrders
                    .Include(r => r.Client)
                    .Include(r => r.Equipment)
                    .FirstOrDefaultAsync(r => r.Id == orderId);

                if (order == null) return false;

                // Генерация простого текстового договора
                var agreementText = $@"
ДОГОВОР АРЕНДЫ № {orderId}
г. Москва                                                                                                    {order.StartDate:dd.MM.yyyy}

АРЕНДОДАТЕЛЬ: SolarRent
АРЕНДАТОР: {order.Client.FullName}

1. ПРЕДМЕТ ДОГОВОРА
1.1. Арендодатель передает Арендатору оборудование: {order.Equipment.Name}
1.2. Срок аренды: с {order.StartDate:dd.MM.yyyy} по {order.EndDate:dd.MM.yyyy}
1.3. Стоимость аренды: {order.RentalPrice:N0} ₽
1.4. Залог: {order.Deposit:N0} ₽

2. ПРАВА И ОБЯЗАННОСТИ СТОРОН
...

ПОДПИСИ СТОРОН:

Арендодатель: _________________          Арендатор: _________________
";

                await File.WriteAllTextAsync(outputPath, agreementText);
                _logger.LogInformation("Договор аренды сгенерирован: {Path}", outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации договора аренды заказа {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> GenerateAcceptanceCertificateAsync(int orderId, string outputPath)
        {
            try
            {
                var order = await _context.RentalOrders
                    .Include(r => r.Client)
                    .Include(r => r.Equipment)
                    .FirstOrDefaultAsync(r => r.Id == orderId);

                if (order == null) return false;

                var certificateText = $@"
АКТ ПРИЕМА-ПЕРЕДАЧИ № {orderId}
от {DateTime.Now:dd.MM.yyyy}

Мы, нижеподписавшиеся, составили настоящий акт о том, что:

АРЕНДОДАТЕЛЬ передал, а АРЕНДАТОР {order.Client.FullName} принял следующее оборудование:

1. {order.Equipment.Name}
   Серийный номер: {order.EquipmentId}
   Состояние: Отличное

Оборудование проверено, претензий нет.

АРЕНДОДАТЕЛЬ: _________________          АРЕНДАТОР: _________________
";

                await File.WriteAllTextAsync(outputPath, certificateText);
                _logger.LogInformation("Акт приема-передачи сгенерирован: {Path}", outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации акта заказа {OrderId}", orderId);
                return false;
            }
        }
    }
}