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

        SystemThemeWatcher.Watch(this);

        InitializeComponent();

        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));
    }
}
