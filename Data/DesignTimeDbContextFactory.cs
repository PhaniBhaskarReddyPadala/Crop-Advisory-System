using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;

namespace CropAdvisory.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var rawConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(rawConn))
            rawConn = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(rawConn))
            rawConn = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(rawConn))
            rawConn = config.GetConnectionString("DefaultConnection");

        var connStr = ConvertPostgresUrlToConnectionString(rawConn);
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("Configure a PostgreSQL connection string for EF design-time operations.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connStr);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ConvertPostgresUrlToConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("[PASTE YOUR", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            var npgsqlBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "",
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                Pooling = true
            };
            return npgsqlBuilder.ConnectionString;
        }

        return connectionString;
    }
}
