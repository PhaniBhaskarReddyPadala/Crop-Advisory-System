using System;
using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.Models;

public class Advisory
{
    public int AdvisoryId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Level { get; set; } = "green"; // green, yellow, red

    [MaxLength(50)]
    public string Category { get; set; } = "General"; // General, Pest, Weather, Soil

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
