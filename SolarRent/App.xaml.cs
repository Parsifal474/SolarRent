using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarRent.Core.Services;
using SolarRent.Data;
using SolarRent.Data.Repositories;
using SolarRent.SolarRent.Data;
using SolarRent.WPF.ViewModels;
using System.Windows;

namespace SolarRent.WPF
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // База данных
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql("Host=localhost;Database=SolarRent;Username=postgres;Password=password"));

                    // Репозитории
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    // Сервисы
                    services.AddScoped<IEquipmentService, EquipmentService>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();

                    // Окна
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Применяем миграции при старте (опционально)
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

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