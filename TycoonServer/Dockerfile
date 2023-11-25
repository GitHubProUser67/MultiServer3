#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TycoonServer/TycoonServer.csproj", "TycoonServer/"]
RUN dotnet restore "TycoonServer/TycoonServer.csproj"
COPY . .
WORKDIR "/src/TycoonServer"
RUN dotnet build "TycoonServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TycoonServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TycoonServer.dll"]