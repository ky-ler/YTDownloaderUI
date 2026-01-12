using System.Windows;
using YTDownloaderUI.Services;

namespace YTDownloaderUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _ = YtDlpService.Instance;
            _ = FFmpegService.Instance;
            _ = VideoInfoService.Instance;
        }
    }
}
