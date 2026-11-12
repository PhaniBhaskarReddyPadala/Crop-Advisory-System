using CropAdvisory.Data;
using CropAdvisory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Ensure legacy timestamp behavior for PostgreSQL compatibility across timezones
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Load user secrets across all environments
builder.Configuration.AddUserSecrets<Program>(optional: true);

// Render / Container Dynamic Port Binding
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ─── Configuration & Database ───────────────────────────────────────────────
var rawConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrWhiteSpace(rawConn))
    rawConn = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(rawConn))
    rawConn = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(rawConn))
    rawConn = builder.Configuration.GetConnectionString("DefaultConnection");

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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        DbInitializer.Initialize(
            db,
            Environment.GetEnvironmentVariable("INITIAL_ADMIN_EMAIL"),
            Environment.GetEnvironmentVariable("INITIAL_ADMIN_PASSWORD"),
            logger);
    }
    catch
    {
        logger.LogCritical("Database initialization failed. Verify database connectivity, migrations, and initial-admin configuration.");
        throw new InvalidOperationException("Database initialization failed. Verify the server configuration.");
    }
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("No PostgreSQL connection string is configured. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
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
