# EscapeSchool Test Runner Script
# This script runs Unity tests from the command line

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("EditMode", "PlayMode", "All")]
    [string]$TestPlatform = "All",
    
    [Parameter(Mandatory = $false)]
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe",
    
    [Parameter(Mandatory = $false)]
    [string]$ResultsPath = "TestResults"
)

# Find Unity executable
$unityExe = Get-ChildItem -Path $UnityPath -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName

if (-not $unityExe) {
    Write-Host "Unity executable not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please specify the correct Unity path using -UnityPath parameter" -ForegroundColor Yellow
    Write-Host "Example: .\run-tests.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe'" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using Unity: $unityExe" -ForegroundColor Green

# Create results directory
if (-not (Test-Path $ResultsPath)) {
    New-Item -ItemType Directory -Path $ResultsPath | Out-Null
}

$projectPath = Join-Path $PSScriptRoot "EscapeSchool"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

function Invoke-UnityTests {
    param([string]$Platform)
    
    $resultsFile = Join-Path $ResultsPath "TestResults_${Platform}_${timestamp}.xml"
    
    Write-Host "`nRunning $Platform tests..." -ForegroundColor Cyan
    Write-Host "Results will be saved to: $resultsFile" -ForegroundColor Gray
    
    $arguments = @(
        "-runTests",
        "-batchmode",
        "-projectPath", "`"$projectPath`"",
        "-testResults", "`"$resultsFile`"",
        "-testPlatform", $Platform,
        "-logFile", "-"
    )
    
    $process = Start-Process -FilePath $unityExe -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ $Platform tests PASSED" -ForegroundColor Green
        return $true
    }
    elseif ($process.ExitCode -eq 2) {
        Write-Host "✗ $Platform tests FAILED" -ForegroundColor Red
        return $false
    }
    else {
        Write-Host "✗ $Platform tests ERROR (Exit code: $($process.ExitCode))" -ForegroundColor Red
        return $false
    }
}

# Run tests based on platform selection
$allPassed = $true

if ($TestPlatform -eq "All") {
    $editModePassed = Invoke-UnityTests -Platform "EditMode"
    $playModePassed = Invoke-UnityTests -Platform "PlayMode"
    $allPassed = $editModePassed -and $playModePassed
}
else {
    $allPassed = Invoke-UnityTests -Platform $TestPlatform
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "All tests completed successfully!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "Some tests failed. Check results in: $ResultsPath" -ForegroundColor Red
    exit 1
}
