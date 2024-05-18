@echo MultiServer build script	17/05/2024
@echo.

@echo Clean up directories:
@rmdir ~BuildOutput /S
@echo.

:Build
@echo Building MultiServer...
dotnet restore
dotnet clean
dotnet build

@echo.
@echo All platforms and kinds of packages are processed.
@echo.

@GOTO :EOF