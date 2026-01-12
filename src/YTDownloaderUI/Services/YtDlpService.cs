using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace YTDownloaderUI.Services;

public class YtDlpService : INotifyPropertyChanged
{
    private static YtDlpService? _instance;
    public static YtDlpService Instance => _instance ??= new YtDlpService();

    private bool _isYtDlpAvailable;
    private string? _ytDlpPath;

    public bool IsYtDlpAvailable
    {
        get => _isYtDlpAvailable;
        private set
        {
            if (_isYtDlpAvailable != value)
            {
                _isYtDlpAvailable = value;
                OnPropertyChanged();
            }
        }
    }

    public string? YtDlpPath
    {
        get => _ytDlpPath;
        private set
        {
            if (_ytDlpPath != value)
            {
                _ytDlpPath = value;
                OnPropertyChanged();
            }
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
