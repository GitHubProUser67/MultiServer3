@echo off
@echo MultiServer publisher script 05/10/2024
@echo.

@echo Cleaning up directories:
@rmdir /S /Q ~PublishOutput
@echo.

:Build
dotnet restore MultiServer3.sln
dotnet clean MultiServer3.sln
dotnet build "Plugins/HTTP/HomeWebTools/HomeWebTools.csproj" --configuration Debug --property WarningLevel=0
dotnet build "Plugins/HTTP/HomeWebTools/HomeWebTools.csproj" --configuration Release --property WarningLevel=0
dotnet build "Plugins/HTTP/EdNetCRCCalculator/EdNetCRCCalculator.csproj" --configuration Debug --property WarningLevel=0
dotnet build "Plugins/HTTP/EdNetCRCCalculator/EdNetCRCCalculator.csproj" --configuration Release --property WarningLevel=0
dotnet build "Plugins/HTTP/PdfToJpeg/PdfToJpeg.csproj" --configuration Debug --property WarningLevel=0
dotnet build "Plugins/HTTP/PdfToJpeg/PdfToJpeg.csproj" --configuration Release --property WarningLevel=0

@echo off

setlocal enabledelayedexpansion

rem List of RIDs to publish for
set RIDs=win-x64

rem Common parameters
set params=-p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --property WarningLevel=0 --self-contained

for %%r in (%RIDs%) do (
    @echo Publishing MultiServer for %%r ...
    dotnet publish MultiServer3.sln -r %%r -c Debug %params%
    dotnet publish MultiServer3.sln -r %%r -c Release %params%
	
	@echo Copying %%r build output to ~PublishOutput...
	xcopy /E /Y /I "SpecializedServers/Horizon/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/MultiSocks/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/QuazalServer/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/SSFWServer/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/SVO/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/MultiSpy/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "WebServers/HTTPServer/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "WebServers/MitmDNS/bin/Debug/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Debug"
	xcopy /E /Y /I "SpecializedServers/Horizon/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "SpecializedServers/MultiSocks/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "SpecializedServers/QuazalServer/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "SpecializedServers/SSFWServer/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "SpecializedServers/SVO/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "SpecializedServers/MultiSpy/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "WebServers/HTTPServer/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	xcopy /E /Y /I "WebServers/MitmDNS/bin/Release/net6.0/%%r/publish" "~PublishOutput/MultiServer/%%r/Release"
	if exist "Plugins/HTTP/HomeWebTools/bin/Debug/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/HomeWebTools/bin/Debug/net6.0/static" "~PublishOutput/MultiServer/%%r/Debug/static"
	)
	if exist "Plugins/HTTP/HomeWebTools/bin/Release/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/HomeWebTools/bin/Release/net6.0/static" "~PublishOutput/MultiServer/%%r/Release/static"
	)
	if exist "Plugins/HTTP/EdNetCRCCalculator/bin/Debug/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/EdNetCRCCalculator/bin/Debug/net6.0/static" "~PublishOutput/MultiServer/%%r/Debug/static"
	)
	if exist "Plugins/HTTP/EdNetCRCCalculator/bin/Release/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/EdNetCRCCalculator/bin/Release/net6.0/static" "~PublishOutput/MultiServer/%%r/Release/static"
	)
	if exist "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/static" "~PublishOutput/MultiServer/%%r/Debug/static"
	)
	if exist "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/static" (
		xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/static" "~PublishOutput/MultiServer/%%r/Release/static"
	)
	if exist "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/runtimes" (
		xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/runtimes" "~PublishOutput/MultiServer/%%r/Debug/runtimes"
	)
	if exist "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/runtimes" (
		xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/runtimes" "~PublishOutput/MultiServer/%%r/Release/runtimes"
	)
	if "%%r"=="win-x64" (
		xcopy /E /Y /I "Tools/Home/NautilusXP2024/bin/Debug/net6.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/Debug"
		xcopy /E /Y /I "Tools/Home/NautilusXP2024/bin/Release/net6.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/Release"
	)
	if "%%r"=="win-x86" (
		xcopy /E /Y /I "Tools/Home/NautilusXP2024/bin/Debug/net6.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/Debug"
		xcopy /E /Y /I "Tools/Home/NautilusXP2024/bin/Release/net6.0-windows/%%r/publish" "~PublishOutput/Nautilus/%%r/Release"
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