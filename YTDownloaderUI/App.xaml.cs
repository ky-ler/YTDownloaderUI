using System.Windows;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Services;

namespace YTDownloaderUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services at startup
            _ = YtDlpService.Instance;
            _ = FFmpegService.Instance;
            _ = VideoInfoService.Instance;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
