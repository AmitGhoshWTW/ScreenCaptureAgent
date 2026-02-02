@echo off
setlocal enabledelayedexpansion

echo ================================================================
echo        Screen Capture Agent - Build Script
echo ================================================================
echo.

set CONFIGURATION=Release
if "%1"=="Debug" set CONFIGURATION=Debug

set SOLUTION_DIR=%~dp0
set PROJECT_PATH=%SOLUTION_DIR%ScreenCaptureAgent\ScreenCaptureAgent.csproj
set PUBLISH_DIR=%SOLUTION_DIR%publish\%CONFIGURATION%

echo Configuration: %CONFIGURATION%
echo.

:: Clean
if exist "%PUBLISH_DIR%" (
    echo Cleaning previous build...
    rd /s /q "%PUBLISH_DIR%"
)

:: Restore
echo Restoring NuGet packages...
dotnet restore "%SOLUTION_DIR%ScreenCaptureAgent.sln"
if errorlevel 1 (
    echo ERROR: Restore failed!
    exit /b 1
)

:: Build
echo Building solution...
dotnet build "%SOLUTION_DIR%ScreenCaptureAgent.sln" --configuration %CONFIGURATION% --no-restore
if errorlevel 1 (
    echo ERROR: Build failed!
    exit /b 1
)

:: Publish
echo Publishing self-contained executable...
dotnet publish "%PROJECT_PATH%" ^
    --configuration %CONFIGURATION% ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "%PUBLISH_DIR%" ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true

if errorlevel 1 (
    echo ERROR: Publish failed!
    exit /b 1
)

echo.
echo ================================================================
echo Build completed successfully!
echo ================================================================
echo.
echo Output: %PUBLISH_DIR%
echo.
echo Test the executable:
echo   cd "%PUBLISH_DIR%"
echo   ScreenCapture.exe --help
echo.

endlocal
