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
using SolarRent.Services.Navigation;
using SolarRent.ViewModels;
using SolarRent.Views;
using SolarRent.Views.Pages;

namespace SolarRent
{
    public partial class App : Application
    {
        private static IHost? _host;

        public static IServiceProvider Services => _host?.Services
            ?? throw new InvalidOperationException("Хост не инициализирован");

        public App()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // === 🔹 База данных ===
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? "Host=localhost;Port=5432;Database=solarrent;Username=postgres;Password=POPO";

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // === 🔹 Репозитории ===
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    // === 🔹 Сервисы ===
                    services.AddScoped<IEquipmentService, EquipmentService>();
                    services.AddScoped<IClientService, ClientService>();
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<ICalendarService, CalendarService>();
                    services.AddScoped<IReportService, ReportService>();

                    // === 🔹 Навигация ===
                    services.AddSingleton<INavigationService, NavigationService>();

                    // === 🔹 ViewModel ===
                    services.AddTransient<CatalogViewModel>();
                    services.AddTransient<RentalCalendarViewModel>();
                    services.AddTransient<ClientsViewModel>();
                    services.AddTransient<ReportsViewModel>();

                    // === 🔹 Страницы (Page) — для навигации в Frame ===
                    services.AddTransient<Views.Pages.Catalog>();
                    services.AddTransient<Views.Pages.RentalCalendar>();
                    services.AddTransient<Views.Pages.Reports>();
                    services.AddTransient<Views.Pages.Clients>();
                    services.AddTransient<Views.Pages.SettingsPage>();
                    services.AddTransient<Views.Pages.Sale>();
                    services.AddTransient<Views.Pages.Lease_issue>();      // 🔥 Выдача аренды
                    services.AddTransient<Views.Pages.Lease_acceptance>(); // 🔥 Приемка аренды

                    // === 🔹 Окна (Window) — модальные диалоги ===
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<AddEquipmentWindow>();
                    services.AddTransient<EditEquipmentWindow>();         // 🔥 Новое окно
                    services.AddTransient<AddClient>();
                    services.AddTransient<NewRental>();
                    services.AddTransient<RegisterWindow>();
                    services.AddTransient<DayEventsWindow>();
                    services.AddTransient<ClientOrdersWindow>();
                    services.AddTransient<Sale>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Инициализация БД
            using (var scope = _host.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await dbContext.Database.EnsureCreatedAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка подключения к БД:\n{ex.Message}",
                        "Ошибка базы данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            // Запуск окна входа
            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}