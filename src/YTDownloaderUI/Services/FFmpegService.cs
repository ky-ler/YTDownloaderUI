using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace YTDownloaderUI.Services;

public class FFmpegService : INotifyPropertyChanged
{
    private static FFmpegService? _instance;
    public static FFmpegService Instance => _instance ??= new FFmpegService();

    private bool _isFFmpegAvailable;
    private string? _ffmpegPath;
    private string? _ffprobePath;

    public bool IsFFmpegAvailable
    {
        get => _isFFmpegAvailable;
        private set
        {
            if (_isFFmpegAvailable != value)
            {
                _isFFmpegAvailable = value;
                OnPropertyChanged();
            }
        }
    }

    public string? FFmpegPath
    {
        get => _ffmpegPath;
        private set
        {
            if (_ffmpegPath != value)
            {
                _ffmpegPath = value;
                OnPropertyChanged();
            }
        }
    }

    public string? FFprobePath
    {
        get => _ffprobePath;
        private set
        {
            if (_ffprobePath != value)
            {
                _ffprobePath = value;
                OnPropertyChanged();
            }
        }
    }

    private FFmpegService()
    {
        ValidateFFmpeg();
    }

    public void ValidateFFmpeg()
    {
        FFmpegPath = FindFFmpeg();
        FFprobePath = FindFFprobe();
        IsFFmpegAvailable = !string.IsNullOrEmpty(FFmpegPath) && !string.IsNullOrEmpty(FFprobePath);
    }

    private static string? FindFFmpeg()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var ffmpegPath = Path.Combine(appDir, "tools", "ffmpeg.exe");
        return File.Exists(ffmpegPath) ? ffmpegPath : null;
    }

    private static string? FindFFprobe()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var ffprobePath = Path.Combine(appDir, "tools", "ffprobe.exe");
        return File.Exists(ffprobePath) ? ffprobePath : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
