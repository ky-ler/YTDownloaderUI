using Moq;
using YTDownloaderUI.Models;
using YTDownloaderUI.Services;

namespace YTDownloaderUI.Tests.Services;

public class VideoInfoServiceTests
{
    private readonly Mock<IYtDlpService> _mockYtDlpService;
    private readonly VideoInfoService _service;

    public VideoInfoServiceTests()
    {
        _mockYtDlpService = new Mock<IYtDlpService>();
        _mockYtDlpService.Setup(x => x.IsYtDlpAvailable).Returns(true);
        _service = new VideoInfoService(_mockYtDlpService.Object);
    }

    [Fact]
    public async Task AddToQueueAsync_AddsItemToQueue()
    {
        // Arrange
        const string url = "https://youtube.com/watch?v=123";
        _mockYtDlpService.Setup(x => x.GetTitleAsync(url))
            .ReturnsAsync("Test Video Title");

        // Act
        await _service.AddToQueueAsync(url, "mp4");

        // Assert
        Assert.Single(_service.Queue);
        var item = _service.Queue.First();
        Assert.Equal(url, item.Url);
        Assert.Equal("Queued", item.Status);
    }

    [Fact]
    public async Task AddToQueueAsync_FetchesAndUpdatesTitle()
    {
        // Arrange
        const string url = "https://youtube.com/watch?v=123";
        const string expectedTitle = "Test Video Title";

        _mockYtDlpService.Setup(x => x.GetTitleAsync(url))
            .ReturnsAsync(expectedTitle);

        // Act
        await _service.AddToQueueAsync(url, "mp4");

        // Wait for title to update (handling fire-and-forget async)
        await WaitForConditionAsync(() => _service.Queue.First().Title == expectedTitle, 2000);

        // Assert
        Assert.Equal(expectedTitle, _service.Queue.First().Title);
    }

    [Fact]
    public void FindDuplicate_ReturnsMatchingItem()
    {
        // Arrange
        const string url = "https://youtube.com/watch?v=123";
        _service.Queue.Add(new VideoInfo(url, "Queued", 0.0, false, false, "mp4"));

        // Act
        var result = _service.FindDuplicate("123", "mp4", false, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(url, result.Url);
    }

    [Fact]
    public void FindDuplicate_ReturnsNull_WhenNoMatch()
    {
        // Arrange
        const string url = "https://youtube.com/watch?v=123";
        _service.Queue.Add(new VideoInfo(url, "Queued", 0.0, false, false, "mp4"));

        // Act
        var result = _service.FindDuplicate("456", "mp4", false, false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReplaceInQueue_ReplacesItemAndFetchesTitle()
    {
        // Arrange
        var oldItem = new VideoInfo("old", "Queued", 0.0, false, false, "mp4");
        _service.Queue.Add(oldItem);

        const string newUrl = "https://youtube.com/new";
        const string newTitle = "New Title";
        _mockYtDlpService.Setup(x => x.GetTitleAsync(newUrl)).ReturnsAsync(newTitle);

        // Act
        _service.ReplaceInQueue(oldItem, newUrl, "mp4", false, false);

        // Assert
        Assert.Single(_service.Queue);
        Assert.NotEqual(oldItem, _service.Queue.First());
        Assert.Equal(newUrl, _service.Queue.First().Url);

        await WaitForConditionAsync(() => _service.Queue.First().Title == newTitle, 2000);
        Assert.Equal(newTitle, _service.Queue.First().Title);
    }

    [Fact]
    public async Task AddToQueueAsync_DoesNotFetchTitle_WhenYtDlpNotAvailable()
    {
        // Arrange
        _mockYtDlpService.Setup(x => x.IsYtDlpAvailable).Returns(false);
        const string url = "https://youtube.com/watch?v=123";

        // Act
        await _service.AddToQueueAsync(url, "mp4");

        // Assert
        // Verify GetTitleAsync was NEVER called
        _mockYtDlpService.Verify(x => x.GetTitleAsync(It.IsAny<string>()), Times.Never);
        Assert.Null(_service.Queue.First().Title);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, int timeoutMs)
    {
        var start = DateTime.Now;
        while (!condition() && (DateTime.Now - start).TotalMilliseconds < timeoutMs)
        {
            await Task.Delay(50);
        }
    }
}
