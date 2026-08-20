# Sync backend from Obsidian Vault to Unity project
$BackendSource = "C:\Users\AXIOO\Documents\Obsidian Vault\3D-AI-Assistant\backend"
$BackendDest = "C:\Users\AXIOO\Unity Projects\3D-AI-Assistant\backend"

Write-Host "Syncing backend from $BackendSource to $BackendDest..."

# Create destination if not exists
if (-not (Test-Path $BackendDest)) {
    New-Item -ItemType Directory -Force -Path $BackendDest | Out-Null
    Write-Host "Created: $BackendDest"
}

# Copy all files except .env and __pycache__
Get-ChildItem -Path $BackendSource -Recurse -File | ForEach-Object {
    $RelativePath = $_.FullName.Substring($BackendSource.Length).TrimStart('\')
    $DestPath = Join-Path $BackendDest $RelativePath
    
    if ($_.Name -eq '.env') {
        Write-Host "Skipping: $RelativePath (contains secrets)"
        return
    }
    if ($_.DirectoryName -match '__pycache__') {
        Write-Host "Skipping: $RelativePath (__pycache__)"
        return
    }
    
    $DestDir = Split-Path $DestPath -Parent
    if (-not (Test-Path $DestDir)) {
        New-Item -ItemType Directory -Force -Path $DestDir | Out-Null
    }
    
    Copy-Item -Path $_.FullName -Destination $DestPath -Force
    Write-Host "Synced: $RelativePath"
}

# Create .env.example if not exists
$EnvExample = Join-Path $BackendDest '.env.example'
$EnvSource = Join-Path $BackendSource '.env.example'
if (Test-Path $EnvSource) {
    if (-not (Test-Path $EnvExample)) {
        Copy-Item -Path $EnvSource -Destination $EnvExample -Force
        Write-Host "Created: .env.example"
    }
}

# Create .env if not exists (empty)
$EnvFile = Join-Path $BackendDest '.env'
if (-not (Test-Path $EnvFile)) {
    New-Item -ItemType File -Force -Path $EnvFile | Out-Null
    Write-Host "Created: .env (add your GROQ_API_KEY)"
}

Write-Host ""
Write-Host "Backend synced successfully!"
Write-Host "Next: cd backend && pip install -r requirements.txt"