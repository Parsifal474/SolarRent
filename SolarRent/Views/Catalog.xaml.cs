using Microsoft.Extensions.DependencyInjection;
using SolarRent.ViewModels;
using SolarRent.Views; // Если AddEquipmentWindow лежит в папке Views
using System.Windows;

namespace SolarRent
{
    public partial class Catalog : Window
    {
        private readonly CatalogViewModel _viewModel;

        // 🔥 Конструктор для DI (основной)
        public Catalog(CatalogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;  // 🔥 Ключевая строка: привязка ViewModel
        }

        // 🔥 Конструктор для дизайнера (опционально, можно удалить)
        public Catalog() : this(App.Services.GetRequiredService<CatalogViewModel>())
        {
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 Открываем окно добавления через сервис (не через DbContext!)
            var addWindow = App.Services.GetRequiredService<AddEquipmentWindow>();

            // Если нужно обновить каталог после добавления:
            addWindow.Closed += (s, args) => _viewModel.LoadCommand?.Execute(null);

            addWindow.ShowDialog();
        }
    }
}