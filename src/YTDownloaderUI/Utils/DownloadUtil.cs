using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(30);
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.log");
    private static readonly Lock LogLock = new();

    private static void WriteLog(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var line = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
        lock (LogLock)
        {
            File.AppendAllText(LogFilePath, line);
        }
    }

    private static async Task Download(VideoInfo video, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(DownloadTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        Process process = new();
        var cmdArgs = GetYtDlpOptions(video);
        process.StartInfo.FileName = YtDlpService.Instance.YtDlpPath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;
        process.StartInfo.Arguments = cmdArgs;

        var errorBuilder = new StringBuilder();

        WriteLog("INFO", $"Starting download for: {video.Url}");
        WriteLog("INFO", $"Command: {process.StartInfo.FileName} {cmdArgs}");

        try
        {
            if (!process.Start())
                throw new DownloadException("yt-dlp failed to start.");

            lock (ProcessLock)
            {
                _currentProcess = process;
            }

            video.Status = "Downloading";
            video.ErrorMessage = null; // Clear previous error

            // Register cancellation to kill the process
            await using var registration = linkedToken.Register(() =>
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

            // Read stderr in background
            var errorTask = ReadStderrAsync(process.StandardError, errorBuilder, linkedToken);

            await Task.WhenAll(
                GetDownloadProgress(process.StandardOutput, video, linkedToken),
                errorTask,
                process.WaitForExitAsync(linkedToken)
            );

            // Check exit code
            WriteLog("INFO", $"Process exited with code: {process.ExitCode}");
            if (process.ExitCode != 0)
            {
                var errorMessage = ParseYtDlpError(errorBuilder.ToString(), process.ExitCode);
                throw new DownloadException(errorMessage);
            }

            video.Status = "Finished";
            WriteLog("INFO", $"Download finished: {video.Url}");
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            video.Status = "Error";
            video.ErrorMessage = "Download timed out after 30 minutes";
            WriteLog("ERROR", $"Timeout: {video.Url}");
            throw new DownloadException(video.ErrorMessage);
        }
        catch (OperationCanceledException)
        {
            video.Status = "Cancelled";
            WriteLog("INFO", $"Cancelled: {video.Url}");
            throw;
        }
        catch (DownloadException ex)
        {
            video.Status = "Error";
            video.ErrorMessage = ex.Message;
            WriteLog("ERROR", $"Download error for {video.Url}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            video.Status = "Error";
            video.ErrorMessage = $"Unexpected error: {ex.Message}";
            WriteLog("ERROR", $"Unexpected error for {video.Url}: {ex}");
            throw new DownloadException(video.ErrorMessage, ex);
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

    private static async Task ReadStderrAsync(StreamReader stderr, StringBuilder builder, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var line = await stderr.ReadLineAsync(ct);
                if (line == null) break;
                WriteLog("STDERR", line);
                builder.AppendLine(line);
            }
        }
        catch (OperationCanceledException) { /* expected */ }
    }

    private static string ParseYtDlpError(string stderr, int exitCode)
    {
        // Common yt-dlp error patterns
        if (stderr.Contains("Video unavailable"))
            return "Video is unavailable (private, deleted, or region-locked)";

        if (stderr.Contains("Sign in to confirm your age"))
            return "Video requires age verification";

        if (stderr.Contains("Private video"))
            return "This video is private";

        if (stderr.Contains("ERROR: Unable to download webpage") ||
            stderr.Contains("URLError") ||
            stderr.Contains("Connection refused") ||
            stderr.Contains("timed out"))
            return "Network error - check your internet connection";

        if (stderr.Contains("ERROR: Incomplete YouTube ID"))
            return "Invalid YouTube video ID";

        if (stderr.Contains("HTTP Error 404"))
            return "Video not found (404)";

        if (stderr.Contains("HTTP Error 403"))
            return "Access denied (403)";

        if (stderr.Contains("ffmpeg") && stderr.Contains("not found"))
            return "FFmpeg required but not found";

        // Extract first ERROR line if no specific match
        var match = Regex.Match(stderr, @"ERROR:\s*(.+)");
        if (match.Success)
            return match.Groups[1].Value.Trim();

        return $"Download failed (exit code {exitCode})";
    }

    public static async Task ProcessQueue(ObservableCollection<VideoInfo> videos, CancellationToken cancellationToken)
    {
        if (!YtDlpService.Instance.IsYtDlpAvailable)
            return;

        foreach (var video in videos)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Skip finished, cancelled, or permanently failed
            if (video.Status is "Finished" or "Cancelled") continue;
            if (video.Status == "Failed") continue; // Max retries exceeded

            // Handle retry case
            if (video.Status == "Error")
            {
                if (video.RetryCount >= VideoInfo.MaxRetries)
                {
                    video.Status = "Failed";
                    continue;
                }
                video.RetryCount++;
            }

            try
            {
                await Download(video, cancellationToken);
            }
            catch (DownloadException)
            {
                // Error already set in Download method
                // Continue to next video instead of stopping
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

            WriteLog("STDOUT", line);

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

        // Apply preset — for mp4, expand manually to add proto:https which
        // prefers progressive streams over DASH (avoids 403 errors on fragments)
        if (video.Preset == "mp4")
        {
            args += "--merge-output-format mp4 --remux-video mp4 " +
                    "-S \"proto:https,vcodec:h264,lang,quality,res,fps,hdr:12,acodec:aac\" ";
        }
        else if (!string.IsNullOrEmpty(video.Preset))
        {
            args += $"-t {video.Preset} ";
        }

        args += video.Url;

        return args;
    }
}
