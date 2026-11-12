using CropAdvisory.Data;
using CropAdvisory.Services;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CropAdvisory.Controllers;

[Authorize(Roles = "Farmer")]
public class DashboardController : Controller
{
    private readonly IWeatherService _weather;
    private readonly IRecommendationService _recService;
    private readonly ApplicationDbContext _db;

    public DashboardController(IWeatherService weather, IRecommendationService recService, ApplicationDbContext db)
    {
        _weather = weather;
        _recService = recService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var farmer = await _db.Farmers.FindAsync(farmerId);
        if (farmer == null) return RedirectToAction("Login", "Account");

        var weather = await _weather.GetCurrentWeatherAsync(farmer.Location);

        // Determine alert level
        string alertMsg = "Weather looks fine today. Good day for farming!";
        string alertLevel = "green";
        if (weather.RainProbability >= 70)
        {
            alertMsg = "⚠️ Heavy rain expected today. Check field drainage and avoid spraying fertilizers.";
            alertLevel = "red";
        }
        else if (weather.RainProbability >= 40)
        {
            alertMsg = "🌦 Moderate rain possible. Monitor your crops and ensure drainage is clear.";
            alertLevel = "yellow";
        }

        // Determine current season
        var month = DateTime.Now.Month;
        var season = month switch
        {
            >= 6 and <= 10 => "Kharif (Monsoon)",
            >= 11 or <= 2 => "Rabi (Winter)",
            _ => "Zaid (Summer)"
        };

        var vm = new DashboardViewModel
        {
            FarmerName = farmer.Name,
            CurrentDate = DateTime.Now,
            Location = farmer.Location,
            Season = season,
            Weather = weather,
            WeatherAlertMessage = alertMsg,
            WeatherAlertLevel = alertLevel,
            RecentRecommendations = await _recService.GetRecentRecommendationsAsync(farmerId)
        };

        return View(vm);
    }
}
