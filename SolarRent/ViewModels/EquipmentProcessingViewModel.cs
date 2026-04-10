using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SolarRent.Models;
using SolarRent.Services;

namespace SolarRent.ViewModels
{
    public partial class EquipmentProcessingViewModel : ObservableObject
    {
        private readonly IRentalOrderProcessingService _processingService;

        [ObservableProperty]
        private RentalOrderProcessing? _currentOrder;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _photoPaths = new();

        [ObservableProperty]
        private bool _isIssueMode = true;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public EquipmentProcessingViewModel(IRentalOrderProcessingService processingService)
        {
            _processingService = processingService;
        }

        public async Task LoadOrderAsync(int orderId)
        {
            IsLoading = true;
            try
            {
                CurrentOrder = await _processingService.GetOrderByIdAsync(orderId);
                if (CurrentOrder == null)
                {
                    StatusMessage = "Заказ не найден";
                }
                else
                {
                    StatusMessage = $"Заказ #{CurrentOrder.OrderNumber} загружен";
                    IsIssueMode = CurrentOrder.Status != "Returned";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ConfirmIssueAsync()
        {
            if (CurrentOrder == null)
            {
                MessageBox.Show("Нет загруженного заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _processingService.IssueEquipmentAsync(CurrentOrder.OrderId, Notes);
            if (result)
            {
                MessageBox.Show("Оборудование успешно выдано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Выдача подтверждена";
            }
            else
            {
                MessageBox.Show("Ошибка при выдаче оборудования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ConfirmReturnAsync()
        {
            if (CurrentOrder == null)
            {
                MessageBox.Show("Нет загруженного заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _processingService.ReturnEquipmentAsync(CurrentOrder.OrderId, Notes, new List<string>(PhotoPaths));
            if (result)
            {
                MessageBox.Show("Оборудование успешно возвращено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Возврат подтвержден";
            }
            else
            {
                MessageBox.Show("Ошибка при возврате оборудования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateAgreementAsync()
        {
            if (CurrentOrder == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"Договор_аренды_{CurrentOrder.OrderId}.txt",
                Title = "Сохранить договор аренды"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _processingService.GenerateRentalAgreementAsync(CurrentOrder.OrderId, dialog.FileName);
                if (result)
                {
                    MessageBox.Show("Договор успешно сгенерирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при генерации договора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task GenerateCertificateAsync()
        {
            if (CurrentOrder == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"Акт_приема_{CurrentOrder.OrderId}.txt",
                Title = "Сохранить акт приема-передачи"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _processingService.GenerateAcceptanceCertificateAsync(CurrentOrder.OrderId, dialog.FileName);
                if (result)
                {
                    MessageBox.Show("Акт успешно сгенерирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при генерации акта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddPhoto()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
                Multiselect = true,
                Title = "Выберите фотографии оборудования"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    PhotoPaths.Add(file);
                }
            }
        }

        [RelayCommand]
        private void RemovePhoto(string path)
        {
            PhotoPaths.Remove(path);
        }

        [RelayCommand]
        private void Cancel()
        {
            // Закрытие окна
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}