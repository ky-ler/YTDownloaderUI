using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace YTDownloaderUI.Utils;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string;
        return status switch
        {
            "Error" or "Failed" => Brushes.Red,
            "Finished" => Brushes.Green,
            "Cancelled" => Brushes.Gray,
            _ => Application.Current.Resources["TextFillColorPrimaryBrush"] ?? Brushes.White
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
