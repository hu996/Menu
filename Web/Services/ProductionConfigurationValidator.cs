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
        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
            throw new InvalidOperationException("Production must apply EF migrations as a deployment step, not during application startup.");

        if (!string.Equals(configuration["Storage:Provider"], "ObjectStorage", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production must use the object-storage provider.");
        Require(configuration["Storage:Endpoint"], "Storage:Endpoint");
        Require(configuration["Storage:Bucket"], "Storage:Bucket");
        Require(configuration["Storage:Region"], "Storage:Region");
        Require(configuration["Storage:AccessKey"], "Storage:AccessKey");
        Require(configuration["Storage:SecretKey"], "Storage:SecretKey");

        if (string.Equals(configuration["Payments:Provider"], "Sandbox", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production must not use the sandbox payment gateway.");
        Require(configuration["Payments:WebhookSecret"], "Payments:WebhookSecret");

        if (string.Equals(configuration["Email:Provider"], "NotConfigured", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Production email delivery must be selected through external configuration.");
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
}
