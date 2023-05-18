using System.Linq;

namespace YTDownloaderUI.Utils;

public static class UrlParser
{
    public static string GetVideoId(string url)
    {
        var cleanUrl = StripSpaces(url);

        string videoId;
        int findIndex = 0;

        var regularUrl = "youtube.com/watch?v=";
        var miniLink = "youtu.be/";
        var shorts = "shorts/";

        if (cleanUrl.Contains(shorts))
            findIndex = cleanUrl.IndexOf(shorts) + shorts.Length;

        else if (cleanUrl.Contains(regularUrl))
            findIndex = cleanUrl.IndexOf(regularUrl) + regularUrl.Length;

        else if (cleanUrl.Contains(miniLink))
            findIndex = cleanUrl.IndexOf(miniLink) + miniLink.Length;



        videoId = cleanUrl[findIndex..];

        // Split url at &, example: https://www.youtube.com/watch?v=VIDEO_ID&feature=youtu.be
        var endIndex = videoId.IndexOf("&");

        if (endIndex >= 0)
            videoId = videoId[..endIndex];

        if (cleanUrl.Contains(shorts))
            return shorts + videoId;

        return videoId;
    }

    private static string StripSpaces(string url)
    {
        return new string(url.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
