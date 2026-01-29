using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using YTDownloaderUI.Models;

namespace YTDownloaderUI.Services;

public interface IDownloadService
{
    Task ProcessQueueAsync(ObservableCollection<VideoInfo> queue, CancellationToken cancellationToken);
    void CancelCurrentDownload();
}
