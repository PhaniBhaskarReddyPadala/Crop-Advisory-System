using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.ViewModels;

public class SoilPHStepViewModel
{
    [Range(0.0, 14.0)]
    public double PHValue { get; set; } = 7.0;
}

public class SeasonStepViewModel
{
    [Required]
    public string SelectedSeason { get; set; } = string.Empty;
}

public class IrrigationStepViewModel
{
    public bool IrrigationAvailable { get; set; }
}

public class CropResultDto
{
    public int CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string SoilType { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string GrowingPeriod { get; set; } = string.Empty;
    public string SuitableConditions { get; set; } = string.Empty;
    public int Score { get; set; }

    public string SuitabilityLabel => Score switch
    {
        >= 80 => "Excellent Match",
        >= 60 => "Good Match",
        >= 40 => "Fair Match",
        _ => "Possible Match"
    };

    public string SuitabilityColor => Score switch
    {
        >= 80 => "success",
        >= 60 => "primary",
        >= 40 => "warning",
        _ => "secondary"
    };
}

public class RecommendationResultViewModel
{
    public string SelectedSoilType { get; set; } = string.Empty;
    public double SelectedPH { get; set; }
    public string SelectedSeason { get; set; } = string.Empty;
    public bool IrrigationAvailable { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public List<CropResultDto> MatchingCrops { get; set; } = new();
}
