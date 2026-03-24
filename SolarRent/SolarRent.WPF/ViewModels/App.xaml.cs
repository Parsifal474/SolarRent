using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Core.Services;
using SolarRent.Data;
using SolarRent.Data.Repositories;
using SolarRent.WPF.ViewModels;

namespace SolarRent.WPF
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            // База данных
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=SolarRent;Username=postgres;Password=password"));

            // Репозитории
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Сервисы
            services.AddScoped<IEquipmentService, EquipmentService>();

            // ViewModels и окна
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Применяем миграции (опционально)
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}