using System;
using System.IO;
using Wpf.Ui.Appearance;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Views.Pages;

namespace YTDownloaderUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        DataContext = this;

        InitializeComponent();

        if (Settings.Default.Theme == "Light")
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
        }
        else
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        }

        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));
    }
}
