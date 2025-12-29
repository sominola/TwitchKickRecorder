using System.Text.Json.Serialization;

namespace TwitchRecorderApi.Services.Kick.Models;

public class KickChannelResponse
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("user_id")]
    public long UserId { get; init; }

    [JsonPropertyName("slug")]
    public string Slug { get; init; } = null!;

    [JsonPropertyName("is_banned")]
    public bool IsBanned { get; init; }

    [JsonPropertyName("playback_url")]
    public string PlayBackUrl { get; init; } = null!;

    [JsonPropertyName("livestream")]
    public Livestream Livestream { get; init; } = null!;
}

public class Livestream
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("slug")]
    public string Slug { get; init; } = null!;

    [JsonPropertyName("channel_id")]
    public long ChannelId { get; init; }

    // [JsonPropertyName("created_at")]
    // [JsonConverter(typeof(NullableDateTimeOffsetConverter))]
    // public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("session_title")]
    public string SessionTitle { get; init; } = null!;

    [JsonPropertyName("is_live")]
    public bool IsLive { get; init; }

    [JsonPropertyName("risk_level_id")]
    public int? RiskLevelId { get; init; }

    // [JsonPropertyName("start_time")]
    // [JsonConverter(typeof(NullableDateTimeOffsetConverter))]
    // public DateTimeOffset? StartTime { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = null!;

    [JsonPropertyName("twitch_channel")]
    public string TwitchChannel { get; init; } = null!;

    [JsonPropertyName("duration")]
    public int Duration { get; init; }

    [JsonPropertyName("language")]
    public string Language { get; init; } = null!;

    [JsonPropertyName("is_mature")]
    public bool IsMature { get; init; }

    [JsonPropertyName("viewer_count")]
    public int ViewerCount { get; init; }

    [JsonPropertyName("viewers")]
    public int Viewers { get; init; }

    [JsonPropertyName("lang_iso")]
    public string LangIso { get; init; } = null!;
}
