// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wpf.Ui.Appearance;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Services;

namespace YTDownloaderUI.Views.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage
{
    public SettingsPage()
    {
        DataContext = this;

        InitializeComponent();

        if (Theme.GetAppTheme() == ThemeType.Dark)
            DarkThemeRadioButton.IsChecked = true;
        else
            LightThemeRadioButton.IsChecked = true;

        VersionInfo.Text = $"Version {GetAssemblyVersion()}";
    }

    private string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
    }

    private void OnLightThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        Theme.Apply(ThemeType.Light);
        Settings.Default.Theme = "Light";
        Settings.Default.Save();
    }

    private void OnDarkThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        Theme.Apply(ThemeType.Dark);
        Settings.Default.Theme = "Dark";
        Settings.Default.Save();
    }

    private void YtDlpLocation_Button_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "yt-dlp",
            DefaultExt = ".exe",
            Filter = "Executables (.exe)|*.exe",
            InitialDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
        };

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            YtDlpLocation_TextBox.Text = dialog.FileName;
        }
    }

    private void SaveSettings_Button_Click(object sender, RoutedEventArgs e)
    {
        // Save yt-dlp path
        Settings.Default.YtDlpLocation = YtDlpLocation_TextBox.Text;
        Settings.Default.Save();

        // Re-validate FFmpeg availability (auto-detected based on yt-dlp location)
        FFmpegService.Instance.ValidateFFmpeg();
    }

    private void Github_Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/ky-ler/YTDownloaderUI") { UseShellExecute = true });
    }
}
