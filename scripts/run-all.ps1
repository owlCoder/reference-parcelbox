$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

$processes = @()
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT

try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'

    dotnet build ParcelBox.sln

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $projects = @(
        @{
            Path = 'simulators/ParcelBox.Simulators.LockerController'
            Url = 'http://localhost:5101'
        },
        @{
            Path = 'simulators/ParcelBox.Simulators.MessageGateway'
            Url = 'http://localhost:5102'
        },
        @{
            Path = 'src/ParcelBox.Api'
            Url = 'http://localhost:5100'
        },
        @{
            Path = 'src/ParcelBox.Web'
            Url = 'http://localhost:5000'
        }
    )

    foreach ($project in $projects) {
        $arguments = @(
            'run',
            '--no-build',
            '--no-launch-profile',
            '--urls', $project.Url
        )

        $processes += Start-Process `
            -FilePath 'dotnet' `
            -ArgumentList $arguments `
            -WorkingDirectory (Join-Path $repoRoot $project.Path) `
            -NoNewWindow `
            -PassThru
    }

    Write-Host 'ParcelBox Web:             http://localhost:5000'
    Write-Host 'ParcelBox API:             http://localhost:5100'
    Write-Host 'Locker Controller:         http://localhost:5101'
    Write-Host 'Message Gateway:           http://localhost:5102'
    Write-Host 'Environment:               Development'
    Write-Host 'Press Ctrl+C to stop all processes.'

    $processes | Wait-Process
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }

    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Pop-Location
}
