param(
    [string]$Tag = "latest",
    [string]$Registry = "harbor.labs.local/ganka",
    [string]$ApiUrl = "https://ganka-api.gosei.space"
)

$ErrorActionPreference = "Stop"

$backendImage = "$Registry/backend:$Tag"
$frontendImage = "$Registry/frontend:$Tag"

Write-Host "Building backend image..." -ForegroundColor Cyan
docker build -t $backendImage -f backend/Dockerfile backend/
if ($LASTEXITCODE -ne 0) { throw "Backend build failed" }

Write-Host "Building frontend image..." -ForegroundColor Cyan
docker build -t $frontendImage --build-arg VITE_API_URL=$ApiUrl -f frontend/Dockerfile frontend/
if ($LASTEXITCODE -ne 0) { throw "Frontend build failed" }

Write-Host "Pushing backend image..." -ForegroundColor Cyan
docker push $backendImage
if ($LASTEXITCODE -ne 0) { throw "Backend push failed" }

Write-Host "Pushing frontend image..." -ForegroundColor Cyan
docker push $frontendImage
if ($LASTEXITCODE -ne 0) { throw "Frontend push failed" }

Write-Host "Done! Pushed:" -ForegroundColor Green
Write-Host "  $backendImage"
Write-Host "  $frontendImage"
