using System;

namespace YTDownloaderUI.Utils;

public class DownloadException : Exception
{
    public DownloadException(string message) : base(message) { }
    public DownloadException(string message, Exception inner) : base(message, inner) { }
}
