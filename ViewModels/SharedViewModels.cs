namespace CropAdvisory.ViewModels;

public class WeatherPageViewModel
{
    public string Location { get; set; } = string.Empty;
    public bool LocationFound { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class PestListViewModel
{
    public List<CropAdvisory.Models.PestAdvisory> Pests { get; set; } = new();
    public List<CropAdvisory.Models.Crop> Crops { get; set; } = new();
    public int? SelectedCropId { get; set; }
}

public class AdminStatsViewModel
{
    public int TotalFarmers { get; set; }
    public int TotalCrops { get; set; }
    public int TotalPests { get; set; }
    public int TotalRecommendations { get; set; }
    public int TotalAdvisories { get; set; }
    public List<CropAdvisory.Models.Advisory> ActiveAdvisories { get; set; } = new();
    public List<CropAdvisory.Models.Farmer> RecentFarmers { get; set; } = new();
}
