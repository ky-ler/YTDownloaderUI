using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YTDownloaderUI.Models;

public class VideoInfo : INotifyPropertyChanged
{
    public VideoInfo(string url, string status, double downloadProgress, bool getPlaylist, bool getSubtitles, string preset = "")
    {
        Url = url;
        Status = status;
        DownloadProgress = downloadProgress;
        GetPlaylist = getPlaylist;
        GetSubtitles = getSubtitles;
        Preset = preset;
    }

    public string Url { get; set; }

    public string? Title
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    /// <summary>
    /// Returns Title if available, otherwise falls back to Url
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(Title) ? Title : Url;

    public bool GetPlaylist { get; set; }

    public bool GetSubtitles { get; set; }

    public string Preset { get; set; }

    /// <summary>
    /// Returns a user-friendly preset display name
    /// </summary>
    public string PresetDisplay => string.IsNullOrEmpty(Preset) ? "Default" : Preset.ToUpper();

    /// <summary>
    /// Returns a tooltip describing the download options
    /// </summary>
    public string OptionsTooltip
    {
        get
        {
            var options = new System.Collections.Generic.List<string>();
            if (GetPlaylist) options.Add("Playlist");
            if (GetSubtitles) options.Add("Subtitles");
            return options.Count > 0 ? string.Join(", ", options) : "No extra options";
        }
    }

    public string Status
    {
        get;
        set
        {
            var oldValue = field;

            if (value == oldValue) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public double DownloadProgress
    {
        get;
        set
        {
            var oldValue = field;

            const double tolerance = 0.0001;
            if (Math.Abs(value - oldValue) < tolerance) return;
            field = value;
            OnPropertyChanged();
        }
    } = 0.0;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
