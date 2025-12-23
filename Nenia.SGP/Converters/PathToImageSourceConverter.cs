using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Nenia.SGP.Converters;

public sealed class PathToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var path = value as string;
        if (string.IsNullOrWhiteSpace(path)) return null;

        var abs = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(abs)) return null;

        var bi = new BitmapImage();
        bi.BeginInit();
        bi.CacheOption = BitmapCacheOption.OnLoad;
        bi.UriSource = new Uri(abs, UriKind.Absolute);
        bi.EndInit();
        bi.Freeze();
        return bi;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
