$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $repoRoot 'src\Kine.Api\Kine.Api.csproj'
$webRoot = Join-Path $repoRoot 'src\Kine.Web'

Start-Process -FilePath powershell -WorkingDirectory $repoRoot -ArgumentList @(
    '-NoExit',
    '-Command',
    "dotnet run --project '$apiProject' --urls http://localhost:5080"
)

Start-Process -FilePath powershell -WorkingDirectory $webRoot -ArgumentList @(
    '-NoExit',
    '-Command',
    'npm run dev'
)

Write-Host 'Backend and frontend started in separate PowerShell windows.'