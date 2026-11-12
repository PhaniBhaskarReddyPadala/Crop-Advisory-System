using System;
using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.Models;

public class Farmer
{
    public int FarmerId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(5)]
    public string PreferredLanguage { get; set; } = "en";

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Farmer"; // "Farmer" or "Admin"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
