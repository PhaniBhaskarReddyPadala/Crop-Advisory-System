using CropAdvisory.Models;
using CropAdvisory.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CropAdvisory.Data;

namespace CropAdvisory.Services;

public record GeoResult(double Latitude, double Longitude, string Name);

public interface IWeatherService
{
    Task<GeoResult?> GeocodeAsync(string location);
    Task<WeatherInfoDto> GetCurrentWeatherAsync(string location);
    Task<List<DailyForecastDto>> GetForecastAsync(string location);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient http, IConfiguration config, ILogger<WeatherService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    // Geocoding works from Render (Open-Meteo only rate-limits the forecast host there),
    // so this stays on the server. Tries India first, then worldwide.
    public async Task<GeoResult?> GeocodeAsync(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;
        location = location.Trim();

        foreach (var country in new[] { "&countryCode=IN", "" })
        {
            try
            {
                var geoUrl = $"{_config["OpenMeteo:GeocodingUrl"]}?name={Uri.EscapeDataString(location)}&count=1&language=en&format=json{country}";
                var resp = await _http.GetStringAsync(geoUrl);
                using var doc = JsonDocument.Parse(resp);
                if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                    continue;
                var first = results[0];
                var name = first.TryGetProperty("name", out var n) ? n.GetString() ?? location : location;
                return new GeoResult(first.GetProperty("latitude").GetDouble(),
                                     first.GetProperty("longitude").GetDouble(), name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding failed for {Location}", location);
            }
        }
        return null;
    }

    private async Task<(double lat, double lon)?> GeolocateAsync(string location)
    {
        var g = await GeocodeAsync(location);
        return g == null ? null : (g.Latitude, g.Longitude);
    }

    public async Task<WeatherInfoDto> GetCurrentWeatherAsync(string location)
    {
        try
        {
            var coords = await GeolocateAsync(location) ?? (17.3850, 78.4867); // Default: Hyderabad
            var url = $"{_config["OpenMeteo:ForecastUrl"]}?latitude={coords.lat}&longitude={coords.lon}" +
                      "&current=temperature_2m,relative_humidity_2m,wind_speed_10m,precipitation_probability,weathercode" +
                      "&timezone=auto";

            var resp = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(resp);
            var current = doc.RootElement.GetProperty("current");

            return new WeatherInfoDto
            {
                TemperatureC = current.GetProperty("temperature_2m").GetDouble(),
                Humidity = current.GetProperty("relative_humidity_2m").GetDouble(),
                WindSpeed = current.GetProperty("wind_speed_10m").GetDouble(),
                RainProbability = current.TryGetProperty("precipitation_probability", out var pp) ? pp.GetDouble() : 0,
                Condition = WeatherCodeToCondition(current.GetProperty("weathercode").GetInt32())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Current weather request failed for {Location}", location);
            return new WeatherInfoDto { TemperatureC = 28, Condition = "Sunny", Humidity = 50, WindSpeed = 10, RainProbability = 10 };
        }
    }

    public async Task<List<DailyForecastDto>> GetForecastAsync(string location)
    {
        try
        {
            var coords = await GeolocateAsync(location) ?? (17.3850, 78.4867);
            var url = $"{_config["OpenMeteo:ForecastUrl"]}?latitude={coords.lat}&longitude={coords.lon}" +
                      "&daily=weathercode,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                      "&timezone=auto&forecast_days=7";

            var resp = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(resp);
            var daily = doc.RootElement.GetProperty("daily");

            var dates = daily.GetProperty("time").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
            var codes = daily.GetProperty("weathercode").EnumerateArray().Select(e => e.GetInt32()).ToList();
            var maxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(e => e.GetDouble()).ToList();
            var minTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(e => e.GetDouble()).ToList();
            var rainProbs = daily.GetProperty("precipitation_probability_max").EnumerateArray().Select(e => e.GetDouble()).ToList();

            return dates.Select((d, i) => new DailyForecastDto
            {
                Date = d,
                WeatherCode = codes[i],
                MaxTempC = maxTemps[i],
                MinTempC = minTemps[i],
                RainProbability = rainProbs[i]
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weather forecast request failed for {Location}", location);
            return new List<DailyForecastDto>();
        }
    }

    private static string WeatherCodeToCondition(int code) => code switch
    {
        0 => "Clear Sky",
        1 or 2 or 3 => "Partly Cloudy",
        45 or 48 => "Foggy",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rainy",
        71 or 73 or 75 => "Snowy",
        80 or 81 or 82 => "Rain Showers",
        95 => "Thunderstorm",
        _ => "Variable"
    };
}
