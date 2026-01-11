using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YTDownloaderUI.Models;

public class VideoInfo : INotifyPropertyChanged
{
    private string status = "Queued";
    private double downloadProgress = 0.0;
    private string? title;

    public VideoInfo(string url, string status, double downloadProgress, bool audioOnly, bool getPlaylist, bool getSubtitles, string preset = "")
    {
        Url = url;
        Status = status;
        DownloadProgress = downloadProgress;
        AudioOnly = audioOnly;
        GetPlaylist = getPlaylist;
        GetSubtitles = getSubtitles;
        Preset = preset;
    }

    public string Url { get; set; }

    public string? Title
    {
        get => title;
        set
        {
            if (title != value)
            {
                title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    /// <summary>
    /// Returns Title if available, otherwise falls back to Url
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(Title) ? Title : Url;

    public bool AudioOnly { get; set; } = false;

    public bool GetPlaylist { get; set; } = false;

    public bool GetSubtitles { get; set; } = false;

    public string Preset { get; set; } = string.Empty;

    /// <summary>
    /// Returns a user-friendly preset display name
    /// </summary>
    public string PresetDisplay => string.IsNullOrEmpty(Preset) ? "(None)" : Preset.ToUpper();

    public string Status
    {
        get => status;
        set
        {
            string newValue = value;
            string oldValue = status;

            if (newValue != oldValue)
            {
                status = newValue;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public double DownloadProgress
    {
        get => downloadProgress;
        set
        {
            double newValue = value;
            double oldValue = downloadProgress;

            if (newValue != oldValue)
            {
                downloadProgress = newValue;
                OnPropertyChanged(nameof(DownloadProgress));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        if (PropertyChanged == null)
            return;

        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
