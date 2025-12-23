using Microsoft.Extensions.DependencyInjection;
using Nenia.SGP.Models;
using Nenia.SGP.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nenia.SGP.Views
{
	public partial class ProductListPage : Page
	{
		private readonly ProductService _productService;
		private readonly ObservableCollection<Product> _items = new();

		public ProductListPage()
		{
			InitializeComponent();

			_productService = ((App)Application.Current)
				.Services
				.GetRequiredService<ProductService>();

			ProductGrid.ItemsSource = _items;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await LoadAllAsync();
		}

		private async void Search_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var keyword = SearchBox.Text?.Trim() ?? "";
				var list = string.IsNullOrEmpty(keyword)
					? await _productService.GetAllProductsAsync()
					: await _productService.SearchProductsAsync(keyword);

				ReplaceItems(list);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async void Reload_Click(object sender, RoutedEventArgs e)
		{
			SearchBox.Text = "";
			await LoadAllAsync();
		}

		

		private async Task LoadAllAsync()
		{
			var list = await _productService.GetAllProductsAsync();
			ReplaceItems(list);

			var total = await _productService.GetTotalCountAsync();
			var placed = await _productService.GetPlacedCountAsync();
			StatsText.Text = $"전체 {total} / 배치 {placed} / 미배치 {total - placed}";
		}

		private void ReplaceItems(System.Collections.Generic.List<Product> list)
		{
			_items.Clear();
			foreach (var p in list) _items.Add(p);
		}

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var win = new ProductEditWindow { Owner = Window.GetWindow(this) };
            if (win.ShowDialog() == true)
                await LoadAllAsync();
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductGrid.SelectedItem is not Product selected) return;

            // 선택된 객체를 편집창으로 전달
            var win = new ProductEditWindow(selected) { Owner = Window.GetWindow(this) };
            if (win.ShowDialog() == true)
                await LoadAllAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductGrid.SelectedItem is not Product selected) return;

            var r = MessageBox.Show($"'{selected.ProductName}' 삭제할까요?",
                "확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (r != MessageBoxResult.Yes) return;

            await _productService.DeleteProductAsync(selected.ProductID);
            await LoadAllAsync();
        }

    }
}
