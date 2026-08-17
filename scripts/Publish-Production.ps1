param(
    [string]$OutputPath = (Join-Path (Get-Location) 'deployment\artifacts\publish')
)

$ErrorActionPreference = 'Stop'
$project = Join-Path (Get-Location) 'Web\Web.csproj'
dotnet restore $project --locked-mode --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Locked production restore failed with exit code $LASTEXITCODE."
}

dotnet publish $project -c Release -o $OutputPath --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Production publish failed with exit code $LASTEXITCODE."
}

$developmentSettings = Join-Path $OutputPath 'appsettings.Development.json'
if (Test-Path -LiteralPath $developmentSettings) {
    throw 'Production publish contains appsettings.Development.json.'
}

$manifestPath = Join-Path $OutputPath 'SHA256SUMS.txt'
$manifestLines = Get-ChildItem -LiteralPath $OutputPath -File -Recurse |
    Where-Object { $_.FullName -ne $manifestPath } |
    Sort-Object FullName |
    ForEach-Object {
        $relativePath = [System.IO.Path]::GetRelativePath($OutputPath, $_.FullName).Replace('\', '/')
        $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $relativePath"
    }
Set-Content -LiteralPath $manifestPath -Value $manifestLines -Encoding utf8

Write-Output "Published production artifact to $OutputPath"
