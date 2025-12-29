using System.Net;
using System.Text.RegularExpressions;
using TwitchRecorderApi.Services.Kick.Models;

namespace TwitchRecorderApi.Services.Kick;

public partial class KickApiService(IHttpClientFactory clientFactory)
{
    private HttpClient GetClient()
    {
        return clientFactory.CreateClient(nameof(KickApiService));
    }

    public async Task<KickChannelResponse?> GetChannelInfo(string channelName, CancellationToken ct = default)
    {
        const string url = "https://kick.com/api/v1/channels/{0}";
        var combinedUrl = string.Format(url, channelName);

        using var request = new HttpRequestMessage(HttpMethod.Get, combinedUrl);
        request.Version = HttpVersion.Version30;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

        using var response = await GetClient().SendAsync(request, ct);
        var channelResponse = await response.Content.ReadFromJsonAsync<KickChannelResponse>(
            AppJsonSerializerContext.Default.KickChannelResponse,
            ct
        );

        return channelResponse;
    }

    public async Task<string> GetM3U8Qualities(string playbackUrl, CancellationToken ct = default)
    {
        var response = await GetClient().GetStringAsync(playbackUrl, ct);
        return response;
    }

    public static string GetBestQualityStreamUrl(string m3U8Content)
    {
        if (string.IsNullOrWhiteSpace(m3U8Content))
            throw new ArgumentException("M3U8 content is empty", nameof(m3U8Content));

        string? bestUrl = null;
        var maxBandwidth = 0;

        var regex = MyRegex();

        foreach (Match match in regex.Matches(m3U8Content))
        {
            if (int.TryParse(match.Groups[1].Value, out int bandwidth))
            {
                var url = match.Groups[2].Value.Trim();
                if (bandwidth > maxBandwidth)
                {
                    maxBandwidth = bandwidth;
                    bestUrl = url;
                }
            }
        }

        if (bestUrl == null)
            throw new InvalidOperationException("No stream URL found in M3U8 content.");

        return bestUrl;
    }

    [GeneratedRegex(@"#EXT-X-STREAM-INF:.*?BANDWIDTH=(\d+).*?\n(https?:\/\/[^\s]+)", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
