# YTDownloaderUI

GUI wrapper for [yt-dlp](https://github.com/yt-dlp/yt-dlp) that allows you to download YouTube videos.

## Features

- Batch download YouTube videos
- Light/Dark Theme

## Dependencies

- [.NET 7.0](https://dotnet.microsoft.com/en-us/download)

- [yt-dlp](https://github.com/yt-dlp/yt-dlp/releases/latest) (yt-dlp.exe for Windows)

Optional - Needed for post-processing options

- [FFmpeg and FFprobe executables](https://www.ffmpeg.org/download.html) (Get packages & executable files section)

## Instructions

1. Download and extract [YTDownloaderUI.zip](https://github.com/ky-ler/YTDownloaderUI/releases/latest/download/YTDownloaderUI.zip) and run YTDownloaderUI.exe
2. Go to the Settings tab, and scroll down to "Dependencies" and set your yt-dlp.exe location (likely in your Downloads folder)
   - Optional: If using post-processing options, also set your FFmpeg and FFprobe paths
3. Go back to the Home tab, and start pasting YouTube links in the Video Links text box
4. Click the "Add to Queue" button
5. Click the "Download" button to start downloading the videos in your queue
6. Wait for downloads to complete.
   - Videos can be found in a folder called "downloads" in the same directory as YTDownloaderUI.exe

## Images

### Dark Theme

![Main page - dark theme](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/home_dark.png)

![Settings page - dark theme](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/settings_dark.png)

### Light Theme

![Main page - light theme](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/home_light.png)

![Settings page - light theme](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/settings_light.png)

## Special Thanks

Thank you to the creators of these projects:

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) ([LICENSE](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE))

- [WPF UI](https://github.com/lepoco/wpfui) ([LICENSE](https://github.com/lepoco/wpfui/blob/main/LICENSE))

- [FFmpeg](https://www.ffmpeg.org/) ([LICENSE](https://www.ffmpeg.org/legal.html))
