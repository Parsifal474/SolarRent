using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SolarRent.Views.Pages;

namespace SolarRent.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private Frame? _mainFrame;
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<string> _history = new();

        public bool CanGoBack => _history.Count > 0;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public void NavigateTo(string pageKey)
        {
            if (_mainFrame == null)
                throw new InvalidOperationException("Navigation frame not initialized.");

            Page page = pageKey switch
            {
                "Catalog" => _serviceProvider.GetRequiredService<Catalog>(),
                "Calendar" => _serviceProvider.GetRequiredService<RentalCalendar>(),
                "Reports" => _serviceProvider.GetRequiredService<Reports>(),
                "Clients" => _serviceProvider.GetRequiredService<Clients>(),
                _ => throw new ArgumentException($"Unknown page: {pageKey}")
            };

            if (_mainFrame.Content != null)
            {
                string currentKey = _mainFrame.Content.GetType().Name;
                _history.Push(currentKey);
            }

            _mainFrame.Navigate(page);
        }

        public void GoBack()
        {
            if (!CanGoBack) return;
            string previousKey = _history.Pop();
            NavigateTo(previousKey);
        }
    }
}