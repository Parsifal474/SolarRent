using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarRent.Data;
using SolarRent.Data.Repositories;
using SolarRent.Services;
using SolarRent.ViewModels;
using SolarRent.Views;
using SolarRent.Services.Navigation;
using SolarRent.Views.Pages;

namespace SolarRent
{
    public partial class App : Application
    {
        private static IHost? _host; //изменено ?

        // 🔹 Статический доступ к сервисам
        public static IServiceProvider Services => _host?.Services
            ?? throw new InvalidOperationException("Хост не инициализирован"); // изменено, проверка by рыжик

        public App()
        {
            // 🔹 Настройка конфигурации
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)  // ✅ Правильный путь для WPF
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // 🔹 Настройка хоста и DI
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // === База данных ===
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? "Host=localhost;Port=5432;Database=solarrent;Username=postgres;Password=POPO";

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // === Репозитории ===
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    // === Сервисы ===
                    services.AddScoped<IEquipmentService, EquipmentService>();
                    // services.AddScoped<IClientService, ClientService>();
                    // services.AddScoped<IRentalOrderService, RentalOrderService>();

                    // === ViewModel (Transient — новый экземпляр для каждого окна) ===
                    services.AddTransient<CatalogViewModel>();
                    // services.AddTransient<MainViewModel>();
                    // services.AddTransient<LoginViewModel>();

                    // === Окна (Transient — можно открывать/закрывать сколько угодно) ===
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<AddClient>();
                    services.AddTransient<AddEquipmentWindow>();
                    services.AddTransient<NewRental>();

                    // === Страницы ===
                    services.AddTransient<Views.Pages.Catalog>();
                    services.AddTransient<Views.Pages.RentalCalendar>();
                    services.AddTransient<Views.Pages.Reports>();

                    services.AddSingleton<MainWindow>();

                    services.AddSingleton<Services.Navigation.INavigationService, Services.Navigation.NavigationService>();

                    services.AddScoped<ICalendarService, CalendarService>();
                    services.AddTransient<RentalCalendarViewModel>();
                    services.AddTransient<DayEventsWindow>();

                    services.AddTransient<Views.DayEventsWindow>();

                    services.AddScoped<IClientService, ClientService>();
                    services.AddTransient<ClientsViewModel>();
                    services.AddTransient<Views.Pages.Clients>();
                    services.AddTransient<Views.ClientOrdersWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // 🔹 Инициализация БД (только для разработки!)
            using (var scope = _host.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // ✅ Для продакшена используй миграции:
                    // await dbContext.Database.MigrateAsync();

                    // 🔹 Для разработки (создаёт БД если нет):
                    await dbContext.Database.EnsureCreatedAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка подключения к БД:\n{ex.Message}\n\nПроверьте appsettings.json",
                        "Ошибка базы данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            // 🔹 Запуск окна входа
            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // 🔹 Корректное завершение хоста
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}