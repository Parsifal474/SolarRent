using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarRent.Data;
using SolarRent.Models;

namespace SolarRent.Services
{
    public interface IAuthService
    {
        Task<bool> AuthenticateAsync(string login, string password);
        User? CurrentUser { get; }
        Task<bool> CreateUserAsync(User newUser, string directorPassword);
        Task<bool> IsDirectorAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private User? _currentUser;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public User? CurrentUser => _currentUser;

        // 🔥 Простая проверка: пароль в базе == введённый пароль
        public async Task<bool> AuthenticateAsync(string login, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == login && u.PasswordHash == password);

            if (user == null)
                return false;

            _currentUser = user;
            return true;
        }

        public async Task<bool> IsDirectorAsync()
        {
            return _currentUser?.Role == Role.Director;
        }

        // 🔥 Создание пользователя (пароли сохраняются как есть)
        public async Task<bool> CreateUserAsync(User newUser, string directorPassword)
        {
            // 1. Проверяем, что текущий пользователь — ДИРЕКТОР
            if (_currentUser == null || _currentUser.Role != Role.Director)
                return false;

            // 2. Проверяем пароль директора (простое сравнение)
            if (_currentUser.PasswordHash != directorPassword)
                return false;

            // 3. Проверяем уникальность логина
            if (await _context.Users.AnyAsync(u => u.Login == newUser.Login))
                return false;

            // 4. Сохраняем пароль как есть (БЕЗ хеширования!)
            newUser.CreatedAt = DateTime.UtcNow;

            // 5. Сохраняем в БД
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}