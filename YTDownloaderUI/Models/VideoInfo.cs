namespace YTDownloaderUI.Models;

public class VideoInfo
{
    public string Url { get; set; } = string.Empty;

    public string Status { get; set; } = "Queued";

    public float DownloadProgress { get; set; }
}
