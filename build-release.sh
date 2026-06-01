#!/bin/bash
echo "======================================"
echo "Cookie Analyzer - Build & Publish"
echo "======================================"
echo ""
echo "Building Release configuration..."
dotnet build -c Release

echo ""
echo "Publishing standalone executable..."
dotnet publish -c Release -o ./publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo ""
echo "Build completed!"
echo "Output directory: ./publish"
echo "EXE file: ./publish/CookieAnalyzer.UI.exe"
echo ""
