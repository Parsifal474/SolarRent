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

    public partial class Equipment  // ← Добавили partial
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public string Name { get; set; } = string.Empty;     // Название модели
        public EquipmentType Type { get; set; }              // Тип оборудования
        public double Power { get; set; }                    // Мощность (кВт)
        public decimal Price { get; set; }                   // Цена
        public string Status { get; set; } = "InStock";      // Статус: InStock, Rented, Repair, Disposed
        public string? Description { get; set; }             // Описание
        public ICollection<RentalOrder> Rentals { get; set; } = new List<RentalOrder>();
        public ICollection<DefectRecord> Defects { get; set; } = new List<DefectRecord>();

        // ============================================================
        // Вычисляемые свойства для отображения в UI
        // ============================================================

        public string TypeDisplay => Type switch
        {
            EquipmentType.Panel => "🔆 Панель",
            EquipmentType.Inverter => "⚡ Инвертор",
            EquipmentType.Battery => "🔋 Аккумулятор",
            EquipmentType.Accessory => "🔧 Комплектующее",
            _ => Type.ToString()
        };

        public string PowerDisplay => Type switch
        {
            EquipmentType.Panel or EquipmentType.Inverter => $"{Power} кВт",
            EquipmentType.Battery => $"{Power} кВт·ч",
            _ => $"{Power} кВт"
        };

        public decimal RentalPricePerDay => Price * 0.01m;
        public decimal DepositAmount => Price * 0.5m;
        public string DisplayName => $"{Name} ({PowerDisplay})";
    }
}