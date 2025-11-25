#!/usr/bin/env pwsh
Set-Location "D:\Proyectos\App Web\Backend-Energix"
Write-Host "Creating migration..." -ForegroundColor Green
dotnet ef migrations add AddSubscriptionsTables --context AppDbContext --verbose
Write-Host "Done!" -ForegroundColor Green

