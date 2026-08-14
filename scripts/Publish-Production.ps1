param(
    [string]$OutputPath = (Join-Path (Get-Location) 'deployment\artifacts\publish')
)

$ErrorActionPreference = 'Stop'
$project = Join-Path (Get-Location) 'Web\Web.csproj'
dotnet publish $project -c Release -o $OutputPath --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Production publish failed with exit code $LASTEXITCODE."
}

$developmentSettings = Join-Path $OutputPath 'appsettings.Development.json'
if (Test-Path -LiteralPath $developmentSettings) {
    throw 'Production publish contains appsettings.Development.json.'
}

Write-Output "Published production artifact to $OutputPath"
