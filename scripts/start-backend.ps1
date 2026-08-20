# Start backend server
$BackendPath = "C:\Users\AXIOO\Documents\Obsidian Vault\3D-AI-Assistant\backend"

Write-Host "Starting 3D AI Assistant backend..."
Write-Host "Backend: $BackendPath"
Write-Host "URL: http://localhost:8000"
Write-Host ""
Write-Host "Press Ctrl+C to stop"
Write-Host ""

Push-Location $BackendPath
python server.py