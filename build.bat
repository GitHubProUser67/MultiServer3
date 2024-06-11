@echo off
@echo MultiServer build script 07/06/2024
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
xcopy /E /Y /I "MiddlewareServices/DatabaseMiddleware/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/Horizon/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/MultiSocks/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/QuazalServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SSFWServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "SpecializedServers/SVO/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPSecureServerLite/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/HTTPServer/bin" "~BuildOutput/MultiServer"
xcopy /E /Y /I "WebServers/MitmDNS/bin" "~BuildOutput/MultiServer"
if exist "Plugins/HomeWebTools/bin/Debug/net6.0/static" (
    xcopy /E /Y /I "Plugins/HomeWebTools/bin/Debug/net6.0/static" "~BuildOutput/MultiServer/Debug/net6.0/static"
)
if exist "Plugins/HomeWebTools/bin/Release/net6.0/static" (
    xcopy /E /Y /I "Plugins/HomeWebTools/bin/Release/net6.0/static" "~BuildOutput/MultiServer/Release/net6.0/static"
)
xcopy /E /Y /I "Plugins/NautilusXP2024/bin" "~BuildOutput/Nautilus"

@echo Crafting final output:
if exist "~BuildOutput/MultiServer/Debug/net6.0" (
    xcopy /E /Y /I "~BuildOutput/MultiServer/Debug/net6.0" "~BuildOutput/MultiServer/Debug"
	@rmdir /S /Q "~BuildOutput/MultiServer/Debug/net6.0"
)
if exist "~BuildOutput/MultiServer/Release/net6.0" (
    xcopy /E /Y /I "~BuildOutput/MultiServer/Release/net6.0" "~BuildOutput/MultiServer/Release"
	@rmdir /S /Q "~BuildOutput/MultiServer/Release/net6.0"
)
if exist "~BuildOutput/Nautilus/Debug/net6.0-windows" (
    xcopy /E /Y /I "~BuildOutput/Nautilus/Debug/net6.0-windows" "~BuildOutput/Nautilus/Debug"
	@rmdir /S /Q "~BuildOutput/Nautilus/Debug/net6.0-windows"
)
if exist "~BuildOutput/Nautilus/Release/net6.0-windows" (
    xcopy /E /Y /I "~BuildOutput/Nautilus/Release/net6.0-windows" "~BuildOutput/Nautilus/Release"
	@rmdir /S /Q "~BuildOutput/Nautilus/Release/net6.0-windows"
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