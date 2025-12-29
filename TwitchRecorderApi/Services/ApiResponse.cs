namespace TwitchRecorderApi.Services;

public class ApiResponse
{
    public bool IsStreaming { get; init; }
    public string ChannelName { get; init; } = null!;
}

public class ApiErrorResponse
{
    public string ErrorMessage { get; init; } = null!;
}

public class ApiRecordedResponse
{
    public bool Recorded { get; init; }
    public string ChannelName { get; init; } = null!;
}
