using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CropAdvisory.Models;

public class PestAdvisory
{
    [Key]
    public int PestId { get; set; }

    public int? CropId { get; set; }  // null = general advisory

    [Required, MaxLength(100)]
    public string PestName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Symptoms { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PreventiveMeasures { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Severity { get; set; } = "Medium"; // Low, Medium, High

    [MaxLength(200)]
    public string ImageUrl { get; set; } = "/images/pests/default.jpg";

    public bool IsActive { get; set; } = true;

    [ForeignKey("CropId")]
    public Crop? Crop { get; set; }
}
