using System.Text.Json.Serialization;

namespace TwitchRecorderApi.Services.Twitch.Models;

public class GraphQlRequest
{
    [JsonPropertyName("operationName")]
    public string OperationName { get; init; } = null!;

    [JsonPropertyName("query")]
    public string Query { get; init; } = null!;

    [JsonPropertyName("variables")]
    public GraphQlVariables Variables { get; init; } = null!;
}

public class GraphQlVariables
{
    [JsonPropertyName("isLive")]
    public bool IsLive { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("isVod")]
    public bool IsVod { get; set; }

    [JsonPropertyName("vodID")]
    public string VodID { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("playerBackend")]
    public string PlayerBackend { get; set; } = string.Empty;

    [JsonPropertyName("playerType")]
    public string PlayerType { get; set; } = string.Empty;
}

public class PlaybackAccessTokenResponse
{
    [JsonPropertyName("data")]
    public StreamData Data { get; set; } = null!;
}

public class StreamData
{
    [JsonPropertyName("streamPlaybackAccessToken")]
    public StreamPlaybackAccessToken StreamPlaybackAccessToken { get; set; } = null!;
}

public class StreamPlaybackAccessToken
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = null!;
}
