// ============================================================
// Модель: Заказ на аренду оборудования
// Разработчик: Ковалевский И.М. (Backend разработчик)
// Описание: Хранит информацию о каждой аренде оборудования
// ============================================================

using System;

namespace SolarRent.Models
{
    public class RentalOrder
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public int ClientId { get; set; }                    // Внешний ключ: ID клиента
        public Client Client { get; set; } = null!;          // Навигация: клиент
        public int EquipmentId { get; set; }                 // Внешний ключ: ID оборудования
        public Equipment Equipment { get; set; } = null!;    // Навигация: оборудование
        public DateTime StartDate { get; set; }              // Дата начала аренды
        public DateTime EndDate { get; set; }                // Плановая дата окончания
        public DateTime? ActualReturnDate { get; set; }      // Фактическая дата возврата
        public decimal RentalPrice { get; set; }             // Стоимость аренды
        public decimal Deposit { get; set; }                 // Залог
        public decimal Penalty { get; set; }                 // Пеня
        public string Status { get; set; } = "Active";       // Active, Returned, Overdue
        public int? ManagedByUserId { get; set; }            // Внешний ключ: ID менеджера
        public User? ManagedBy { get; set; }                 // Навигация: менеджер
    }
}