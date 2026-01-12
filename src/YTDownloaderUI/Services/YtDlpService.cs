using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace YTDownloaderUI.Services;

public class YtDlpService : INotifyPropertyChanged
{
    private static YtDlpService? _instance;
    public static YtDlpService Instance => _instance ??= new YtDlpService();

    public bool IsYtDlpAvailable
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string? YtDlpPath
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    private YtDlpService()
    {
        ValidateYtDlp();
    }

    public void ValidateYtDlp()
    {
        YtDlpPath = FindYtDlp();
        IsYtDlpAvailable = !string.IsNullOrEmpty(YtDlpPath);
    }

    private static string? FindYtDlp()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var ytDlpPath = Path.Combine(appDir, "tools", "yt-dlp.exe");
        return File.Exists(ytDlpPath) ? ytDlpPath : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
