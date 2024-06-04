@echo off
@echo MultiServer build script 03/06/2024
@echo.

@echo Cleaning up directories:
@rmdir /S /Q ~BuildOutput
@echo.

:Build
@echo Building MultiServer...
dotnet restore
dotnet clean
dotnet build

@echo Copying build output to ~BuildOutput...
xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/Horizon/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/MultiSocks/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/QuazalServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SSFWServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SVO/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/MitmDNS/bin" "~BuildOutput/MultiServer"
if exist "Plugins/HomeWebTools/bin/Debug/net8.0/static" (
    xcopy /E /Y /I "Plugins/HomeWebTools/bin/Debug/net8.0/static" "~BuildOutput/MultiServer/Debug/net8.0/static"
)
if exist "Plugins/HomeWebTools/bin/Release/net8.0/static" (
    xcopy /E /Y /I "Plugins/HomeWebTools/bin/Release/net8.0/static" "~BuildOutput/MultiServer/Release/net8.0/static"
)
xcopy /E /Y /I "Plugins/NautilusXP2024/bin" "~BuildOutput/Nautilus"

@echo Cleaning up temp build files and directories:
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"

@echo.

@echo.
@echo All platforms and kinds of packages are processed.
@echo.

@echo Build completed successfully.
@pause