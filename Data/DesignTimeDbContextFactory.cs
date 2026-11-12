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

        var rawConn = config.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                      ?? Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");

        var connStr = ConvertPostgresUrlToConnectionString(rawConn);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connStr);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ConvertPostgresUrlToConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("[PASTE YOUR", StringComparison.OrdinalIgnoreCase))
        {
            return "Host=localhost;Database=CropAdvisoryDb;Username=postgres;Password=postgres";
        }

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
