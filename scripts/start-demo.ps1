# Intelligent Property Inspection Platform — Demo Startup Script
# Starts .NET services locally and Python services via Docker.
# Usage: .\scripts\start-demo.ps1

$ErrorActionPreference = "Continue"
$root = Split-Path $PSScriptRoot -Parent

Write-Host "=== Intelligent Property Inspection Platform — Demo ===" -ForegroundColor Cyan
Write-Host ""

# ── Pre-flight checks ──────────────────────────────────────────────────────────

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "ERROR: .NET SDK not found. Install .NET 10 SDK from https://dot.net" -ForegroundColor Red
    exit 1
}
$dotnetVersion = dotnet --version
Write-Host "dotnet $dotnetVersion found" -ForegroundColor Green

$dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCmd) {
    Write-Host "ERROR: Docker not found. Install Docker Desktop from https://docker.com" -ForegroundColor Red
    exit 1
}
Write-Host "docker $(docker --version) found" -ForegroundColor Green
Write-Host ""

# ── Build Docker images for Python services ────────────────────────────────────

Write-Host "Building Python service images (first run takes ~2 minutes)..." -ForegroundColor Yellow
Set-Location $root
docker compose build risk-scoring-service notification-service | Select-String "Built|Error|FAILED" | Write-Host
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed. Check output above." -ForegroundColor Red
    exit 1
}
Write-Host "Images built." -ForegroundColor Green
Write-Host ""

# ── Start Python services in Docker ───────────────────────────────────────────

Write-Host "Starting Python services via Docker..." -ForegroundColor Cyan

docker rm -f pip-risk-scoring pip-notification-service 2>$null | Out-Null

docker run -d --name pip-risk-scoring -p 5004:8000 -e PYTHONUNBUFFERED=1 `
    intelligent-property-inspection-platform-risk-scoring-service:latest | Out-Null
Write-Host "  RiskScoringService started on http://localhost:5004" -ForegroundColor White

docker run -d --name pip-notification-service -p 5005:8000 -e PYTHONUNBUFFERED=1 `
    intelligent-property-inspection-platform-notification-service:latest | Out-Null
Write-Host "  NotificationService started on http://localhost:5005" -ForegroundColor White

# ── Start .NET services ────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Starting .NET services..." -ForegroundColor Cyan

$p1 = Start-Process -FilePath "dotnet" -ArgumentList "run", "--launch-profile", "http" `
    -WorkingDirectory "$root\src\PropertyService\PropertyService.API" `
    -WindowStyle Hidden -PassThru
Write-Host "  PropertyService started on http://localhost:5001 (PID $($p1.Id))" -ForegroundColor White

$p2 = Start-Process -FilePath "dotnet" -ArgumentList "run", "--launch-profile", "http" `
    -WorkingDirectory "$root\src\InspectionService\InspectionService.API" `
    -WindowStyle Hidden -PassThru
Write-Host "  InspectionService started on http://localhost:5002 (PID $($p2.Id))" -ForegroundColor White

$p3 = Start-Process -FilePath "dotnet" -ArgumentList "run", "--launch-profile", "http" `
    -WorkingDirectory "$root\src\DocumentService\DocumentService.API" `
    -WindowStyle Hidden -PassThru
Write-Host "  DocumentService started on http://localhost:5003 (PID $($p3.Id))" -ForegroundColor White

# ── Wait for startup ───────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
$maxWait = 90
$elapsed = 0
$allUp = $false

while ($elapsed -lt $maxWait) {
    Start-Sleep -Seconds 5
    $elapsed += 5
    $up = 0
    foreach ($port in @(5001,5002,5003,5004,5005)) {
        try {
            $r = Invoke-WebRequest -Uri "http://localhost:$port/health" -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($r.StatusCode -eq 200) { $up++ }
        } catch {}
    }
    Write-Host "  $elapsed s — $up/5 services healthy" -ForegroundColor Gray
    if ($up -eq 5) { $allUp = $true; break }
}

Write-Host ""
if ($allUp) {
    Write-Host "=== All services are UP! ===" -ForegroundColor Green
} else {
    Write-Host "=== Some services may still be starting (this is normal) ===" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "  Property Service      http://localhost:5001          Swagger: http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Inspection Service    http://localhost:5002          Swagger: http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Document Service      http://localhost:5003          Swagger: http://localhost:5003/swagger" -ForegroundColor White
Write-Host "  Risk Scoring Service  http://localhost:5004          Docs:    http://localhost:5004/docs" -ForegroundColor White
Write-Host "  Notification Service  http://localhost:5005          Docs:    http://localhost:5005/docs" -ForegroundColor White
Write-Host ""
Write-Host "  Demo Dashboard (open this in your browser):" -ForegroundColor Cyan
Write-Host "  $root\demo\dashboard\index.html" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C or close this window to stop. .NET services run as background processes." -ForegroundColor Gray
Write-Host "To stop all services: docker rm -f pip-risk-scoring pip-notification-service" -ForegroundColor Gray

try {
    while ($true) { Start-Sleep -Seconds 10 }
} finally {
    Write-Host "`nStopping Python services..." -ForegroundColor Yellow
    docker stop pip-risk-scoring pip-notification-service 2>$null | Out-Null
    docker rm pip-risk-scoring pip-notification-service 2>$null | Out-Null
    Write-Host "Stopping .NET services..." -ForegroundColor Yellow
    Stop-Process -Id $p1.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $p2.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $p3.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Done." -ForegroundColor Green
}
