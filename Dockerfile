FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoSyncData ./AutoSyncData
COPY ./Sources/Fanex.AutoSyncDataCentralDb.Service/Fanex.AutoSyncDataCentralDb.Service.csproj ./Fanex.AutoSyncDataCentralDb.Service/Fanex.AutoSyncDataCentralDb.Service.csproj

WORKDIR /src/Fanex.AutoSyncDataCentralDb.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore -v n

COPY ./Sources/Fanex.AutoSyncDataCentralDb.Service .

RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.AutoSyncDataCentralDb.Service.dll"]
