FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

ARG RID=linux-musl-x64

RUN apk add --no-cache clang build-base icu-libs zlib-dev

COPY *.csproj ./
RUN dotnet restore -r $RID

COPY . ./

RUN dotnet publish -c Release -r $RID -o /app/publish \
    -p:PublishAot=true \
    -p:SelfContained=true \
    -p:StripSymbols=true \
    -p:DebugType=None

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache ffmpeg libmsquic icu-libs

COPY --from=build /app/publish ./
RUN chmod +x TwitchRecorderApi

EXPOSE 8080
ENV DOTNET_URLS=http://+:8080

ENTRYPOINT ["./TwitchRecorderApi"]
