using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CropAdvisory.ViewModels;

public class DailyForecastDto
{
    // Date in ISO (yyyy-MM-dd)
    public string Date { get; set; } = string.Empty;

    public double MaxTempC { get; set; }
    public double MinTempC { get; set; }
    public double RainProbability { get; set; }
    public int WeatherCode { get; set; }
    public string Condition => WeatherCodeToDescription(WeatherCode);

    private static string WeatherCodeToDescription(int code) => code switch
    {
        0 => "Clear",
        1 or 2 or 3 => "Partly cloudy",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        80 or 81 or 82 => "Rain showers",
        95 => "Thunderstorm",
        _ => "Variable"
    };
}
