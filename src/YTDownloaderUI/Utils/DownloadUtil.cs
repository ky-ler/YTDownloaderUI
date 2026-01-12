using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTDownloaderUI.Models;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Services;

namespace YTDownloaderUI.Utils;

public class DownloadUtil
{
    private static Process? _currentProcess;
    private static readonly Lock ProcessLock = new();

    private static async Task Download(VideoInfo video, CancellationToken cancellationToken)
    {
        Process process = new();
        var cmdArgs = GetYtDlpOptions(video);
        process.StartInfo.FileName = YtDlpService.Instance.YtDlpPath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;
        process.StartInfo.Arguments = cmdArgs;

        try
        {
            if (!process.Start())
                throw new Exception("yt-dlp failed to start.");

            lock (ProcessLock)
            {
                _currentProcess = process;
            }

            video.Status = "Downloading";

            // Register cancellation to kill the process
            await using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // ignored
                }
            });

            await Task.WhenAll(
                GetDownloadProgress(process.StandardOutput, video, cancellationToken),
                process.WaitForExitAsync(cancellationToken)
            );

            video.Status = "Finished";
        }
        catch (OperationCanceledException)
        {
            video.Status = "Cancelled";
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            lock (ProcessLock)
            {
                _currentProcess = null;
            }
            process.Dispose();
        }
    }

    public static async Task ProcessQueue(ObservableCollection<VideoInfo> videos, CancellationToken cancellationToken)
    {
        if (!YtDlpService.Instance.IsYtDlpAvailable)
            return;

        foreach (var video in videos)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (video.Status is "Finished" or "Cancelled") continue;

            try
            {
                await Download(video, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Mark remaining queued items as cancelled
                foreach (var v in videos)
                {
                    if (v.Status == "Queued")
                        v.Status = "Cancelled";
                }
                throw;
            }
        }
    }

    public static void CancelCurrentDownload()
    {
        lock (ProcessLock)
        {
            try
            {
                if (_currentProcess is { HasExited: false })
                {
                    _currentProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private static async Task GetDownloadProgress(StreamReader output, VideoInfo video, CancellationToken cancellationToken)
    {
        // yt-dlp download status lines begin with [download]
        const string lineStart = "[download] ";
        const string findPercentage = "% of ";

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await output.ReadLineAsync(cancellationToken);

            // Loop exit condition. line == null when yt-dlp exits
            if (line == null) return;

            if (!line.StartsWith(lineStart)) continue;

            // lineStart.Length = 11
            line = line[11..];

            var percentageIndex = line.IndexOf(findPercentage, StringComparison.Ordinal);

            // Shouldn't happen, but check anyways
            if (percentageIndex < 0) continue;

            var percentage = Convert.ToDouble(line[..percentageIndex]) / 100;

            video.DownloadProgress = percentage;
        }
    }

    private static string GetYtDlpOptions(VideoInfo video)
    {
        // Use custom download directory if set, otherwise default to ./downloads
        var customPath = Settings.Default.DownloadDirectory;
        var downloadsPath = string.IsNullOrWhiteSpace(customPath)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads")
            : customPath;

        // Ensure the directory exists
        Directory.CreateDirectory(downloadsPath);

        var args = $"-P \"{downloadsPath}\" --force-overwrites ";

        // Output template: include preset suffix when a preset is specified
        if (!string.IsNullOrEmpty(video.Preset))
        {
            args += $"-o \"%(title)s_[{video.Preset}].%(ext)s\" ";
        }

        if (video.GetPlaylist)
            args += "--yes-playlist ";
        else
            args += "--no-playlist ";

        if (video.GetSubtitles)
            args += "--write-subs --write-auto-subs --sub-lang en ";

        // a lot of yt-dlp post-processing requires ffmpeg and/or ffprobe
        var ffmpegService = FFmpegService.Instance;
        if (ffmpegService.IsFFmpegAvailable)
        {
            args += $"--ffmpeg-location \"{ffmpegService.FFmpegPath}\" ";
        }

        // Apply preset alias
        if (!string.IsNullOrEmpty(video.Preset))
        {
            args += $"-t {video.Preset} ";
        }

        args += video.Url;

        return args;
    }
}
