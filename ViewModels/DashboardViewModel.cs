// ViewModel for Farmer Dashboard
using System;
using System.Collections.Generic;
using CropAdvisory.Models;

namespace CropAdvisory.ViewModels;

public class DashboardViewModel
{
    public string FarmerName { get; set; } = string.Empty;
    public DateTime CurrentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;

    // Weather info
    public WeatherInfoDto Weather { get; set; } = new();
    public string WeatherAlertMessage { get; set; } = string.Empty;
    public string WeatherAlertLevel { get; set; } = "green"; // green, yellow, red

    // Recent crop recommendations
    public List<RecommendationDto> RecentRecommendations { get; set; } = new();
}

public class WeatherInfoDto
{
    public double TemperatureC { get; set; }
    public string Condition { get; set; } = string.Empty; // e.g., "Sunny"
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public double RainProbability { get; set; }
}

public class RecommendationDto
{
    public int RecommendationId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string SoilType { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
}
