using System.Data.Common;

namespace RestaurantMenuPlatform.Web.Services;

public static class ProductionConfigurationValidator
{
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be supplied through external configuration.");

        if (!environment.IsProduction())
            return;

        var connection = new DbConnectionStringBuilder { ConnectionString = connectionString };
        var dataSource = GetValue(connection, "Data Source", "Server");
        if (string.IsNullOrWhiteSpace(dataSource) ||
            dataSource.Contains("SQLEXPRESS", StringComparison.OrdinalIgnoreCase) ||
            dataSource.Contains("LOCALDB", StringComparison.OrdinalIgnoreCase) ||
            dataSource.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            dataSource.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            dataSource.Equals(".", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production must use an externally supplied non-local SQL Server data source.");

        if (!bool.TryParse(GetValue(connection, "Encrypt"), out var encrypt) || !encrypt)
            throw new InvalidOperationException("Production SQL Server connections must explicitly enable encryption.");
        if (bool.TryParse(GetValue(connection, "TrustServerCertificate"), out var trustServerCertificate) && trustServerCertificate)
            throw new InvalidOperationException("Production SQL Server connections must validate the server certificate.");

        var allowedHosts = configuration["AllowedHosts"]?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        if (allowedHosts.Length == 0 || allowedHosts.Any(x => x == "*"))
            throw new InvalidOperationException("Production AllowedHosts must explicitly list the public hostnames.");

        if (!configuration.GetValue("Security:RequireHttps", false))
            throw new InvalidOperationException("Production must require HTTPS.");
        if (configuration.GetValue("Security:PrincipalValidationCacheSeconds", 15) is < 5 or > 60)
            throw new InvalidOperationException("Security:PrincipalValidationCacheSeconds must be between 5 and 60 seconds.");
        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
            throw new InvalidOperationException("Production must apply EF migrations as a deployment step, not during application startup.");
        if (configuration.GetValue("Database:InitializeReferenceDataOnStartup", true))
            throw new InvalidOperationException("Production reference data must be initialized as a deployment step, not by every web replica.");
        if (!string.Equals(configuration["Session:Provider"], "SqlServer", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production sessions must use the shared SQL Server cache provider.");
        Require(configuration["Security:DataProtectionKeysPath"], "Security:DataProtectionKeysPath");

        var knownProxies = configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
        if (knownProxies.Length == 0 || knownProxies.Any(x => !System.Net.IPAddress.TryParse(x, out _)))
            throw new InvalidOperationException("Production ReverseProxy:KnownProxies must contain trusted proxy IP addresses.");

        if (!string.Equals(configuration["Storage:Provider"], "ObjectStorage", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production must use the object-storage provider.");
        Require(configuration["Storage:Endpoint"], "Storage:Endpoint");
        Require(configuration["Storage:Bucket"], "Storage:Bucket");
        Require(configuration["Storage:Region"], "Storage:Region");
        Require(configuration["Storage:AccessKey"], "Storage:AccessKey");
        Require(configuration["Storage:SecretKey"], "Storage:SecretKey");
        RequireHttpsUri(configuration["Storage:Endpoint"], "Storage:Endpoint");
        if (configuration.GetValue<long>("Storage:MaxUploadBytes") is < 1 or > 20_971_520)
            throw new InvalidOperationException("Storage:MaxUploadBytes must be between 1 byte and 20 MB.");

        if (!string.Equals(configuration["Payments:Provider"], "External", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production must use the external payment gateway.");
        RequireHttpsUri(configuration["Payments:ApiBaseUrl"], "Payments:ApiBaseUrl");
        RequireSecret(configuration["Payments:ApiKey"], "Payments:ApiKey", 24);
        RequireSecret(configuration["Payments:WebhookSecret"], "Payments:WebhookSecret", 32);
        RequireHttpsUri(configuration["Payments:SuccessUrl"], "Payments:SuccessUrl");
        RequireHttpsUri(configuration["Payments:CancelUrl"], "Payments:CancelUrl");
        var checkoutHosts = configuration.GetSection("Payments:AllowedCheckoutHosts").Get<string[]>() ?? [];
        if (checkoutHosts.Length == 0 || checkoutHosts.Any(x => string.IsNullOrWhiteSpace(x) || x.Contains('/') || x.Contains('*')))
            throw new InvalidOperationException("Payments:AllowedCheckoutHosts must explicitly list trusted checkout hostnames.");

        if (!string.Equals(configuration["Email:Provider"], "Smtp", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production email delivery must use the configured SMTP provider.");
        RequireHttpsUri(configuration["Email:PublicBaseUrl"], "Email:PublicBaseUrl");
        Require(configuration["Email:FromAddress"], "Email:FromAddress");
        Require(configuration["Email:Smtp:Host"], "Email:Smtp:Host");
        Require(configuration["Email:Smtp:Username"], "Email:Smtp:Username");
        RequireSecret(configuration["Email:Smtp:Password"], "Email:Smtp:Password", 12);
        if (!configuration.GetValue("Email:Smtp:EnableSsl", true))
            throw new InvalidOperationException("Production SMTP delivery must enable TLS.");
    }

    private static string? GetValue(DbConnectionStringBuilder connection, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (connection.TryGetValue(key, out var value))
                return Convert.ToString(value);
        }

        return null;
    }

    private static void Require(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} must be supplied through external configuration.");
    }

    private static void RequireSecret(string? value, string key, int minimumLength)
    {
        Require(value, key);
        if (value!.Trim().Length < minimumLength)
            throw new InvalidOperationException($"{key} must contain at least {minimumLength} characters.");
    }

    private static void RequireHttpsUri(string? value, string key)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException($"{key} must be an absolute HTTPS URL.");
    }
}
