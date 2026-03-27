using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarRent.Data;
using SolarRent.Views;
using System;
using System.IO;

namespace SolarRent
{
    public partial class App : Application
    {
        private static IHost _host;  // ← static!

        // 🔹 Статическое свойство
        public static IServiceProvider Services => _host.Services;

        public App()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? "Host=localhost;Port=5432;Database=solarrent;Username=postgres;Password=POPO";

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    services.AddSingleton<LoginWindow>();
                    services.AddSingleton<MainDashboardWindow>();
                    services.AddSingleton<Catalog>();
                    services.AddTransient<AddClient>();
                    services.AddTransient<NewRental>();
                    services.AddTransient<RentalCalendar>();
                    services.AddTransient<Reports>();
                    services.AddTransient<AddEquipmentWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }
}