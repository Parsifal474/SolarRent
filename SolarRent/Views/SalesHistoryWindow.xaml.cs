// Views/SalesHistoryWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.ViewModels;

namespace SolarRent.Views
{
    public partial class SalesHistoryWindow : Window
    {
        public SalesHistoryWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<SalesHistoryViewModel>();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SalesHistoryViewModel vm && vm.SelectedSale != null)
            {
                vm.ViewSaleDetailsCommand.Execute(vm.SelectedSale);
            }
        }
    }

    // Конвертер для безопасного деления
    public class SafeDivisionConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is object[] values && values.Length == 2)
            {
                if (decimal.TryParse(values[0]?.ToString(), out decimal total) &&
                    int.TryParse(values[1]?.ToString(), out int count) &&
                    count > 0)
                {
                    return total / count;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}