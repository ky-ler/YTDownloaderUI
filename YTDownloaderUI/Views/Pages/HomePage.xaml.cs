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
            DownloadBtnStatus.Text = "Set your yt-dlp.exe location in settings!";
        }
        else
        {
            StartDownload_Button.IsEnabled = true;
            DownloadBtnStatus.Text = "";
        }
    }

    private void AddToQueue_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var urls = UrlList.Text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .ToList();

        foreach (var url in urls)
        {
            if (!UrlsQueue.Any(x => x.Url == url))
                UrlsQueue.Add(new VideoInfo { Url = url, Status = "Queued", DownloadProgress = 0.0f });
        }

        DataContext = this;
        UrlList.Text = "";
    }

    private async void StartDownload_Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DownloadBtnStatus.Text = "Downloading...";
        StartDownload_Button.IsEnabled = false;
        await DownloadUtil.ProcessQueue(UrlsQueue);
        DownloadBtnStatus.Text = "Finished Downloading!";
        StartDownload_Button.IsEnabled = true;
    }
}
