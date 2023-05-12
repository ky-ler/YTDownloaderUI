using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YTDownloaderUI.Models;
using YTDownloaderUI.Properties;

namespace YTDownloaderUI.Utils;

public static class DownloadUtil
{
    private static async Task Download(VideoInfo video)
    {
        Process _p = new();
        _p.StartInfo.FileName = Settings.Default.YtDlpLocation;
        _p.StartInfo.UseShellExecute = false;
        _p.StartInfo.CreateNoWindow = true;
        _p.StartInfo.Arguments = $"-q -P {AppDomain.CurrentDomain.BaseDirectory}/downloads/ ";
        _p.StartInfo.Arguments += video.Url;

        try
        {
            if (!_p.Start())
                throw new Exception("yt-dlg failed to start.");

            await _p.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    // This should probably be threaded
    public static async Task ProcessQueue(ObservableCollection<VideoInfo> videos)
    {
        if (!File.Exists(Settings.Default.YtDlpLocation))
            return;

        foreach (var video in videos)
        {
            await Download(video);
        }
    }
}
