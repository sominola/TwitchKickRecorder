using System.Text.Json.Serialization;
using FFMpegCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TwitchRecorderApi.Endpoints;
using TwitchRecorderApi.Options;
using TwitchRecorderApi.Services;
using TwitchRecorderApi.Services.Kick;
using TwitchRecorderApi.Services.Kick.Models;
using TwitchRecorderApi.Services.Twitch;
using TwitchRecorderApi.Services.Twitch.Models;

var builder = WebApplication.CreateSlimBuilder(args);
var services = builder.Services;

var section = builder.Configuration.GetRequiredSection(nameof(AppOptions));
builder.WebHost.UseKestrelHttpsConfiguration();
var appOptions = new AppOptions
{
    FmmpegBinaryFolder = section.GetValue<string>(nameof(AppOptions.FmmpegBinaryFolder)) ?? "",
    FmmpegTemporaryFilesFolder = section.GetValue<string>(nameof(AppOptions.FmmpegTemporaryFilesFolder)) ?? "",
    FmmpegOutputFolder = section.GetValue<string>(nameof(AppOptions.FmmpegOutputFolder)) ?? "",
    UserAgent = section.GetValue<string>(nameof(AppOptions.UserAgent)) ?? ""
};

services.AddSingleton(Options.Create(appOptions));

services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
services.AddOpenApi();
services.AddSingleton<TwitchApiService>();
services.AddSingleton<KickApiService>();


services.AddHttpClient(nameof(TwitchApiService),
    client => { client.DefaultRequestHeaders.UserAgent.ParseAdd(appOptions.UserAgent); });
services.AddHttpClient(nameof(KickApiService), client =>
{
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.UserAgent.ParseAdd(appOptions.UserAgent);
});
GlobalFFOptions.Configure(options =>
{
    options.BinaryFolder = appOptions.FmmpegBinaryFolder;
    options.TemporaryFilesFolder = appOptions.FmmpegTemporaryFilesFolder;
});

var app = builder.Build();

var appStoppingCt = app.Lifetime.ApplicationStopping;
var appStoppedCt = app.Lifetime.ApplicationStopped;
using var stopSourceCt = CancellationTokenSource.CreateLinkedTokenSource(appStoppingCt, appStoppedCt);
var stopCt = stopSourceCt.Token;

app.MapOpenApi(pattern: "api/document.json");
app.MapScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "api/document.json";
    options.Title = "Stream record";
    options.Theme = ScalarTheme.Default;
    options.Favicon = "/favicon.svg";
    options.Layout = ScalarLayout.Modern;
    options.DarkMode = true;
});


app.MapTwitchStartRecordEndpoint(appOptions, stopCt);
app.MapKickStartRecordEndpoint(appOptions, stopCt);

app.Run();

[JsonSerializable(typeof(AppOptions))]
[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(ApiErrorResponse))]
[JsonSerializable(typeof(ApiRecordedResponse))]
[JsonSerializable(typeof(GraphQlRequest))]
[JsonSerializable(typeof(GraphQlVariables))]
[JsonSerializable(typeof(PlaybackAccessTokenResponse))]
[JsonSerializable(typeof(StreamData))]
[JsonSerializable(typeof(KickChannelResponse))]
[JsonSerializable(typeof(Livestream))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
