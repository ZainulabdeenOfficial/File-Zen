param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root "FileZen.csproj"
$publishDir = Join-Path $root "artifacts\publish\$Runtime"
$installerDir = Join-Path $root "artifacts\installer"
$innoScript = Join-Path $root "installer\FileZenPro.iss"
$pngIcon = Join-Path $root "FileZen.png"
$icoIcon = Join-Path $root "FileZen.ico"

function Convert-PngToIco {
    param(
        [Parameter(Mandatory = $true)][string]$PngPath,
        [Parameter(Mandatory = $true)][string]$IcoPath
    )

    if (-not (Test-Path $PngPath)) {
        throw "Icon source was not found: $PngPath"
    }

    Add-Type -AssemblyName System.Drawing

    $sizes = @(256, 64, 48, 32, 16)
    $entries = New-Object System.Collections.Generic.List[byte[]]
    $source = [System.Drawing.Image]::FromFile($PngPath)

    try {
        foreach ($size in $sizes) {
            $bitmap = New-Object System.Drawing.Bitmap $size, $size
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.Clear([System.Drawing.Color]::Transparent)

            $scale = [Math]::Min($size / $source.Width, $size / $source.Height)
            $width = [int]($source.Width * $scale)
            $height = [int]($source.Height * $scale)
            $x = [int](($size - $width) / 2)
            $y = [int](($size - $height) / 2)
            $graphics.DrawImage($source, $x, $y, $width, $height)

            $memory = New-Object System.IO.MemoryStream
            $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
            $entries.Add($memory.ToArray())

            $memory.Dispose()
            $graphics.Dispose()
            $bitmap.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }

    $stream = [System.IO.File]::Create($IcoPath)
    $writer = New-Object System.IO.BinaryWriter $stream

    try {
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]$entries.Count)

        $offset = 6 + (16 * $entries.Count)
        for ($i = 0; $i -lt $entries.Count; $i++) {
            $size = $sizes[$i]
            $bytes = $entries[$i]
            $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
            $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]32)
            $writer.Write([UInt32]$bytes.Length)
            $writer.Write([UInt32]$offset)
            $offset += $bytes.Length
        }

        foreach ($bytes in $entries) {
            $writer.Write($bytes)
        }
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

if ((-not (Test-Path $icoIcon)) -or ((Get-Item $pngIcon).LastWriteTime -gt (Get-Item $icoIcon).LastWriteTime)) {
    Write-Host "Creating Windows icon from FileZen.png..." -ForegroundColor Cyan
    Convert-PngToIco -PngPath $pngIcon -IcoPath $icoIcon
}

Write-Host "Publishing FileZen Pro for $Runtime..." -ForegroundColor Cyan
dotnet publish $project `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=false `
    -p:PublishTrimmed=false `
    --output $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host "Published app: $publishDir" -ForegroundColor Green

if ($SkipInstaller) {
    Write-Host "Skipped setup wizard creation." -ForegroundColor Yellow
    exit 0
}

$isccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)

$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) {
    New-Item -ItemType Directory -Force -Path $installerDir | Out-Null
    Write-Host "Inno Setup 6 was not found, so the setup EXE was not created." -ForegroundColor Yellow
    Write-Host "Install Inno Setup 6, then run this script again to generate:" -ForegroundColor Yellow
    Write-Host "  artifacts\installer\FileZenPro-Setup-1.0.0.exe" -ForegroundColor Yellow
    exit 0
}

Write-Host "Building setup wizard with Inno Setup..." -ForegroundColor Cyan
& $iscc $innoScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

Write-Host "Installer ready: $(Join-Path $installerDir 'FileZenPro-Setup-1.0.0.exe')" -ForegroundColor Green
