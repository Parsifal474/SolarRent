using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SolarRent.Views
{
    public partial class EquipmentProcessingWindow : Window
    {
        public EquipmentProcessingWindow()
        {
            InitializeComponent();
            LoadOrderData();
        }

        private void LoadOrderData()
        {
            // Загружаем тестовые данные для заказа #1024
            LblOrderNumber.Text = "#1024";
            LblClient.Text = "Клиент: ООО \"Энергия\"";
            LblPeriod.Text = "Период: 15.03.2025 - 20.03.2025";
            LblAmount.Text = "Сумма: 22 500 ₽";
        }

        private void BtnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
                Multiselect = true,
                Title = "Выберите фотографии"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    AddPhotoThumbnail(file);
                }
            }
        }

        private void AddPhotoThumbnail(string filePath)
        {
            var border = new Border
            {
                Margin = new Thickness(5),
                Width = 150,
                Height = 150,
                CornerRadius = new CornerRadius(4),
                // ✅ ИСПРАВЛЕНО: используем Brushes.LightBlue напрямую
                Background = System.Windows.Media.Brushes.LightBlue
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Безопасная загрузка изображения
            try
            {
                var image = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(filePath)),
                    Stretch = System.Windows.Media.Stretch.UniformToFill,
                    Margin = new Thickness(2)
                };
                grid.Children.Add(image);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = "Ошибка загрузки",
                    Foreground = System.Windows.Media.Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                grid.Children.Add(errorText);
            }

            var fileName = new TextBlock
            {
                Text = System.IO.Path.GetFileName(filePath),
                FontSize = 10,
                TextTrimming = System.Windows.TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(2, 4, 2, 2)
            };
            grid.Children.Add(fileName);

            var removeBtn = new Button
            {
                Content = "✕",
                Width = 20,
                Height = 20,
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2),
                FontSize = 10,
                Padding = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            removeBtn.Click += (s, e) => ListPhotos.Items.Remove(border);
            grid.Children.Add(removeBtn);

            border.Child = grid;
            ListPhotos.Items.Add(border);
              }

        private void BtnCertificate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = $"Акт_приема_1024_{DateTime.Now:yyyyMMdd}.txt",
                Title = "Сохранить акт приема-передачи"
            };

            if (dialog.ShowDialog() == true)
            {
                var content = $@"АКТ ПРИЕМА-ПЕРЕДАЧИ № 1024
от {DateTime.Now:dd.MM.yyyy}

АРЕНДОДАТЕЛЬ: SolarRent
АРЕНДАТОР: ООО ""Энергия""

1. Передано оборудование:
   - Солнечная панель 300W (S/N: 300-2025-001)
   - Состояние: Отличное

2. Претензий к внешнему виду и комплектности нет.

ПОДПИСИ:
Арендодатель: _______________      Арендатор: _______________
";
                File.WriteAllText(dialog.FileName, content, System.Text.Encoding.UTF8);
                LblStatus.Text = "✓ Акт сохранён: " + Path.GetFileName(dialog.FileName);
            }
        }

        private void BtnAgreement_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = $"Договор_аренды_1024_{DateTime.Now:yyyyMMdd}.txt",
                Title = "Сохранить договор аренды"
            };

            if (dialog.ShowDialog() == true)
            {
                var content = $@"ДОГОВОР АРЕНДЫ № 1024
г. Москва                                                      {DateTime.Now:dd.MM.yyyy}

АРЕНДОДАТЕЛЬ: ООО ""SolarRent""
АРЕНДАТОР: ООО ""Энергия""

1. ПРЕДМЕТ ДОГОВОРА
1.1. Арендодатель передает Арендатору оборудование:
     - Солнечная панель 300W, S/N: 300-2025-001

1.2. Срок аренды: с 15.03.2025 по 20.03.2025
1.3. Стоимость аренды: 15 000 ₽
1.4. Залог: 7 500 ₽

2. ПРАВА И ОБЯЗАННОСТИ
2.1. Арендатор обязуется использовать оборудование по назначению.
2.2. Арендодатель обязуется передать оборудование в исправном состоянии.

3. ОТВЕТСТВЕННОСТЬ СТОРОН
...

ПОДПИСИ СТОРОН:

Арендодатель: _______________      Арендатор: _______________
";
                File.WriteAllText(dialog.FileName, content, System.Text.Encoding.UTF8);
                LblStatus.Text = "✓ Договор сохранён: " + Path.GetFileName(dialog.FileName);
            }
        }

        private void BtnIssue_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет логика выдачи через сервис
            MessageBox.Show("✓ Оборудование выдано!\nЗаказ #1024 переведён в статус 'Active'",
                          "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LblStatus.Text = "Выдача подтверждена";
            BtnIssue.IsEnabled = false;
            BtnReturn.Visibility = Visibility.Visible;
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет логика возврата через сервис
            MessageBox.Show("✓ Оборудование возвращено!\nЗаказ #1024 переведён в статус 'Returned'",
                          "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LblStatus.Text = "Возврат подтверждён";
            BtnReturn.IsEnabled = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}