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

    // The server only resolves the place name to coordinates.
    // The forecast itself is fetched by the visitor's browser (see Views/Weather/Index.cshtml),
    // because Open-Meteo rate-limits (HTTP 429) Render's shared server IP.
    public async Task<IActionResult> Index(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var farmer = await _db.Farmers.FindAsync(farmerId);
            location = farmer?.Location ?? "Hyderabad";
        }

        var geo = await _weather.GeocodeAsync(location);

        return View(new WeatherPageViewModel
        {
            Location = geo?.Name ?? location,
            LocationFound = geo != null,
            Latitude = geo?.Latitude ?? 0,
            Longitude = geo?.Longitude ?? 0
        });
    }
}
