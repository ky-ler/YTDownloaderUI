// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
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

    public HomePage()
    {
        if (Settings.Default.Theme == "Light")
            Theme.Apply(ThemeType.Light);
        else
            Theme.Apply(ThemeType.Dark);

        InitializeComponent();

        _videoInfoService = VideoInfoService.Instance;
        _ffmpegService = FFmpegService.Instance;

        // Bind queue from service
        QueueList.ItemsSource = _videoInfoService.Queue;

        UpdateUIState();
        UpdatePresetAvailability();

        // Subscribe to FFmpeg availability changes
        _ffmpegService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FFmpegService.IsFFmpegAvailable))
            {
                Dispatcher.Invoke(UpdatePresetAvailability);
            }
        };
    }

    private void UpdateUIState()
    {
        if (string.IsNullOrEmpty(Settings.Default.YtDlpLocation))
        {
            StartDownload_Button.IsEnabled = false;
            StatusText.Text = "Set your yt-dlp.exe location in settings!";
        }
        else
        {
            StartDownload_Button.IsEnabled = true;
            StatusText.Text = "";
        }
    }

    private void UpdatePresetAvailability()
    {
        bool ffmpegAvailable = _ffmpegService.IsFFmpegAvailable;

        // Disable audio-only presets when ffmpeg is not available
        foreach (var item in PresetComboBox.Items.Cast<ComboBoxItem>())
        {
            var tag = item.Tag?.ToString() ?? "";
            // mp3 and aac require ffmpeg for audio extraction
            if (tag == "mp3" || tag == "aac")
            {
                item.IsEnabled = ffmpegAvailable;
                if (!ffmpegAvailable && PresetComboBox.SelectedItem == item)
                {
                    PresetComboBox.SelectedIndex = 0; // Reset to (None)
                }
            }
        }

        // Show warning if ffmpeg not found
        FFmpegWarning.Visibility = ffmpegAvailable ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void AddToQueue_Button_Click(object sender, RoutedEventArgs e)
    {
        var urls = UrlList.Text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(x => x.Contains("youtu.be/") || x.Contains("youtube.com/"))
                                .Select(x => x.Trim())
                                .ToList();

        var selectedPreset = (PresetComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

        foreach (var url in urls)
        {
            var videoId = UrlParser.GetVideoId(url);
            var normalizedUrl = $"https://youtu.be/{videoId}";

            // Check for duplicates
            if (!_videoInfoService.IsInQueue(videoId))
            {
                await _videoInfoService.AddToQueueAsync(normalizedUrl, selectedPreset);
            }
        }

        UrlList.Text = "";
    }

    private async void StartDownload_Button_Click(object sender, RoutedEventArgs e)
    {
        StartDownload_Button.IsEnabled = false;

        await DownloadUtil.ProcessQueue(_videoInfoService.Queue);

        StartDownload_Button.IsEnabled = true;
    }
}
