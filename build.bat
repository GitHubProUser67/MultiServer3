@echo off
@echo MultiServer build script 23/02/2025
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
xcopy /E /Y /I "Servers/Horizon/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/MultiSocks/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/QuazalServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/SSFWServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/SVO/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/MultiSpy/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/ApacheNet/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "Servers/MitmDNS/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "RemoteControl/bin/Debug/net6.0-windows" "~BuildOutput/MultiServer/Debug"
xcopy /E /Y /I "RemoteControl/bin/Release/net6.0-windows" "~BuildOutput/MultiServer/Release"
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