using FFMpegCore;
using Microsoft.Extensions.Options;
using TwitchRecorderApi.Options;

namespace TwitchRecorderApi.Services;

public class RecorderService(IOptions<AppOptions> appOptions, ILogger<RecorderService> logger)
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public async Task StartRecord(
        string bestQualityM3U8,
        string channelName,
        string broadCastId,
        string providerName,
        CancellationToken ct = default
    )
    {
        var uri = new Uri(bestQualityM3U8);
        var streamName = channelName + "_" + broadCastId;
        var outputFolder = Path.Combine(_appOptions.FmmpegOutputFolder, providerName, streamName);
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
                    .WithCustomArgument($"-headers \"User-Agent: {_appOptions.UserAgent}\""))
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
            logger.LogError(e, "An error occured during processing of stream");
        }
    }
}
