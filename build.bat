@echo off
@echo MultiServer build script 26/11/2024
@echo.

@echo Cleaning up directories:
@rmdir /S /Q ~BuildOutput
@echo.

:Build
@echo Building MultiServer...
dotnet restore MultiServer3.sln
dotnet clean MultiServer3.sln
dotnet build MultiServer3.sln --configuration Debug --property WarningLevel=0
dotnet build MultiServer3.sln --configuration Release --property WarningLevel=0

@echo Copying build output to ~BuildOutput...
xcopy /E /Y /I "SpecializedServers/Horizon/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/MultiSocks/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/QuazalServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SSFWServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SVO/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/MultiSpy/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/MitmDNS/bin" "~BuildOutput/MultiServer"
if exist "Plugins/HTTP/HomeWebTools/bin/Debug/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/HomeWebTools/bin/Debug/net6.0/static" "~BuildOutput/MultiServer/Debug/net6.0/static"
)
if exist "Plugins/HTTP/HomeWebTools/bin/Release/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/HomeWebTools/bin/Release/net6.0/static" "~BuildOutput/MultiServer/Release/net6.0/static"
)
if exist "Plugins/HTTP/EdNetCRCCalculator/bin/Debug/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/EdNetCRCCalculator/bin/Debug/net6.0/static" "~BuildOutput/MultiServer/Debug/net6.0/static"
)
if exist "Plugins/HTTP/EdNetCRCCalculator/bin/Release/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/EdNetCRCCalculator/bin/Release/net6.0/static" "~BuildOutput/MultiServer/Release/net6.0/static"
)
if exist "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/static" "~BuildOutput/MultiServer/Debug/net6.0/static"
)
if exist "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/static" (
    xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/static" "~BuildOutput/MultiServer/Release/net6.0/static"
)
if exist "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/runtimes" (
    xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Debug/net6.0/runtimes" "~BuildOutput/MultiServer/Debug/net6.0/runtimes"
)
if exist "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/runtimes" (
    xcopy /E /Y /I "Plugins/HTTP/PdfToJpeg/bin/Release/net6.0/runtimes" "~BuildOutput/MultiServer/Release/net6.0/runtimes"
)

@echo Crafting final output:
if exist "~BuildOutput/MultiServer/Debug/net6.0" (
    xcopy /E /Y /I "~BuildOutput/MultiServer/Debug/net6.0" "~BuildOutput/MultiServer/Debug"
	@rmdir /S /Q "~BuildOutput/MultiServer/Debug/net6.0"
)
if exist "~BuildOutput/MultiServer/Release/net6.0" (
    xcopy /E /Y /I "~BuildOutput/MultiServer/Release/net6.0" "~BuildOutput/MultiServer/Release"
	@rmdir /S /Q "~BuildOutput/MultiServer/Release/net6.0"
)
@echo.

@echo Cleaning up temp build files and directories:
for /d /r . %%d in (bin,obj,".vs") do @if exist "%%d" rd /s/q "%%d"

@echo.

@echo.
@echo All platforms and kinds of packages are processed.
@echo.

@echo Build completed successfully.
@pause