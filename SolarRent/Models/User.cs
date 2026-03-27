// ============================================================
// Модель: Пользователь системы (сотрудник)
// Разработчик: Дюмин С.А. (DevOps, тестировщик)
// Описание: Хранит информацию о сотрудниках с доступом к системе
// ============================================================

using System;

namespace SolarRent.Models
{
    public enum Role
    {
        Manager,    // Менеджер по продажам и аренде
        Warehouse,  // Складской работник / Техник
        Director,   // Директор / Управляющий
        Accountant  // Бухгалтер
    }

    public class User
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public string FullName { get; set; } = string.Empty; // Полное имя сотрудника
        public string Login { get; set; } = string.Empty;    // Логин
        public string PasswordHash { get; set; } = string.Empty; // Хеш пароля
        public Role Role { get; set; }                       // Роль
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Дата создания
    }
}