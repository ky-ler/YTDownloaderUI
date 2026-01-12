// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Services;
using YTDownloaderUI.Utils;

namespace YTDownloaderUI.Views.Pages;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage
{
    private readonly VideoInfoService _videoInfoService;
    private readonly FFmpegService _ffmpegService;
    private readonly YtDlpService _ytDlpService;
    private CancellationTokenSource? _downloadCts;

    private static string GetDownloadDirectory()
    {
        var configuredPath = Settings.Default.DownloadDirectory;

        // Use configured directory if set, otherwise default to ./downloads
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return configuredPath;

        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");
    }

    public HomePage()
    {
        InitializeComponent();

        _videoInfoService = VideoInfoService.Instance;
        _ffmpegService = FFmpegService.Instance;
        _ytDlpService = YtDlpService.Instance;

        // Bind queue from service
        QueueList.ItemsSource = _videoInfoService.Queue;

        // Subscribe to queue changes
        _videoInfoService.Queue.CollectionChanged += (s, e) =>
        {
            Dispatcher.Invoke(UpdateQueueButtonStates);
        };

        UpdateUIState();
        UpdatePresetAvailability();
        UpdateQueueButtonStates();

        // Subscribe to availability changes
        _ffmpegService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FFmpegService.IsFFmpegAvailable))
            {
                Dispatcher.Invoke(UpdatePresetAvailability);
            }
        };

        _ytDlpService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(YtDlpService.IsYtDlpAvailable))
            {
                Dispatcher.Invoke(UpdateUIState);
            }
        };

        _videoInfoService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VideoInfoService.IsFetchingTitles))
            {
                Dispatcher.Invoke(UpdateQueueButtonStates);
            }
        };
    }

    private void UpdateUIState()
    {
        if (!_ytDlpService.IsYtDlpAvailable)
        {
            StatusText.Text = "yt-dlp.exe not found. Please reinstall the application.";
        }
        else
        {
            StatusText.Text = "";
        }
        UpdateQueueButtonStates();
    }

    private void UpdateQueueButtonStates()
    {
        bool hasItems = _videoInfoService.Queue.Count > 0;
        bool isFetching = _videoInfoService.IsFetchingTitles;
        bool canDownload = hasItems && _ytDlpService.IsYtDlpAvailable && !isFetching;

        StartDownloadButton.IsEnabled = canDownload;
        ClearQueueButton.IsEnabled = hasItems;
    }

    private void UpdatePresetAvailability()
    {
        bool ffmpegAvailable = _ffmpegService.IsFFmpegAvailable;

        // Disable all format presets when ffmpeg is not available
        foreach (var item in PresetComboBox.Items.Cast<ComboBoxItem>())
        {
            var tag = item.Tag?.ToString() ?? "";
            // mp3, aac, mp4, mkv all require ffmpeg
            if (tag == "mp3" || tag == "aac" || tag == "mp4" || tag == "mkv")
            {
                item.IsEnabled = ffmpegAvailable;
                if (!ffmpegAvailable && PresetComboBox.SelectedItem == item)
                {
                    PresetComboBox.SelectedIndex = 0; // Reset to Default
                }
            }
        }

        // Show warning if ffmpeg not found
        FFmpegWarning.Visibility = ffmpegAvailable ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void AddToQueue_Button_Click(object sender, RoutedEventArgs e)
    {
        var urls = UrlList.Text.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries)
                                .Where(x => x.Contains("youtu.be/") || x.Contains("youtube.com/"))
                                .Select(x => x.Trim())
                                .ToList();

        var selectedPreset = (PresetComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
        var getSubtitles = SubtitlesCheckBox.IsChecked == true;

        int duplicatesSkipped = 0;

        foreach (var url in urls)
        {
            var isPlaylistUrl = UrlParser.IsPlaylistUrl(url);
            var normalizedUrl = UrlParser.NormalizeUrl(url);

            // Auto-enable playlist download for playlist URLs, otherwise use checkbox
            var getPlaylist = isPlaylistUrl || PlaylistCheckBox.IsChecked == true;

            // Get the unique identifier for duplicate checking
            var uniqueId = isPlaylistUrl && !url.Contains("watch?v=")
                ? UrlParser.GetPlaylistId(url) ?? normalizedUrl
                : UrlParser.GetVideoId(url);

            // Check for duplicates (same video/playlist with same options)
            var existingDuplicate = _videoInfoService.FindDuplicate(uniqueId, selectedPreset, getPlaylist, getSubtitles);

            if (existingDuplicate == null)
            {
                // No duplicate, add to queue
                await _videoInfoService.AddToQueueAsync(normalizedUrl, selectedPreset, getPlaylist, getSubtitles);
            }
            else if (existingDuplicate.Status == "Cancelled" || existingDuplicate.Status == "Finished")
            {
                // Duplicate exists but is cancelled or finished, replace it
                _videoInfoService.ReplaceInQueue(existingDuplicate, normalizedUrl, selectedPreset, getPlaylist, getSubtitles);
            }
            else
            {
                // Duplicate is still queued or downloading, skip
                duplicatesSkipped++;
            }
        }

        // Show duplicate warning if any were skipped
        if (duplicatesSkipped > 0)
        {
            DuplicateWarning.Message = duplicatesSkipped == 1
                ? "1 URL was already in the queue with the same options and was skipped."
                : $"{duplicatesSkipped} URLs were already in the queue with the same options and were skipped.";
            DuplicateWarningContainer.Visibility = Visibility.Visible;
        }
        else
        {
            DuplicateWarningContainer.Visibility = Visibility.Collapsed;
        }

        UrlList.Text = "";
        UpdateQueueButtonStates();
    }

    private void UrlList_TextChanged(object sender, TextChangedEventArgs e)
    {
        AddToQueueButton.IsEnabled = !string.IsNullOrWhiteSpace(UrlList.Text);
    }

    private void ClearQueue_Button_Click(object sender, RoutedEventArgs e)
    {
        _videoInfoService.Queue.Clear();
    }

    private async void StartDownload_Button_Click(object sender, RoutedEventArgs e)
    {
        // Get the download directory and create it if it doesn't exist
        var downloadPath = GetDownloadDirectory();
        if (!System.IO.Directory.Exists(downloadPath))
            System.IO.Directory.CreateDirectory(downloadPath);

        // Re-run detection (catches newly added executables since app start)
        _ytDlpService.ValidateYtDlp();
        _ffmpegService.ValidateFFmpeg();
        UpdateUIState();
        UpdatePresetAvailability();

        // Validate yt-dlp exists
        if (!_ytDlpService.IsYtDlpAvailable)
        {
            MessageBox.Show(
                "yt-dlp.exe not found.\n\n" +
                "The application may be corrupted. Please reinstall YTDownloaderUI.",
                "yt-dlp Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        // Check if any queue items require FFmpeg
        var requiresFFmpeg = _videoInfoService.Queue
            .Any(v => v.Preset == "mp3" || v.Preset == "aac" || v.Preset == "mp4" || v.Preset == "mkv");

        if (requiresFFmpeg && !_ffmpegService.IsFFmpegAvailable)
        {
            MessageBox.Show(
                "Some items in the queue require FFmpeg.\n\n" +
                "Download FFmpeg from ffmpeg.org and place ffmpeg.exe and ffprobe.exe in the tools folder.",
                "FFmpeg Required",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Set up UI for downloading state
        StartDownloadButton.IsEnabled = false;
        CancelDownloadButton.Visibility = Visibility.Visible;
        ClearQueueButton.IsEnabled = false;

        _downloadCts = new CancellationTokenSource();

        try
        {
            await DownloadUtil.ProcessQueue(_videoInfoService.Queue, _downloadCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Download was cancelled - this is expected
        }
        finally
        {
            _downloadCts.Dispose();
            _downloadCts = null;

            // Restore UI state
            CancelDownloadButton.Visibility = Visibility.Collapsed;
            UpdateQueueButtonStates();
        }
    }

    private void CancelDownload_Button_Click(object sender, RoutedEventArgs e)
    {
        _downloadCts?.Cancel();
        DownloadUtil.CancelCurrentDownload();
    }

    private void RefreshFFmpeg_Click(object sender, RoutedEventArgs e)
    {
        _ffmpegService.ValidateFFmpeg();
        UpdatePresetAvailability();
    }

    private void DuplicateWarning_Dismiss_Click(object sender, RoutedEventArgs e)
    {
        DuplicateWarningContainer.Visibility = Visibility.Collapsed;
    }
}
