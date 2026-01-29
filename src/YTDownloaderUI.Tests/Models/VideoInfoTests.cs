using YTDownloaderUI.Models;

namespace YTDownloaderUI.Tests.Models;

public class VideoInfoTests
{
    [Fact]
    public void DisplayName_ReturnsTitle_WhenTitleIsSet()
    {
        var video = new VideoInfo("https://youtube.com/watch?v=123", "Queued", 0.0, false, false)
        {
            Title = "My Video"
        };
        Assert.Equal("My Video", video.DisplayName);
    }

    [Fact]
    public void DisplayName_ReturnsUrl_WhenTitleIsNull()
    {
        const string url = "https://youtube.com/watch?v=123";
        var video = new VideoInfo(url, "Queued", 0.0, false, false);
        Assert.Equal(url, video.DisplayName);
    }

    [Fact]
    public void PresetDisplay_ReturnsDefault_WhenPresetIsEmpty()
    {
        var video = new VideoInfo("url", "Queued", 0.0, false, false, "");
        Assert.Equal("Default", video.PresetDisplay);
    }

    [Fact]
    public void PresetDisplay_ReturnsUppercasePreset_WhenPresetIsSet()
    {
        var video = new VideoInfo("url", "Queued", 0.0, false, false, "mp4");
        Assert.Equal("MP4", video.PresetDisplay);
    }

    [Fact]
    public void OptionsTooltip_ReturnsNoExtraOptions_WhenNoneSelected()
    {
        var video = new VideoInfo("url", "Queued", 0.0, false, false);
        Assert.Equal("No extra options", video.OptionsTooltip);
    }

    [Fact]
    public void OptionsTooltip_ReturnsOptions_WhenSelected()
    {
        var video = new VideoInfo("url", "Queued", 0.0, true, true);
        Assert.Contains("Playlist", video.OptionsTooltip);
        Assert.Contains("Subtitles", video.OptionsTooltip);
    }

    [Fact]
    public void CanRetry_True_WhenStatusIsErrorAndRetriesAvailable()
    {
        var video = new VideoInfo("url", "Error", 0.0, false, false)
        {
            RetryCount = 0
        };
        Assert.True(video.CanRetry);
    }

    [Fact]
    public void CanRetry_False_WhenStatusIsNotError()
    {
        var video = new VideoInfo("url", "Queued", 0.0, false, false);
        Assert.False(video.CanRetry);
    }

    [Fact]
    public void CanRetry_False_WhenMaxRetriesExceeded()
    {
        var video = new VideoInfo("url", "Error", 0.0, false, false)
        {
            RetryCount = VideoInfo.MaxRetries
        };
        Assert.False(video.CanRetry);
    }

    [Fact]
    public void PropertyChanged_Fires_WhenPropertiesChange()
    {
        var video = new VideoInfo("url", "Queued", 0.0, false, false);
        Assert.PropertyChanged(video, nameof(video.Status), () => video.Status = "Downloading");
        Assert.PropertyChanged(video, nameof(video.DownloadProgress), () => video.DownloadProgress = 0.5);
        Assert.PropertyChanged(video, nameof(video.ErrorMessage), () => video.ErrorMessage = "Failed");
        Assert.PropertyChanged(video, nameof(video.RetryCount), () => video.RetryCount++);
    }
}
