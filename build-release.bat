@echo off
REM ==========================================
REM Cookie Analyzer - Build & Publish Script
REM ==========================================
echo.
echo ======================================
echo Cookie Analyzer - Build Release
echo ======================================
echo.

echo Building Release configuration...
dotnet build -c Release

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Publishing standalone executable...
REM Create publish directory
if not exist "publish" mkdir publish

REM Publish as single self-contained exe
dotnet publish -c Release -o ./publish ^^
    --self-contained true ^^
    -p:PublishSingleFile=true ^^
    -p:IncludeNativeLibrariesForSelfExtract=true ^^
    -p:PublishReadyToRun=true ^^
    --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj

if %ERRORLEVEL% NEQ 0 (
    echo Publish failed!
    pause
    exit /b 1
)

echo.
echo ======================================
echo Build completed successfully!
echo ======================================
echo.
echo Output directory: ./publish
echo EXE file: ./publish/CookieAnalyzer.UI.exe
echo.
echo You can now run: ./publish/CookieAnalyzer.UI.exe
echo.
pause
