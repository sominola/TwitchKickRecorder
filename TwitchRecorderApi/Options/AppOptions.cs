namespace TwitchRecorderApi.Options;

public sealed class AppOptions
{
    [ConfigurationKeyName("FmmpegBinaryFolder")]
    public string FmmpegBinaryFolder { get; init; } = string.Empty;

    [ConfigurationKeyName("FmmpegTemporaryFilesFolder")]
    public string FmmpegTemporaryFilesFolder { get; init; } = string.Empty;

    [ConfigurationKeyName("FmmpegOutputFolder")]
    public string FmmpegOutputFolder { get; init; } = string.Empty;

    [ConfigurationKeyName("UserAgent")]
    public string UserAgent { get; init; } = string.Empty;
}
