#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebServers/HTTPSecureServerLite/HTTPSecureServerLite.csproj", "HTTPSecureServerLite/"]
RUN dotnet restore "WebServers/HTTPSecureServerLite/HTTPSecureServerLite.csproj"
COPY . .
WORKDIR "/src/HTTPSecureServerLite"
RUN dotnet build "HTTPSecureServerLite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HTTPSecureServerLite.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HTTPSecureServerLite.dll"]