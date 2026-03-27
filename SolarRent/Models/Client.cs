// ============================================================
// Модель: Клиент (физическое или юридическое лицо)
// Разработчик: Яковчук В.П. (Team Lead)
// Описание: Хранит информацию о клиентах компании
// ============================================================

using System.Collections.Generic;

namespace SolarRent.Models
{
    public class Client
    {
        public int Id { get; set; }                          // Уникальный идентификатор клиента
        public string Type { get; set; } = "Individual";     // Тип: Individual (физ. лицо) или Company (юр. лицо)
        public string FullName { get; set; } = string.Empty; // Полное имя (для физ. лиц) или контактное лицо (для юр. лиц)
        public string? CompanyName { get; set; }             // Название компании (только для юр. лиц)
        public string? TaxId { get; set; }                   // ИНН (только для юр. лиц)
        public string Phone { get; set; } = string.Empty;    // Телефон для связи
        public string Email { get; set; } = string.Empty;    // Email для связи
        public string? Address { get; set; }                 // Адрес
        public bool IsBlacklisted { get; set; }              // Флаг: клиент в чёрном списке
        public ICollection<RentalOrder> Orders { get; set; } = new List<RentalOrder>(); // Заказы клиента
    }
}