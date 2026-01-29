using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTDownloaderUI.Services;

public class YtDlpService : IYtDlpService, INotifyPropertyChanged
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

    public async Task<string?> GetTitleAsync(string url)
    {
        if (!IsYtDlpAvailable || string.IsNullOrEmpty(YtDlpPath))
        {
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = $"--get-title --no-playlist \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read all output - title may not be on first line if there are warnings
            var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            var waitTask = process.WaitForExitAsync(cts.Token);

            try
            {
                await Task.WhenAll(outputTask, waitTask);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); }
                catch
                {
                    // ignored
                }
                return null;
            }

            var output = await outputTask;

            // Get the last non-empty line (the title)
            var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var title = lines.Length > 0 ? lines[^1] : null;

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(title))
            {
                return title.Trim();
            }

            return null;
        }
        catch
        {
            return null;
        }
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
