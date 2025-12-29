FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG RID=linux-musl-x64

RUN apk add --no-cache clang build-base zlib-dev

WORKDIR /src

COPY *.csproj .
RUN dotnet restore -r $RID

COPY . .

RUN dotnet publish -c Release -r $RID -o /out \
    -p:PublishAot=true \
    -p:SelfContained=true \
    -p:InvariantGlobalization=true \
    -p:StripSymbols=true \
    -p:DebugType=None \
    -p:AssemblyName=Application






FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine
WORKDIR /app

RUN apk add --no-cache ffmpeg

COPY --from=build /out ./

EXPOSE 8080
ENV DOTNET_URLS=http://+:8080

USER root

ENTRYPOINT ["./Application"]
