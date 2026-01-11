using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YTDownloaderUI.Models;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Services;

namespace YTDownloaderUI.Utils;

public class DownloadUtil
{
    private static async Task Download(VideoInfo video)
    {
        Process _p = new();
        var cmdArgs = GetYtDlpOptions(video);
        _p.StartInfo.FileName = Settings.Default.YtDlpLocation;
        _p.StartInfo.RedirectStandardOutput = true;
        _p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        _p.StartInfo.UseShellExecute = false;
        _p.StartInfo.CreateNoWindow = true;
        _p.EnableRaisingEvents = true;
        _p.StartInfo.Arguments = cmdArgs;

        try
        {
            if (!_p.Start())
                throw new Exception("yt-dlg failed to start.");

            video.Status = "Downloading";

            await Task.WhenAll(
                GetDownloadProgress(_p.StandardOutput, video),
                _p.WaitForExitAsync()
            );

            video.Status = "Finished";
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
            if (video.Status == "Finished") continue;

            await Download(video);
        }
    }

    private static async Task GetDownloadProgress(StreamReader output, VideoInfo video)
    {
        // yt-dlg download status lines begin with [download]
        const string lineStart = "[download] ";
        const string findPercentage = "% of ";

        while (true)
        {
            var line = await output.ReadLineAsync();

            // Loop exit condition. line == null when yt-dlp exits
            if (line == null) return;

            if (!line.StartsWith(lineStart)) continue;

            // lineStart.Length = 11
            line = line[11..];

            int percentageIndex = line.IndexOf(findPercentage);

            // Shouldn't happen, but check anyways
            if (percentageIndex < 0) continue;

            double percentage = Convert.ToDouble(line[..percentageIndex]) / 100;

            video.DownloadProgress = percentage;
        }
    }

    private static string GetYtDlpOptions(VideoInfo video)
    {
        string args = "-P ";

        /*if (string.IsNullOrEmpty(Settings.Default.DownloadLocation))*/
        args += $"{AppDomain.CurrentDomain.BaseDirectory}/downloads/ ";
        /*        else
                    args += $"{Settings.Default.DownloadLocation} ";*/

        if (video.GetPlaylist)
            args += "--yes-playlist ";
        else
            args += "--no-playlist ";

        if (video.GetSubtitles)
            args += "--write-subs ";

        // a lot of yt-dlp post-processing requires ffmpeg and/or ffprobe
        var ffmpegService = FFmpegService.Instance;
        if (ffmpegService.IsFFmpegAvailable)
        {
            args += $"--ffmpeg-location \"{ffmpegService.FFmpegPath}\" ";

            if (video.AudioOnly)
                args += "-x ";
        }

        if (!string.IsNullOrEmpty(video.Preset))
            args += $"-t {video.Preset} ";

        args += video.Url;

        return args;
    }
}
