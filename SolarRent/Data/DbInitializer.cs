using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarRent.Models;

namespace SolarRent.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ILogger logger)
        {
            if (context.RentalOrders.Any()) return; // Уже инициализировано

            logger.LogInformation("Инициализация тестовых данных...");

            // Создайте тестового клиента
            var client = new Client
            {
                FullName = "ООО \"Энергия\"",
                Type = "Company",
                CompanyName = "ООО \"Энергия\"",
                TaxId = "7701234567",
                Phone = "+7 (495) 123-45-67",
                Email = "info@energia.ru",
                Address = "г. Москва, ул. Примерная, д. 1"
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            // Создайте тестовое оборудование
            var equipment = new Equipment
            {
                Name = "Солнечная панель 300W",
                Type = EquipmentType.Panel,
                Power = 300,
                Price = 15000,
                Status = "InStock",
                Description = "Монокристаллическая панель"
            };
            context.Equipments.Add(equipment);
            await context.SaveChangesAsync();

            // Создайте тестовый заказ #1024
            var order = new RentalOrder
            {
                ClientId = client.Id,
                EquipmentId = equipment.Id,
                StartDate = new DateTime(2025, 3, 15),
                EndDate = new DateTime(2025, 3, 20),
                RentalPrice = 15000,
                Deposit = 7500,
                Penalty = 0,
                Status = "Pending"
            };
            context.RentalOrders.Add(order);
            await context.SaveChangesAsync();

            logger.LogInformation("Тестовые данные созданы. Заказ #1024 доступен.");
        }
    }
}