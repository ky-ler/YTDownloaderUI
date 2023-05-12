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

        Watcher.Watch(this);

        InitializeComponent();

        Height = Settings.Default.WindowHeight;
        Width = Settings.Default.WindowWidth;

        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));
    }

    private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Settings.Default.WindowHeight = Height;
        Settings.Default.WindowWidth = Width;

        Settings.Default.Save();
    }

    private void HomeView_Click(object sender, System.Windows.RoutedEventArgs e)
    {
    }

    private void SettingsView_Click(object sender, System.Windows.RoutedEventArgs e)
    {
    }
}
