#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Servers/Horizon/*", "Horizon/"]
RUN dotnet restore "Horizon/Horizon_docker.csproj"
COPY . .
WORKDIR "/src/Horizon"
RUN dotnet build "Horizon_docker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Horizon_docker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Horizon.dll"]