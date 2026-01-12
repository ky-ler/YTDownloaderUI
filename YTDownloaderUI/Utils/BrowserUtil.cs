using System.Diagnostics;

namespace YTDownloaderUI.Utils;

public static class BrowserUtil
{
    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
