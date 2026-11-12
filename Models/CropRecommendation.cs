using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CropAdvisory.Models;

public class CropRecommendation
{
    [Key]
    public int RecommendationId { get; set; }

    public int FarmerId { get; set; }

    public int CropId { get; set; }

    [MaxLength(50)]
    public string SoilType { get; set; } = string.Empty;

    public double SoilPH { get; set; }

    [MaxLength(20)]
    public string Season { get; set; } = string.Empty;

    public bool IrrigationAvailable { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("FarmerId")]
    public Farmer? Farmer { get; set; }

    [ForeignKey("CropId")]
    public Crop? Crop { get; set; }
}
