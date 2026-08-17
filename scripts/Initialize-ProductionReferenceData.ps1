$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__DefaultConnection)) {
    throw 'Inject ConnectionStrings__DefaultConnection from the deployment secret manager before initializing reference data.'
}

$publishDirectory = Join-Path (Get-Location) 'deployment\artifacts\publish'
$application = Join-Path $publishDirectory 'Web.dll'
if (-not (Test-Path -LiteralPath $application)) {
    throw 'Publish the reviewed production artifact before initializing reference data.'
}

$env:ASPNETCORE_ENVIRONMENT = 'Production'
dotnet $application --initialize-reference-data
if ($LASTEXITCODE -ne 0) {
    throw "Reference-data initialization failed with exit code $LASTEXITCODE."
}

Write-Output 'Production reference data initialized successfully.'
