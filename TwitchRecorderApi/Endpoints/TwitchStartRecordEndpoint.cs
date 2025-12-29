using FFMpegCore;
using Microsoft.AspNetCore.Mvc;
using TwitchRecorderApi.Options;
using TwitchRecorderApi.Services;
using TwitchRecorderApi.Services.Twitch;

namespace TwitchRecorderApi.Endpoints;

public static class TwitchStartRecordEndpoint
{
    public static RouteHandlerBuilder MapTwitchStartRecordEndpoint(
        this IEndpointRouteBuilder endpoints,
        AppOptions appOptions,
        CancellationToken stopCt
    )
    {
        return endpoints.MapPost("/twitch/{channelName}/start_record", async (
                [FromRoute] string channelName,
                [FromServices] TwitchApiService twitchApi,
                CancellationToken cancellationToken
            ) =>
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, stopCt);
                var ct = linkedCts.Token;

                var (sig, value) = await twitchApi.GetSigAndToken(channelName, ct);
                var (isStreaming, channelQualities, broadCastId) =
                    await twitchApi.GetTwitchQualitiesM3U8(channelName, sig, value, ct);
                if (string.IsNullOrEmpty(channelQualities) || !isStreaming)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        IsStreaming = isStreaming,
                        ChannelName = channelName,
                    });
                }

                var bestQualityM3U8 = twitchApi.GetBestQualityStreamUrl(channelQualities);

                var uri = new Uri(bestQualityM3U8);
                var streamName = channelName + "_" + broadCastId;
                var outputFolder = Path.Combine(appOptions.FmmpegOutputFolder, "twitch", streamName);
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }


                var playlistFile = Path.Combine(outputFolder, "stream.m3u8");
                var segmentTemplate = Path.Combine(outputFolder, "seg_%07d.ts");

                int startNumber = 0;
                if (File.Exists(playlistFile))
                {
                    var lines = await File.ReadAllLinesAsync(playlistFile, ct);
                    var lastSegment = lines
                        .LastOrDefault(l => l.EndsWith(".ts"));
                    if (lastSegment != null)
                    {
                        var numberPart =
                            new string(lastSegment.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
                        if (int.TryParse(numberPart, out var lastNum))
                            startNumber = lastNum + 1;
                    }
                }

                try
                {
                    var args = FFMpegArguments
                        .FromUrlInput(uri, options => options
                            .WithCustomArgument($"-headers \"User-Agent: {appOptions.UserAgent}\""))
                        .OutputToFile(playlistFile, true, options => options
                            .WithCustomArgument("-c:v copy")
                            .WithCustomArgument("-c:a copy")
                            .WithCustomArgument("-f hls")
                            .WithCustomArgument("-hls_time 5")
                            .WithCustomArgument("-hls_list_size 0")
                            .WithCustomArgument("-hls_flags append_list")
                            .WithCustomArgument($"-hls_segment_filename \"{segmentTemplate}\"")
                            .WithCustomArgument($"-start_number {startNumber}")
                        );
                    await args
                        .CancellableThrough(ct)
                        .ProcessAsynchronously();
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new ApiErrorResponse
                    {
                        ErrorMessage = e.Message,
                    });
                }

                return Results.Ok(new ApiRecordedResponse
                {
                    Recorded = true,
                    ChannelName = channelName,
                });
            })
            .WithName("Twitch record stream");
    }
}
