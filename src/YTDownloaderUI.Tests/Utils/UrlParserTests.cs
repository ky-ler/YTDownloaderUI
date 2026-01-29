using YTDownloaderUI.Utils;

namespace YTDownloaderUI.Tests.Utils;

public class IsPlaylistUrlTests
{
    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", true)]
    [InlineData("https://youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", true)]
    [InlineData("http://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", true)]
    [InlineData("https://www.youtube.com/playlist?list=PLxxx&index=5", true)]
    public void IsPlaylistUrl_WithPlaylistUrl_ReturnsTrue(string url, bool expected)
    {
        var result = UrlParser.IsPlaylistUrl(url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtm123")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/abcdef123")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://vimeo.com/123456")]
    public void IsPlaylistUrl_WithNonPlaylistUrl_ReturnsFalse(string url)
    {
        var result = UrlParser.IsPlaylistUrl(url);
        Assert.False(result);
    }

    [Fact]
    public void IsPlaylistUrl_WithWhitespace_StripsAndReturnsTrue()
    {
        const string url = "  https://www.youtube.com/playlist?list=PLxxxxxx  ";
        Assert.True(UrlParser.IsPlaylistUrl(url));
    }
}

public class GetPlaylistIdTests
{
    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", "PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf")]
    [InlineData("https://www.youtube.com/playlist?list=PLrAXtm123&index=5", "PLrAXtm123")]
    [InlineData("https://www.youtube.com/watch?v=abc&list=PLrAXtm123", "PLrAXtm123")]
    [InlineData("https://www.youtube.com/watch?v=abc&list=PLrAXtm123&feature=share", "PLrAXtm123")]
    [InlineData("https://youtu.be/abc?list=PLrAXtm456", "PLrAXtm456")]
    public void GetPlaylistId_WithPlaylist_ReturnsPlaylistId(string url, string expected)
    {
        var result = UrlParser.GetPlaylistId(url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/abcdef")]
    [InlineData("")]
    [InlineData("https://vimeo.com/123456")]
    public void GetPlaylistId_WithoutPlaylist_ReturnsNull(string url)
    {
        var result = UrlParser.GetPlaylistId(url);
        Assert.Null(result);
    }

    [Fact]
    public void GetPlaylistId_WithWhitespace_StripsAndReturnsId()
    {
        const string url = "  https://www.youtube.com/playlist?list=PLtest123  ";
        var result = UrlParser.GetPlaylistId(url);
        Assert.Equal("PLtest123", result);
    }
}

public class GetVideoIdTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=youtu.be", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLxxx", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=abc123&t=10&list=PL1", "abc123")]
    public void GetVideoId_StandardWatchUrl_ReturnsVideoId(string url, string expected)
    {
        var result = UrlParser.GetVideoId(url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/abc123&feature=share", "abc123")]
    public void GetVideoId_ShortUrl_ReturnsVideoId(string url, string expected)
    {
        var result = UrlParser.GetVideoId(url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://www.youtube.com/shorts/abcdef123", "shorts/abcdef123")]
    [InlineData("https://youtube.com/shorts/xyz789", "shorts/xyz789")]
    [InlineData("https://www.youtube.com/shorts/test&feature=share", "shorts/test")]
    public void GetVideoId_ShortsUrl_ReturnsShortsPrefix(string url, string expected)
    {
        var result = UrlParser.GetVideoId(url);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetVideoId_WithWhitespace_StripsAndReturnsId()
    {
        var result = UrlParser.GetVideoId("  https://youtu.be/dQw4w9WgXcQ  ");
        Assert.Equal("dQw4w9WgXcQ", result);
    }
}

public class NormalizeUrlTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=share", "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/watch?v=dQw4w9WgXcQ", "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    public void NormalizeUrl_StandardWatchUrl_ReturnsCanonicalForm(string url, string expected)
    {
        var result = UrlParser.NormalizeUrl(url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/dQw4w9WgXcQ", "https://youtu.be/dQw4w9WgXcQ")]
    public void NormalizeUrl_ShortUrl_PreservesShortFormat(string url, string expected)
    {
        var result = UrlParser.NormalizeUrl(url);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeUrl_ShortsUrl_ReturnsCanonicalShortsFormat()
    {
        var result = UrlParser.NormalizeUrl("https://www.youtube.com/shorts/abc123");
        Assert.Equal("https://www.youtube.com/shorts/abc123", result);
    }

    [Fact]
    public void NormalizeUrl_PlaylistOnlyUrl_ReturnsPlaylistFormat()
    {
        var result = UrlParser.NormalizeUrl("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf");
        Assert.Equal("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf", result);
    }

    [Fact]
    public void NormalizeUrl_VideoWithPlaylist_IncludesPlaylistInOutput()
    {
        var result = UrlParser.NormalizeUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtm123");
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtm123", result);
    }

    [Fact]
    public void NormalizeUrl_WithWhitespace_StripsWhitespace()
    {
        var result = UrlParser.NormalizeUrl("  https://youtu.be/dQw4w9WgXcQ  ");
        Assert.Equal("https://youtu.be/dQw4w9WgXcQ", result);
    }
}

public class ValidateAndNormalizeTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/playlist?list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf")]
    public void ValidateAndNormalize_ValidUrls_ReturnsIsValidTrue(string url)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.NormalizedUrl);
    }

    [Theory]
    [InlineData("", "URL cannot be empty")]
    [InlineData("   ", "URL cannot be empty")]
    public void ValidateAndNormalize_EmptyUrl_ReturnsError(string url, string expectedError)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.False(result.IsValid);
        Assert.Equal(expectedError, result.ErrorMessage);
        Assert.Null(result.NormalizedUrl);
    }

    [Theory]
    [InlineData("https://vimeo.com/123456")]
    [InlineData("https://www.example.com/video")]
    [InlineData("not a url")]
    public void ValidateAndNormalize_NonYouTubeUrl_ReturnsError(string url)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.False(result.IsValid);
        Assert.Equal("Not a valid YouTube URL", result.ErrorMessage);
    }

    [Theory]
    [InlineData("https://www.youtube.com/")]
    [InlineData("https://www.youtube.com/channel/UCxxx")]
    public void ValidateAndNormalize_NoVideoOrPlaylistId_ReturnsError(string url)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.False(result.IsValid);
        Assert.Equal("URL must contain a video or playlist ID", result.ErrorMessage);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("https://youtu.be/abc")]
    [InlineData("https://www.youtube.com/watch?v=toolongvideoidthatisnotvalid")]
    public void ValidateAndNormalize_InvalidVideoIdLength_ReturnsError(string url)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.False(result.IsValid);
        Assert.Equal("Invalid video ID format", result.ErrorMessage);
    }

    [Fact]
    public void ValidateAndNormalize_ValidUrl_ReturnsNormalizedUrl()
    {
        var result = UrlParser.ValidateAndNormalize("  https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=share  ");
        Assert.True(result.IsValid);
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", result.NormalizedUrl);
    }

    [Fact]
    public void ValidateAndNormalize_PlaylistUrl_SkipsVideoIdValidation()
    {
        // Playlist URLs don't validate video ID format
        var result = UrlParser.ValidateAndNormalize("https://www.youtube.com/watch?v=abc&list=PLrAXtm123");
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("youtu.be/dQw4w9WgXcQ")]
    public void ValidateAndNormalize_UrlWithoutProtocol_AddsHttpsAndValidates(string url)
    {
        var result = UrlParser.ValidateAndNormalize(url);
        Assert.True(result.IsValid);
        Assert.NotNull(result.NormalizedUrl);
        Assert.StartsWith("https://", result.NormalizedUrl);
    }

    [Fact]
    public void ValidateAndNormalize_UrlWithoutProtocol_NormalizesCorrectly()
    {
        var result = UrlParser.ValidateAndNormalize("youtube.com/watch?v=dQw4w9WgXcQ&feature=share");
        Assert.True(result.IsValid);
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", result.NormalizedUrl);
    }
}
