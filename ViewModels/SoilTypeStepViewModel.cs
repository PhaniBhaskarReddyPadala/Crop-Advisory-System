using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.ViewModels;

public class SoilTypeStepViewModel
{
    [Required]
    [Display(Name = "Soil Type")]
    public string SelectedSoilType { get; set; } = string.Empty;

    // For UI we provide a list of options (hard‑coded for now)
    public static readonly string[] SoilOptions = new[] { "Black", "Red", "Sandy", "Clay", "Loamy" };
}