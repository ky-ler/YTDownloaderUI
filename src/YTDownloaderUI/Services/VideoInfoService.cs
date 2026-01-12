using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTDownloaderUI.Models;

namespace YTDownloaderUI.Services;

public class VideoInfoService : INotifyPropertyChanged
{
    private static VideoInfoService? _instance;
    public static VideoInfoService Instance => _instance ??= new VideoInfoService();

    public ObservableCollection<VideoInfo> Queue { get; } = [];

    private int _activeFetchCount;
    public bool IsFetchingTitles => _activeFetchCount > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private VideoInfoService() { }

    public async Task AddToQueueAsync(string url, string preset, bool getPlaylist = false, bool getSubtitles = false)
    {
        var videoInfo = new VideoInfo(
            url: url,
            status: "Queued",
            downloadProgress: 0.0,
            getPlaylist: getPlaylist,
            getSubtitles: getSubtitles,
            preset: preset);

        Queue.Add(videoInfo);

        // Fire-and-forget title fetch
        _ = FetchTitleAsync(videoInfo);
    }

    public VideoInfo? FindDuplicate(string videoId, string preset, bool getPlaylist, bool getSubtitles)
    {
        return Queue.FirstOrDefault(x =>
            x.Url.Contains(videoId) &&
            x.Preset == preset &&
            x.GetPlaylist == getPlaylist &&
            x.GetSubtitles == getSubtitles);
    }

    public void ReplaceInQueue(VideoInfo existing, string url, string preset, bool getPlaylist, bool getSubtitles)
    {
        var index = Queue.IndexOf(existing);
        if (index >= 0)
        {
            var replacement = new VideoInfo(
                url: url,
                status: "Queued",
                downloadProgress: 0.0,
                getPlaylist: getPlaylist,
                getSubtitles: getSubtitles,
                preset: preset);

            Queue[index] = replacement;

            // Fire-and-forget title fetch
            _ = FetchTitleAsync(replacement);
        }
    }

    private async Task FetchTitleAsync(VideoInfo video)
    {
        var ytDlpPath = YtDlpService.Instance.YtDlpPath;
        if (!YtDlpService.Instance.IsYtDlpAvailable || string.IsNullOrEmpty(ytDlpPath))
        {
            return; // Keep URL as fallback
        }

        Interlocked.Increment(ref _activeFetchCount);
        OnPropertyChanged(nameof(IsFetchingTitles));

        try
        {
            var originalStatus = video.Status;
            video.Status = "Fetching info...";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
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
        finally
        {
            Interlocked.Decrement(ref _activeFetchCount);
            OnPropertyChanged(nameof(IsFetchingTitles));
        }
    }
}
