# YTDownloaderUI

Lightweight Windows application for downloading YouTube videos.

Paste one or more links, add them to a queue, choose a preset, and download to a local folder.

## Features

- Queue-based batch downloads (paste many links at once)
- Built-in `yt-dlp` (no separate install/config required)
- Format/audio presets (FFmpeg optional for mp3/aac extraction)
- Configurable download directory (defaults to `downloads` folder)
- Cancel downloads in progress
- Light/Dark theme

## Instructions

### Download

Choose one of the following versions from the [latest release](https://github.com/ky-ler/YTDownloaderUI/releases/latest):

- **YTDownloaderUI-x.x.x-self-contained.zip** — Recommended for most users. Larger download but includes everything needed to run.
- **YTDownloaderUI-x.x.x-framework-dependent.zip** — Smaller download. Requires [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) to be installed separately.

### Setup

1. Extract the downloaded zip file
2. Run YTDownloaderUI.exe
3. Paste YouTube links in the Video Links text box
4. (Optional) Select a format preset from the dropdown (mp4, mkv, mp3, aac) — see [FFmpeg setup](#optional-ffmpeg)
5. Click "Add to Queue"
6. Click "Download" to start downloading
7. Videos are saved to the configured download directory (Settings > Download Directory, defaults to `downloads` folder)

### Optional: FFmpeg

For presets (mp3, aac, mp4, mkv), you need FFmpeg:

1. Download [ffmpeg](https://www.ffmpeg.org/download.html) for Windows (Builds are available from [gyan.dev](https://www.gyan.dev/ffmpeg/builds/) and [BtbN](https://github.com/BtbN/FFmpeg-Builds/releases))
2. Extract `ffmpeg.exe` and `ffprobe.exe`
3. Place them in the `tools` folder (where `yt-dlp.exe` is located)

## Images

![Main Page](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/home.png)

![Settings Page (1/2)](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/settings_1.png)

![Settings Page (2/2)](https://github.com/ky-ler/YTDownloaderUI/raw/main/media/settings_2.png)

## Building from Source

```bash
git clone https://github.com/ky-ler/YTDownloaderUI.git
cd YTDownloaderUI
```

### Required: yt-dlp

Run the download script to fetch `yt-dlp.exe`:

```powershell
.\download-tools.ps1
```

This downloads `yt-dlp.exe` to `src/YTDownloaderUI/tools/`.

Alternatively, manually download [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp/releases/latest) and place it in `src/YTDownloaderUI/tools/`.


### Optional: FFmpeg

For presets (mp3, aac, mp4, mkv), download [FFmpeg](https://www.ffmpeg.org/download.html) and place `ffmpeg.exe` and `ffprobe.exe` in `src/YTDownloaderUI/tools/`.

### Build

```bash
dotnet restore
dotnet build -c Release
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Legal Disclaimer

### Usage Warning

This tool is provided for downloading videos from platforms like YouTube. **Users must ensure their use complies with applicable laws and the platform's Terms of Service.** Downloading content without proper authorization may violate copyright laws and platform usage policies.

This tool is intended for:

- Downloading content you own or have permission to download
- Personal, non-commercial use
- Content that is not protected by copyright or is used under applicable licenses

**Users are solely responsible for ensuring their use of this tool is legal and complies with all applicable laws and terms of service.**

### Copyright Respect

Please respect the copyright and intellectual property rights of content creators. The Digital Millennium Copyright Act (DMCA) and similar laws in other countries protect copyrighted works. Only download content that:

- You have created
- You have explicit permission from the copyright holder to download
- Is in the public domain
- Is licensed under a Creative Commons or similar permissive license

## Acknowledgments

This application uses the following open source projects:

### yt-dlp

yt-dlp is bundled with this application under the [Unlicense](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE).

- Project: [github.com/yt-dlp/yt-dlp](https://github.com/yt-dlp/yt-dlp)
- License: [Unlicense](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE)

### WPF UI

- Project: [github.com/lepoco/wpfui](https://github.com/lepoco/wpfui)
- License: [MIT License](https://github.com/lepoco/wpfui/blob/main/LICENSE)

### FFmpeg (Optional)

FFmpeg is not bundled but may be used for audio extraction features.

- Project: [ffmpeg.org](https://www.ffmpeg.org/)
- License: [LGPL/GPL](https://www.ffmpeg.org/legal.html)
