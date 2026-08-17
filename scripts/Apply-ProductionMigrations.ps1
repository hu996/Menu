param(
    [ValidateSet('Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__DefaultConnection)) {
    throw 'Inject ConnectionStrings__DefaultConnection from the deployment secret manager before applying migrations.'
}

$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:EF_DESIGN_TIME = 'true'
try {
    dotnet restore Web\Web.csproj --locked-mode --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Locked restore failed with exit code $LASTEXITCODE."
    }

    dotnet ef database update --project Infrastructure --startup-project Web --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "EF migration command failed with exit code $LASTEXITCODE."
    }
}
finally {
    Remove-Item Env:EF_DESIGN_TIME -ErrorAction SilentlyContinue
}
