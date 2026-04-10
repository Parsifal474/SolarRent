using Microsoft.Extensions.DependencyInjection;
using SolarRent.Services.Navigation;
using SolarRent.ViewModels;
using SolarRent.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace SolarRent
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;
        private string? _currentPageKey;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;

            if (_navigationService is NavigationService ns)
                ns.Initialize(MainFrame);

            DataContext = new NavigationViewModel(_navigationService);

            MainFrame.Navigated += MainFrame_Navigated;

            // Стартовая страница
            _navigationService.NavigateTo("Catalog");
            _currentPageKey = "Catalog";
            UpdateHeaderForPage("Catalog");
            NavigationTabControl.SelectedIndex = 0;
        }

        private void NavigationTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationTabControl.SelectedItem is TabItem selectedTab && selectedTab.Tag != null)
            {
                string pageKey = selectedTab.Tag.ToString()!;
                if (pageKey != _currentPageKey)
                {
                    _navigationService.NavigateTo(pageKey);
                    _currentPageKey = pageKey;
                    UpdateHeaderForPage(pageKey);
                }
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is Page page)
            {
                string pageKey = page.GetType().Name.Replace("Page", "");
                _currentPageKey = pageKey;
                UpdateHeaderForPage(pageKey);
            }
        }

        private void UpdateHeaderForPage(string pageKey)
        {
            ActionButtonsPanel.Children.Clear();

            switch (pageKey)
            {
                case "Catalog":
                    PageTitleText.Text = "Каталог оборудования";
                    AddHeaderButton("+ Добавить оборудование", () =>
                    {
                        var addWindow = App.Services.GetRequiredService<AddEquipmentWindow>();
                        addWindow.ShowDialog();
                        // TODO: обновить каталог после закрытия
                    });
                    AddHeaderButton("Фильтры", () => { /* TODO */ });
                    AddHeaderButton("Экспорт", () => { /* TODO */ });
                    break;

                case "Calendar":
                    PageTitleText.Text = "Календарь аренды";
                    AddHeaderButton("Новое оборудование", () =>
                    {
                        var addWindow = App.Services.GetRequiredService<AddEquipmentWindow>();
                        addWindow.ShowDialog();
                    });
                    break;

                case "Reports":
                    PageTitleText.Text = "Отчеты и аналитика";
                    AddHeaderButton("📊 Экспорт", () =>
                    {
                        if (MainFrame.Content is Reports reportsPage &&
                            reportsPage.DataContext is ReportsViewModel vm)
                        {
                            vm.ExportToCsvCommand?.Execute(null);  // Вызываем CSV-экспорт
                        }
                    });
                    break;

                case "Clients":
                    PageTitleText.Text = "База клиентов";
                    AddHeaderButton("+ Добавить клиента", () =>
                    {
                        var addWindow = App.Services.GetRequiredService<AddClient>();
                        addWindow.EditingClient = null;
                        addWindow.Closed += (s, e) =>
                        {
                            if (addWindow.DialogResult == true && MainFrame.Content is Clients clientsPage)
                            {
                                if (clientsPage.DataContext is ClientsViewModel vm)
                                {
                                    vm.LoadDataCommand?.Execute(null);
                                }
                            }
                        };
                        addWindow.ShowDialog();
                    });
                    AddHeaderButton("Экспорт", () =>
                    {
                        if (MainFrame.Content is Clients clientsPage && clientsPage.DataContext is ClientsViewModel vm)
                        {
                            vm.ExportToCsvCommand?.Execute(null);
                        }
                    });
                    break;

                case "Settings":
                    PageTitleText.Text = "Настройки";
                    // Панель быстрых действий оставляем пустой (или добавьте кнопки по желанию)
                    break;
            }
        }

        private void AddHeaderButton(string text, System.Action onClick)
        {
            var button = new Button
            {
                Content = text,
                Style = (Style)FindResource("HeaderButtonStyle")
            };
            button.Click += (s, e) => onClick();
            ActionButtonsPanel.Children.Add(button);
        }
    }
}