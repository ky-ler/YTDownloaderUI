using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YTDownloaderUI.Models;

namespace YTDownloaderUI.Services;

public class VideoInfoService : INotifyPropertyChanged
{
    private static VideoInfoService? _instance;
    public static VideoInfoService Instance => _instance ??= new VideoInfoService(YtDlpService.Instance);

    private readonly IYtDlpService _ytDlpService;

    public ObservableCollection<VideoInfo> Queue { get; } = [];

    private int _activeFetchCount;
    public bool IsFetchingTitles => _activeFetchCount > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal VideoInfoService(IYtDlpService ytDlpService)
    {
        _ytDlpService = ytDlpService;
    }

    public Task AddToQueueAsync(string url, string preset, bool getPlaylist = false, bool getSubtitles = false)
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
        return Task.CompletedTask;
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
        if (index < 0) return;
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

    private async Task FetchTitleAsync(VideoInfo video)
    {
        if (!_ytDlpService.IsYtDlpAvailable)
        {
            return; // Keep URL as fallback
        }

        Interlocked.Increment(ref _activeFetchCount);
        OnPropertyChanged(nameof(IsFetchingTitles));

        try
        {
            var originalStatus = video.Status;
            video.Status = "Fetching info...";

            var title = await _ytDlpService.GetTitleAsync(video.Url);

            if (!string.IsNullOrWhiteSpace(title))
            {
                video.Title = title;
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
