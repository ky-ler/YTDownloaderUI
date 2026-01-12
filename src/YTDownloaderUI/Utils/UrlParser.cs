using System;
using System.Linq;

namespace YTDownloaderUI.Utils;

public static class UrlParser
{
    /// <summary>
    /// Checks if the URL is a YouTube playlist URL
    /// </summary>
    public static bool IsPlaylistUrl(string url)
    {
        var cleanUrl = StripSpaces(url);
        return cleanUrl.Contains("youtube.com/playlist?list=");
    }

    /// <summary>
    /// Gets the playlist ID from a playlist URL
    /// </summary>
    public static string? GetPlaylistId(string url)
    {
        var cleanUrl = StripSpaces(url);

        const string playlistParam = "list=";
        var listIndex = cleanUrl.IndexOf(playlistParam);

        if (listIndex < 0)
            return null;

        var playlistId = cleanUrl[(listIndex + playlistParam.Length)..];

        // Strip any trailing parameters
        var endIndex = playlistId.IndexOf('&');
        if (endIndex >= 0)
            playlistId = playlistId[..endIndex];

        return playlistId;
    }

    /// <summary>
    /// Gets the video ID from a video URL (not for playlist-only URLs)
    /// </summary>
    public static string GetVideoId(string url)
    {
        var cleanUrl = StripSpaces(url);

        var findIndex = 0;

        const string regularUrl = "youtube.com/watch?v=";
        const string miniLink = "youtu.be/";
        const string shorts = "shorts/";

        if (cleanUrl.Contains(shorts))
            findIndex = cleanUrl.IndexOf(shorts, StringComparison.Ordinal) + shorts.Length;

        else if (cleanUrl.Contains(regularUrl))
            findIndex = cleanUrl.IndexOf(regularUrl, StringComparison.Ordinal) + regularUrl.Length;

        else if (cleanUrl.Contains(miniLink))
            findIndex = cleanUrl.IndexOf(miniLink, StringComparison.Ordinal) + miniLink.Length;

        var videoId = cleanUrl[findIndex..];

        // Split url at &, example: https://www.youtube.com/watch?v=VIDEO_ID&feature=youtu.be
        var endIndex = videoId.IndexOf('&');

        if (endIndex >= 0)
            videoId = videoId[..endIndex];

        if (cleanUrl.Contains(shorts))
            return shorts + videoId;

        return videoId;
    }

    /// <summary>
    /// Normalizes a YouTube URL to a canonical form
    /// </summary>
    public static string NormalizeUrl(string url)
    {
        var cleanUrl = StripSpaces(url);
        var playlistId = GetPlaylistId(cleanUrl);

        // For playlist-only URLs, return the playlist URL format
        if (IsPlaylistUrl(cleanUrl) && !cleanUrl.Contains("watch?v="))
        {
            return $"https://www.youtube.com/playlist?list={playlistId}";
        }

        var videoId = GetVideoId(cleanUrl);

        // only return youtu.be/id if it was used in the original URL
        if (cleanUrl.Contains("youtu.be/") && string.IsNullOrEmpty(playlistId))
            return $"https://youtu.be/{videoId}";

        if (!string.IsNullOrEmpty(playlistId))
        {
            return $"https://www.youtube.com/watch?v={videoId}&list={playlistId}";
        }

        return videoId.StartsWith("shorts/") ? $"https://www.youtube.com/{videoId}" : $"https://www.youtube.com/watch?v={videoId}";
    }

    private static string StripSpaces(string url)
    {
        return new string(url.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
