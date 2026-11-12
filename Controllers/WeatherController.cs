using CropAdvisory.Data;
using CropAdvisory.Services;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CropAdvisory.Controllers;

[Authorize(Roles = "Farmer")]
public class WeatherController : Controller
{
    private readonly IWeatherService _weather;
    private readonly ApplicationDbContext _db;

    public WeatherController(IWeatherService weather, ApplicationDbContext db)
    {
        _weather = weather;
        _db = db;
    }

    public async Task<IActionResult> Index(string? location)
    {
        // Default to farmer's registered location
        if (string.IsNullOrWhiteSpace(location))
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var farmer = await _db.Farmers.FindAsync(farmerId);
            location = farmer?.Location ?? "Hyderabad";
        }

        var current = await _weather.GetCurrentWeatherAsync(location);
        var forecast = await _weather.GetForecastAsync(location);

        // Generate advisories
        var advisories = new List<string>();
        string alertLevel = "green";

        if (current.RainProbability >= 70)
        {
            advisories.Add("🌧️ Heavy rain expected. Check field drainage. Avoid applying fertilizers.");
            alertLevel = "red";
        }
        else if (current.RainProbability >= 40)
        {
            advisories.Add("🌦️ Moderate rain possible. Monitor crop fields and ensure drainage.");
            alertLevel = "yellow";
        }
        else
        {
            advisories.Add("☀️ Clear weather. Good day for field activities and spraying.");
        }

        if (current.WindSpeed >= 40)
            advisories.Add("💨 Strong winds. Secure covers, avoid spraying pesticides.");
        if (current.TemperatureC >= 38)
            advisories.Add("🌡️ Very hot. Water crops early morning or evening. Protect cattle.");

        var vm = new WeatherPageViewModel
        {
            Location = location,
            Current = current,
            Forecast = forecast,
            Advisories = advisories,
            AlertLevel = alertLevel
        };

        return View(vm);
    }
}
