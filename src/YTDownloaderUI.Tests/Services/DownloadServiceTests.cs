using Xunit;
using YTDownloaderUI.Services;

namespace YTDownloaderUI.Tests.Services;

public class DownloadServiceTests
{
    [Theory]
    [InlineData("ERROR: Video unavailable", "Video is unavailable (private, deleted, or region-locked)")]
    [InlineData("Sign in to confirm your age", "Video requires age verification")]
    [InlineData("Private video", "This video is private")]
    [InlineData("ERROR: Unable to download webpage: <urlopen error [Errno 11001] getaddrinfo failed>", "Network error - check your internet connection")]
    [InlineData("ERROR: Incomplete YouTube ID", "Invalid YouTube video ID")]
    [InlineData("HTTP Error 404: Not Found", "Video not found (404)")]
    [InlineData("HTTP Error 403: Forbidden", "Access denied (403)")]
    [InlineData("ERROR: ffmpeg not found. Please install or provide the path using --ffmpeg-location", "FFmpeg required but not found")]
    [InlineData("ERROR: Custom error message", "Custom error message")]
    [InlineData("Random error output", "Download failed (exit code 1)")]
    public void ParseYtDlpError_ReturnsCorrectMessage(string stderr, string expected)
    {
        var result = DownloadService.ParseYtDlpError(stderr, 1);
        Assert.Equal(expected, result);
    }
}