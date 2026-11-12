using CropAdvisory.Data;
using CropAdvisory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Ensure legacy timestamp behavior for PostgreSQL compatibility across timezones
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Ensure User Secrets are loaded in any environment
builder.Configuration.AddUserSecrets<Program>(optional: true);

// ─── Configuration & Database ───────────────────────────────────────────────
var rawConn = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");

var connStr = ConvertPostgresUrlToConnectionString(rawConn);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connStr))
    {
        options.UseNpgsql(connStr, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
    }
});

// ─── Authentication ─────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ─── Application Services ───────────────────────────────────────────────────
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IPestService, PestService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── Middleware ──────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ─── Routes ─────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ─── Database Migration & Seed ───────────────────────────────────────────────
if (!string.IsNullOrWhiteSpace(connStr))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        DbInitializer.Initialize(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating or seeding the PostgreSQL database.");
    }
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("No DefaultConnection or DATABASE_URL provided. Please configure your Neon PostgreSQL connection string using user secrets or environment variables.");
}

app.Run();

// ─── Helper for PostgreSQL URL parsing ───────────────────────────────────────
static string ConvertPostgresUrlToConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString) ||
        connectionString.Contains("[PASTE YOUR", StringComparison.OrdinalIgnoreCase))
    {
        return string.Empty;
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
