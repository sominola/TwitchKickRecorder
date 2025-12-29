using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using TwitchRecorderApi.Services.Twitch.Models;

namespace TwitchRecorderApi.Services.Twitch;

public partial class TwitchApiService(IHttpClientFactory clientFactory)
{
    private HttpClient GetClient()
    {
        return clientFactory.CreateClient(nameof(TwitchApiService));
    }

    public async Task<(string sig, string value)> GetSigAndToken(string channel, CancellationToken ct = default)
    {
        const string clientId = "kimne78kx3ncx6brgo4mv6wki5h1ko";
        const string query =
            "query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!," +
            " $vodID: ID!, $isVod: Boolean!, $playerType: String!, $platform: String!)" +
            " {  streamPlaybackAccessToken(channelName: $login, params: {platform: $platform, " +
            "playerBackend: \"mediaplayer\", playerType: $playerType}) @include(if: $isLive) " +
            "{    value    signature   authorization " +
            "{ isForbidden forbiddenReasonCode }   __typename  }  " +
            "videoPlaybackAccessToken(id: $vodID, params: {platform: " +
            "$platform, playerBackend: \"mediaplayer\", playerType: $playerType}) @include(if: $isVod)" +
            " {    value    signature   __typename  }}";

        var payload = new GraphQlRequest
        {
            OperationName = "PlaybackAccessToken_Template",
            Query = query,
            Variables = new GraphQlVariables
            {
                IsLive = true,
                Login = channel,
                IsVod = false,
                VodID = "",
                Platform = "web",
                PlayerBackend = "mediaplayer",
                PlayerType = "site"
            }
        };

        var jsonStr = JsonSerializer.Serialize(payload, AppJsonSerializerContext.Default.GraphQlRequest);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://gql.twitch.tv/gql");
        httpRequest.Content = new StringContent(jsonStr);
        httpRequest.Headers.TryAddWithoutValidation("accept", "*/*");
        httpRequest.Headers.TryAddWithoutValidation("accept-language", "en-US");
        httpRequest.Headers.TryAddWithoutValidation("authorization", "undefined");
        httpRequest.Headers.TryAddWithoutValidation("client-id", clientId);
        httpRequest.Headers.TryAddWithoutValidation("content-type", "text/plain; charset=UTF-8");
        httpRequest.Headers.TryAddWithoutValidation("device-id", "eUE65ObRg7QdvBI4ILdecrRK73wgVJhR");
        httpRequest.Headers.TryAddWithoutValidation("origin", "https://www.twitch.tv");
        httpRequest.Headers.TryAddWithoutValidation("priority", "u=1, i");
        httpRequest.Headers.TryAddWithoutValidation("referer", "https://www.twitch.tv/");
        httpRequest.Headers.TryAddWithoutValidation("sec-ch-ua",
            "\"Google Chrome\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
        httpRequest.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
        httpRequest.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
        httpRequest.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
        httpRequest.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
        httpRequest.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
        httpRequest.Headers.TryAddWithoutValidation("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36");


        using var response = await GetClient().SendAsync(httpRequest, ct);
        var accessTokenResponse = await response.Content.ReadFromJsonAsync<PlaybackAccessTokenResponse>(
            AppJsonSerializerContext.Default.PlaybackAccessTokenResponse,
            ct
        );

        return (
            accessTokenResponse!.Data.StreamPlaybackAccessToken.Signature,
            accessTokenResponse.Data.StreamPlaybackAccessToken.Value
        );
    }


    public async Task<(bool isStreaming, string? m3u8, string? broadCastId)> GetTwitchQualitiesM3U8(
        string channel,
        string signature,
        string token,
        CancellationToken ct = default
    )
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var rnd = Random.Shared.Next(1_000_000, 10_000_000);

        var url =
            $"https://usher.ttvnw.net/api/channel/hls/{channel}.m3u8?" +
            "acmb=eyJBcHBWZXJzaW9uIjoiYWVmYzA4YjAtMTYxMy00NjEzLWJhOTctYmFjYjY0ZDlkMTA2In0%3D" +
            "&allow_source=true" +
            "&browser_family=chrome" +
            "&browser_version=141.0" +
            "&cdm=wv" +
            "&enable_score=true" +
            "&fast_bread=true" +
            "&include_unavailable=true" +
            "&multigroup_video=false" +
            "&os_name=Windows" +
            "&os_version=NT%2010.0" +
            $"&p={rnd}" +
            "&platform=web" +
            "&play_session_id=1433054fbefec7436e22667f59e11267" +
            "&player_backend=mediaplayer" +
            "&player_version=1.47.0-rc.1" +
            "&playlist_include_framerate=true" +
            "&reassignments_supported=true" +
            $"&sig={signature}" +
            "&supported_codecs=av1,h265,h264" +
            $"&token={encodedToken}" +
            "&transcode_mode=cbr_v1";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);

        req.Headers.TryAddWithoutValidation("Accept",
            "application/x-mpegURL, application/vnd.apple.mpegurl, application/json, text/plain");
        req.Headers.TryAddWithoutValidation("Referer", "");


        using var response = await GetClient().SendAsync(req, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (false, null, null);
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        var match = BroadCastIdRegex().Match(content);
        if (match.Success)
        {
            var broadcastId = match.Groups[1].Value;
            return (true, content, broadcastId);
        }

        throw new Exception("Broadcast ID is not found");
    }

    public string GetBestQualityStreamUrl(string m3U8Content)
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

    [GeneratedRegex("""
                    BROADCAST-ID="(\d+)"
                    """)]
    private static partial Regex BroadCastIdRegex();
}
