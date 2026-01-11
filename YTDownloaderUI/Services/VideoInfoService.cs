using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTDownloaderUI.Models;
using YTDownloaderUI.Properties;

namespace YTDownloaderUI.Services;

public class VideoInfoService
{
    private static VideoInfoService? _instance;
    public static VideoInfoService Instance => _instance ??= new VideoInfoService();

    public ObservableCollection<VideoInfo> Queue { get; } = new();

    private VideoInfoService() { }

    public async Task AddToQueueAsync(string url, string preset, bool audioOnly = false, bool getPlaylist = false, bool getSubtitles = false)
    {
        var videoInfo = new VideoInfo(
            url: url,
            status: "Queued",
            downloadProgress: 0.0,
            audioOnly: audioOnly,
            getPlaylist: getPlaylist,
            getSubtitles: getSubtitles,
            preset: preset);

        Queue.Add(videoInfo);

        // Fire-and-forget title fetch
        _ = FetchTitleAsync(videoInfo);
    }

    public bool IsInQueue(string videoId)
    {
        return Queue.Any(x => x.Url.Contains(videoId));
    }

    private async Task FetchTitleAsync(VideoInfo video)
    {
        if (string.IsNullOrEmpty(Settings.Default.YtDlpLocation) ||
            !File.Exists(Settings.Default.YtDlpLocation))
        {
            return; // Keep URL as fallback
        }

        try
        {
            var originalStatus = video.Status;
            video.Status = "Fetching info...";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Settings.Default.YtDlpLocation,
                    Arguments = $"--get-title --no-playlist \"{video.Url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var titleTask = process.StandardOutput.ReadLineAsync();
            var waitTask = process.WaitForExitAsync(cts.Token);

            try
            {
                await Task.WhenAll(titleTask, waitTask);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(); } catch { }
                video.Status = originalStatus;
                return;
            }

            var title = await titleTask;

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(title))
            {
                video.Title = title.Trim();
            }

            video.Status = originalStatus;
        }
        catch
        {
            // Silently fail - URL will be shown as fallback
            video.Status = "Queued";
        }
    }
}
