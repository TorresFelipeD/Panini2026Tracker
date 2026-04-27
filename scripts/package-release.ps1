[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$configPath = Join-Path $projectRoot 'app-config.json'
$runtimePath = Join-Path $projectRoot 'app-runtime'
$releasePath = Join-Path $projectRoot 'release'

if (-not (Test-Path -LiteralPath $configPath)) {
    throw "No se encontro app-config.json en '$configPath'."
}

if (-not (Test-Path -LiteralPath $runtimePath)) {
    throw "No se encontro la carpeta app-runtime en '$runtimePath'. Ejecuta primero el build Release."
}

$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$version = [string]$config.AppMeta.version

if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'AppMeta.version es obligatorio para generar el paquete.'
}

$packageName = "AlbumMundial2026_Tracker_v$version"
$stagingPath = Join-Path $releasePath $packageName
$zipPath = Join-Path $releasePath "$packageName.zip"

if (-not (Test-Path -LiteralPath $releasePath)) {
    New-Item -ItemType Directory -Path $releasePath | Out-Null
}

if (Test-Path -LiteralPath $stagingPath) {
    Remove-Item -LiteralPath $stagingPath -Recurse -Force
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Path $stagingPath | Out-Null
Get-ChildItem -LiteralPath $runtimePath -Force | Copy-Item -Destination $stagingPath -Recurse -Force

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zipFileStream = [System.IO.File]::Open($zipPath, [System.IO.FileMode]::CreateNew)
try {
    $archive = New-Object System.IO.Compression.ZipArchive($zipFileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
    try {
        $baseDirectory = Split-Path -Parent $stagingPath
        $baseUri = [System.Uri](([System.IO.Path]::GetFullPath($baseDirectory).TrimEnd('\') + '\'))
        $files = Get-ChildItem -LiteralPath $stagingPath -Recurse -File

        foreach ($file in $files) {
            $fileUri = [System.Uri]([System.IO.Path]::GetFullPath($file.FullName))
            $entryName = $baseUri.MakeRelativeUri($fileUri).ToString()
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive,
                $file.FullName,
                $entryName,
                [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $zipFileStream.Dispose()
}

Remove-Item -LiteralPath $stagingPath -Recurse -Force
Write-Host "Paquete generado: $zipPath"
