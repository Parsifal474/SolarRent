using SolarRent.WPF.ViewModels;
using System.Windows;

namespace SolarRent.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}