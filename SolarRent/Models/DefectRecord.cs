// ============================================================
// Модель: Запись о дефектовке оборудования
// Разработчик: Ковалевский И.М. (Backend разработчик)
// Описание: Фиксирует результаты осмотра оборудования после возврата из аренды
// ============================================================

using System;

namespace SolarRent.Models
{
    public class DefectRecord
    {
        public int Id { get; set; }                          // Уникальный идентификатор записи
        public int EquipmentId { get; set; }                 // Внешний ключ: ID оборудования
        public Equipment Equipment { get; set; } = null!;    // Навигационное свойство: оборудование
        public DateTime CheckDate { get; set; }              // Дата проведения осмотра
        public string Description { get; set; } = string.Empty; // Описание дефектов
        public string Resolution { get; set; } = string.Empty;   // Решение: Repaired, Replaced, WriteOff
    }
}