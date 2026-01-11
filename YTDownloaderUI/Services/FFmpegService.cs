using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using YTDownloaderUI.Properties;

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

    private string? FindFFmpeg()
    {
        // Priority 1: Same directory as yt-dlp.exe
        if (!string.IsNullOrEmpty(Settings.Default.YtDlpLocation))
        {
            var ytdlpDir = Path.GetDirectoryName(Settings.Default.YtDlpLocation);
            if (!string.IsNullOrEmpty(ytdlpDir))
            {
                var ffmpegInYtdlp = Path.Combine(ytdlpDir, "ffmpeg.exe");
                if (File.Exists(ffmpegInYtdlp))
                    return ffmpegInYtdlp;
            }
        }

        // Priority 2: Application directory
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var ffmpegInApp = Path.Combine(appDir, "ffmpeg.exe");
        if (File.Exists(ffmpegInApp))
            return ffmpegInApp;

        return null;
    }

    private string? FindFFprobe()
    {
        // Priority 1: Same directory as yt-dlp.exe
        if (!string.IsNullOrEmpty(Settings.Default.YtDlpLocation))
        {
            var ytdlpDir = Path.GetDirectoryName(Settings.Default.YtDlpLocation);
            if (!string.IsNullOrEmpty(ytdlpDir))
            {
                var ffprobeInYtdlp = Path.Combine(ytdlpDir, "ffprobe.exe");
                if (File.Exists(ffprobeInYtdlp))
                    return ffprobeInYtdlp;
            }
        }

        // Priority 2: Application directory
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var ffprobeInApp = Path.Combine(appDir, "ffprobe.exe");
        if (File.Exists(ffprobeInApp))
            return ffprobeInApp;

        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
