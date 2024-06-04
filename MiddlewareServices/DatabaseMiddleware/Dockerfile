#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MiddlewareServices/DatabaseMiddleware/DatabaseMiddleware.csproj", "DatabaseMiddleware/"]
RUN dotnet restore "MiddlewareServices/DatabaseMiddleware/DatabaseMiddleware.csproj"
COPY . .
WORKDIR "/src/DatabaseMiddleware"
RUN dotnet build "DatabaseMiddleware.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatabaseMiddleware.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseMiddleware.dll"]