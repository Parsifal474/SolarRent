using System.Collections.Generic;
using System.Windows;
using SolarRent.ViewModels;

namespace SolarRent.Views
{
    public partial class AllDebtorsWindow : Window
    {
        public AllDebtorsWindow(List<DebtorInfo> debtors)
        {
            InitializeComponent();
            DataContext = debtors;
        }

        public AllDebtorsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}