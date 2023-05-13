// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Wpf.Ui.Appearance;
using YTDownloaderUI.Models;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Utils;

namespace YTDownloaderUI.Views.Pages;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage
{
    public ObservableCollection<VideoInfo> UrlsQueue { get; set; }

    public HomePage()
    {
        if (Settings.Default.Theme == "Light")
            Theme.Apply(ThemeType.Light);
        else
            Theme.Apply(ThemeType.Dark);

        InitializeComponent();

        UrlsQueue = new ObservableCollection<VideoInfo>();

        DataContext = UrlsQueue;

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

    private void AddToQueue_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var urls = UrlList.Text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(x => x.Contains("youtu.be/") || x.Contains("youtube.com/watch?v="))
                                .Select(x => x.Trim())
                                .ToList();

        foreach (var url in urls)
        {
            var videoId = UrlParser.GetVideoId(url);

            if (!UrlsQueue.Any(x => x.Url.Contains(videoId)))
            {
                UrlsQueue.Add(new VideoInfo(
                    url: $"https://youtu.be/{videoId}",
                    status: "Queued",
                    downloadProgress: 0.0,
                    audioOnly: false,
                    getPlaylist: false,
                    getSubtitles: false));
            }
        }

        DataContext = this;
        UrlList.Text = "";
    }

    private async void StartDownload_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        StartDownload_Button.IsEnabled = false;

        await DownloadUtil.ProcessQueue(UrlsQueue);

        StartDownload_Button.IsEnabled = true;
    }
}
