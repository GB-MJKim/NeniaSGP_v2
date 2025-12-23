using Microsoft.Extensions.DependencyInjection;
using Nenia.SGP.Models;
using Nenia.SGP.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using System.IO;

using System.Windows.Media.Imaging;

namespace Nenia.SGP.Views
{
    public partial class ProductEditWindow : Window
    {
        private readonly ProductService _productService;
        private readonly Product _product; // 편집 대상(없으면 신규)
        private readonly ImageProcessingService _imageService;

        public bool Saved { get; private set; }


        public ProductEditWindow(Product? product = null)
        {
            InitializeComponent();

            _imageService = ((App)Application.Current).Services.GetRequiredService<ImageProcessingService>();

            _productService = ((App)Application.Current).Services.GetRequiredService<ProductService>();

            _product = product ?? new Product();
            Title = _product.ProductID == 0 ? "상품 추가" : $"상품 수정 (ID: {_product.ProductID})";

            LoadToUi();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            var (origRel, mainRel, thumbRel) = _imageService.ImportAndProcess(dlg.FileName);

            _product.OriginalImagePath = origRel;
            _product.MainImagePath = mainRel;
            _product.ThumbnailImagePath = thumbRel;

            ImagePathText.Text = Path.GetFileName(dlg.FileName);

            MainPreview.Source = LoadPreview(mainRel);
            ThumbPreview.Source = LoadPreview(thumbRel);
        }

        private static BitmapImage LoadPreview(string relOrAbs)
        {
            var abs = ImageProcessingService.ToAbsPath(relOrAbs);

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(abs, UriKind.Absolute);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        private void LoadToUi()
        {
            ProductNameBox.Text = _product.ProductName;
            CapacityBox.Text = _product.Capacity ?? "";
            StandardPriceBox.Text = _product.StandardPrice == 0 ? "" : _product.StandardPrice.ToString(CultureInfo.InvariantCulture);
            PricePerUnitBox.Text = _product.PricePerUnit?.ToString(CultureInfo.InvariantCulture) ?? "";
            TagsBox.Text = _product.Tags ?? "";
            AllergyBox.Text = _product.AllergyInfo ?? "";
            DescriptionBox.Text = _product.Description ?? "";

            if (!string.IsNullOrWhiteSpace(_product.StorageType))
            {
                foreach (var item in StorageTypeCombo.Items)
                {
                    if (item is ComboBoxItem cbi && string.Equals(cbi.Content?.ToString(), _product.StorageType))
                    {
                        StorageTypeCombo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = ProductNameBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("상품명은 필수입니다.");
                    return;
                }

                if (!double.TryParse(StandardPriceBox.Text.Trim(), out var standardPrice))
                {
                    MessageBox.Show("표준가를 숫자로 입력하세요.");
                    return;
                }

                double? ppu = null;
                var ppuText = PricePerUnitBox.Text.Trim();
                if (!string.IsNullOrEmpty(ppuText))
                {
                    if (!double.TryParse(ppuText, out var parsed))
                    {
                        MessageBox.Show("100g 단가를 숫자로 입력하세요.");
                        return;
                    }
                    ppu = parsed;
                }

                _product.ProductName = name;
                _product.Capacity = CapacityBox.Text.Trim();
                _product.StandardPrice = standardPrice;
                _product.PricePerUnit = ppu;
                _product.StorageType = (StorageTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
                _product.Tags = TagsBox.Text.Trim();
                _product.AllergyInfo = AllergyBox.Text.Trim();
                _product.Description = DescriptionBox.Text.Trim();

                if (_product.ProductID == 0)
                    await _productService.AddProductAsync(_product);
                else
                    await _productService.UpdateProductAsync(_product);

                Saved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
