using System.Windows;
using SolarRent.ViewModels;

namespace SolarRent
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструктор без параметров (нужен для XAML-парсера)
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Конструктор для DI (получает ViewModel)
        /// </summary>
        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}