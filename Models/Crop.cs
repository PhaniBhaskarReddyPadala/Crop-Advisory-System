using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.Models;

public class Crop
{
    public int CropId { get; set; }

    [Required, MaxLength(100)]
    public string CropName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string SoilType { get; set; } = string.Empty; // "Black","Red","Sandy","Clay","Loamy","Any"

    public double PHMin { get; set; } = 5.5;
    public double PHMax { get; set; } = 8.0;

    [MaxLength(20)]
    public string Season { get; set; } = string.Empty; // "Kharif","Rabi","Zaid","Any"

    public bool NeedsIrrigation { get; set; } = false;

    [MaxLength(200)]
    public string ImageUrl { get; set; } = "/images/crops/default.jpg";

    [MaxLength(300)]
    public string SuitableConditions { get; set; } = string.Empty;

    [MaxLength(100)]
    public string GrowingPeriod { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<PestAdvisory> PestAdvisories { get; set; } = new List<PestAdvisory>();
    public ICollection<CropRecommendation> CropRecommendations { get; set; } = new List<CropRecommendation>();
}
