@echo off
@echo MultiServer publisher script 05/06/2024
@echo.

@echo Cleaning up directories:
@rmdir /S /Q ~PublishOutput
@echo.

:Build
dotnet restore
dotnet clean
dotnet build "Plugins/HomeWebTools/HomeWebTools.csproj" --configuration Debug --property WarningLevel=0
dotnet build "Plugins/HomeWebTools/HomeWebTools.csproj" --configuration Release --property WarningLevel=0

@echo off

setlocal enabledelayedexpansion

rem List of RIDs to publish for
set RIDs=win-x64 win-x86 osx-x64 osx-arm64 linux-x64 linux-arm linux-arm64

rem Common parameters
set params=-p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --property WarningLevel=0 --self-contained

for %%r in (%RIDs%) do (
    @echo Publishing MultiServer for %%r ...
    dotnet publish -r %%r -c Debug %params%
    dotnet publish -r %%r -c Release %params%
	
	@echo Copying %%r build output to ~PublishOutput...
	if "%%r"=="win-x64" (
		xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin/Debug/net8.0/%%r/publish" "~PublishOutput/Database/%%r/net8.0/Debug"
	)
	if "%%r"=="win-x86" (
		xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin/Debug/net8.0/%%r/publish" "~PublishOutput/Database/%%r/net8.0/Debug"
	)
	xcopy /E /Y /I "SpecializedServers/Horizon/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net6.0/Debug"
	xcopy /E /Y /I "SpecializedServers/MultiSocks/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "SpecializedServers/QuazalServer/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "SpecializedServers/SSFWServer/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "SpecializedServers/SVO/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "WebServers/HTTPServer/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	xcopy /E /Y /I "WebServers/MitmDNS/bin/Debug/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Debug"
	if "%%r"=="win-x64" (
		xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin/Release/net8.0/%%r/publish" "~PublishOutput/Database/%%r/net8.0/Release"
	)
	if "%%r"=="win-x86" (
		xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin/Release/net8.0/%%r/publish" "~PublishOutput/Database/%%r/net8.0/Release"
	)
	xcopy /E /Y /I "SpecializedServers/Horizon/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net6.0/Release"
	xcopy /E /Y /I "SpecializedServers/MultiSocks/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "SpecializedServers/QuazalServer/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "SpecializedServers/SSFWServer/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "SpecializedServers/SVO/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "WebServers/HTTPServer/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	xcopy /E /Y /I "WebServers/MitmDNS/bin/Release/net8.0/%%r/publish" "~PublishOutput/MultiServer/%%r/net8.0/Release"
	if exist "Plugins/HomeWebTools/bin/Debug/net8.0/static" (
		xcopy /E /Y /I "Plugins/HomeWebTools/bin/Debug/net8.0/static" "~PublishOutput/MultiServer/%%r/net8.0/Debug/static"
	)
	if exist "Plugins/HomeWebTools/bin/Release/net8.0/static" (
		xcopy /E /Y /I "Plugins/HomeWebTools/bin/Release/net8.0/static" "~PublishOutput/MultiServer/%%r/net8.0/Release/static"
	)
	if "%%r"=="win-x64" (
		xcopy /E /Y /I "Plugins/NautilusXP2024/bin/Debug/net8.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/net8.0/Debug"
		xcopy /E /Y /I "Plugins/NautilusXP2024/bin/Release/net8.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/net8.0/Release"
	)
	if "%%r"=="win-x86" (
		xcopy /E /Y /I "Plugins/NautilusXP2024/bin/Debug/net8.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/net8.0/Debug"
		xcopy /E /Y /I "Plugins/NautilusXP2024/bin/Release/net8.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/net8.0/Release"
	)
)

endlocal

@echo Cleaning up temp build files and directories:
for /d /r . %%d in (bin,obj,".vs") do @if exist "%%d" rd /s/q "%%d"

@echo.

@echo.
@echo All platforms and kinds of packages are processed.
@echo.

@echo Publishing completed successfully.
@pause