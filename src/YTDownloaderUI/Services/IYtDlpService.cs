using System.Threading.Tasks;

namespace YTDownloaderUI.Services;

public interface IYtDlpService
{
    bool IsYtDlpAvailable { get; }
    string? YtDlpPath { get; }
    Task<string?> GetTitleAsync(string url);
}
