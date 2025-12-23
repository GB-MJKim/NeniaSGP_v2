using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Nenia.SGP.Views;

namespace Nenia.SGP
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Type> _pages = new()
        {
            ["ProductList"] = typeof(ProductListPage),
            ["RegionManagement"] = typeof(RegionManagementPage),
            ["PageLayout"] = typeof(PageLayoutPage),
            ["PriceSetting"] = typeof(PriceSettingPage),
            ["Backup"] = typeof(BackupPage),
        };

        public MainWindow()
        {
            InitializeComponent();
            Menu.SelectedIndex = 0;
        }

        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Menu.SelectedItem is not ListBoxItem item) return;
            var tag = item.Tag?.ToString() ?? "";
            if (_pages.TryGetValue(tag, out var pageType))
                ContentFrame.Navigate(Activator.CreateInstance(pageType));
        }
    }
}
