// ============================================================
// Модель: Оборудование (солнечные панели, инверторы, аккумуляторы)
// Разработчик: Яковчук В.П. (Team Lead)
// Описание: Хранит информацию о товарах/оборудовании компании
// ============================================================

using System.Collections.Generic;

namespace SolarRent.Models
{
    public enum EquipmentType
    {
        Panel,      // Солнечная панель
        Inverter,   // Инвертор
        Battery,    // Аккумулятор
        Accessory   // Комплектующее
    }

    public class Equipment
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public string Name { get; set; } = string.Empty;     // Название модели
        public EquipmentType Type { get; set; }              // Тип оборудования
        public double Power { get; set; }                    // Мощность (кВт)
        public decimal Price { get; set; }                   // Цена
        public string Status { get; set; } = "InStock";      // Статус: InStock, Rented, Repair, Disposed
        public string? Description { get; set; }             // Описание
        public ICollection<RentalOrder> Rentals { get; set; } = new List<RentalOrder>(); // Аренды
        public ICollection<DefectRecord> Defects { get; set; } = new List<DefectRecord>(); // Дефекты
    }
}