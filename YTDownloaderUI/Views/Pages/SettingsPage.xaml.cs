// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using YTDownloaderUI.Properties;
using YTDownloaderUI.Utils;

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

        // Load download directory setting
        DownloadDirectoryTextBox.Text = Settings.Default.DownloadDirectory;
    }

    private static string GetAssemblyVersion()
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

    private void Github_Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        BrowserUtil.OpenUrl("https://github.com/ky-ler/YTDownloaderUI");
    }

    private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Download Directory",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            DownloadDirectoryTextBox.Text = dialog.FolderName;
            Settings.Default.DownloadDirectory = dialog.FolderName;
            Settings.Default.Save();
        }
    }

    private void ResetDirectory_Click(object sender, RoutedEventArgs e)
    {
        DownloadDirectoryTextBox.Text = "";
        Settings.Default.DownloadDirectory = "";
        Settings.Default.Save();
    }
}
