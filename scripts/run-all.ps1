$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

$processes = @()
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT

function Assert-PortAvailable {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Port
    )

    $listener = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue

    if ($null -ne $listener) {
        throw "Port $Port is already in use. Stop the process using it and run the script again."
    }
}

function Wait-Endpoint {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    for ($attempt = 1; $attempt -le 30; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2

            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    throw "Service did not become ready: $Url"
}

try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'

    foreach ($port in @(5200, 5100, 5101, 5102)) {
        Assert-PortAvailable -Port $port
    }

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
            Url = 'http://localhost:5200'
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

    Wait-Endpoint -Url 'http://localhost:5101/health'
    Wait-Endpoint -Url 'http://localhost:5102/health'
    Wait-Endpoint -Url 'http://localhost:5100/health'
    Wait-Endpoint -Url 'http://localhost:5200/'

    Write-Host ''
    Write-Host 'ParcelBox is ready.' -ForegroundColor Green
    Write-Host 'ParcelBox Web:             http://localhost:5200'
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
