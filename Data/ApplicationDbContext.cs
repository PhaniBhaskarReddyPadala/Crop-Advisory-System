using Microsoft.EntityFrameworkCore;
using CropAdvisory.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CropAdvisory.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<PestAdvisory> PestAdvisories => Set<PestAdvisory>();
    public DbSet<CropRecommendation> CropRecommendations => Set<CropRecommendation>();
    public DbSet<Advisory> Advisories => Set<Advisory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Farmer>().HasKey(f => f.FarmerId);
        modelBuilder.Entity<Farmer>().HasIndex(f => f.Email).IsUnique();

        modelBuilder.Entity<Crop>().HasKey(c => c.CropId);
        modelBuilder.Entity<Advisory>().HasKey(a => a.AdvisoryId);
        modelBuilder.Entity<PestAdvisory>().HasKey(p => p.PestId);
        modelBuilder.Entity<CropRecommendation>().HasKey(r => r.RecommendationId);

        modelBuilder.Entity<CropRecommendation>()
            .HasOne(r => r.Farmer)
            .WithMany()
            .HasForeignKey(r => r.FarmerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CropRecommendation>()
            .HasOne(r => r.Crop)
            .WithMany(c => c.CropRecommendations)
            .HasForeignKey(r => r.CropId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PestAdvisory>()
            .HasOne(p => p.Crop)
            .WithMany(c => c.PestAdvisories)
            .HasForeignKey(p => p.CropId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public static class DbInitializer
{
    public static void Initialize(
        ApplicationDbContext db,
        string? initialAdminEmail,
        string? initialAdminPassword,
        ILogger logger)
    {
        db.Database.Migrate();

        var initialAdminCreated = false;
        if (!db.Farmers.Any(f => f.Role == "Admin"))
        {
            var email = !string.IsNullOrWhiteSpace(initialAdminEmail)
                ? initialAdminEmail.Trim()
                : "admin@cropadvisory.com";

            var password = !string.IsNullOrWhiteSpace(initialAdminPassword)
                ? initialAdminPassword
                : "Admin@12345678";

            var admin = new Farmer
            {
                Name = "Admin",
                Email = email,
                Role = "Admin",
                Location = "Hyderabad",
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = new PasswordHasher<Farmer>().HashPassword(admin, password);
            db.Farmers.Add(admin);
            initialAdminCreated = true;
        }

        // Seed sample crops if none exist
        if (!db.Crops.Any())
        {
            db.Crops.AddRange(
                new Crop
                {
                    CropName = "Rice",
                    Description = "Rice is a staple food crop that grows well in flooded conditions with high water availability.",
                    SoilType = "Clay",
                    PHMin = 5.5,
                    PHMax = 7.0,
                    Season = "Kharif",
                    NeedsIrrigation = true,
                    ImageUrl = "/images/crops/rice.jpg",
                    SuitableConditions = "High rainfall, flooded fields, warm climate",
                    GrowingPeriod = "120-150 days"
                },
                new Crop
                {
                    CropName = "Wheat",
                    Description = "Wheat is a cool season crop best grown during Rabi season with moderate irrigation.",
                    SoilType = "Loamy",
                    PHMin = 6.0,
                    PHMax = 7.5,
                    Season = "Rabi",
                    NeedsIrrigation = true,
                    ImageUrl = "/images/crops/wheat.jpg",
                    SuitableConditions = "Cool weather, moderate rainfall, well-drained soil",
                    GrowingPeriod = "120-130 days"
                },
                new Crop
                {
                    CropName = "Cotton",
                    Description = "Cotton thrives in black soil with good drainage and warm temperatures.",
                    SoilType = "Black",
                    PHMin = 6.0,
                    PHMax = 8.0,
                    Season = "Kharif",
                    NeedsIrrigation = false,
                    ImageUrl = "/images/crops/cotton.jpg",
                    SuitableConditions = "Black soil, warm climate, moderate rainfall",
                    GrowingPeriod = "150-180 days"
                },
                new Crop
                {
                    CropName = "Groundnut",
                    Description = "Groundnut grows well in sandy loam soil with good drainage and warm conditions.",
                    SoilType = "Sandy",
                    PHMin = 6.0,
                    PHMax = 7.0,
                    Season = "Kharif",
                    NeedsIrrigation = false,
                    ImageUrl = "/images/crops/groundnut.jpg",
                    SuitableConditions = "Sandy loam soil, warm climate, moderate rainfall",
                    GrowingPeriod = "90-120 days"
                },
                new Crop
                {
                    CropName = "Tomato",
                    Description = "Tomato is a warm-season crop that grows well in Zaid season with irrigation.",
                    SoilType = "Loamy",
                    PHMin = 6.0,
                    PHMax = 7.0,
                    Season = "Zaid",
                    NeedsIrrigation = true,
                    ImageUrl = "/images/crops/tomato.jpg",
                    SuitableConditions = "Loamy soil, warm temperatures, regular watering",
                    GrowingPeriod = "60-90 days"
                },
                new Crop
                {
                    CropName = "Sorghum (Jowar)",
                    Description = "Sorghum is a drought-resistant crop ideal for red and black soil with low water needs.",
                    SoilType = "Red",
                    PHMin = 5.5,
                    PHMax = 8.5,
                    Season = "Kharif",
                    NeedsIrrigation = false,
                    ImageUrl = "/images/crops/sorghum.jpg",
                    SuitableConditions = "Red or black soil, drought tolerant, warm climate",
                    GrowingPeriod = "90-120 days"
                },
                new Crop
                {
                    CropName = "Mustard",
                    Description = "Mustard is a cool season Rabi crop that grows well in loamy and clay soils.",
                    SoilType = "Loamy",
                    PHMin = 6.0,
                    PHMax = 7.5,
                    Season = "Rabi",
                    NeedsIrrigation = false,
                    ImageUrl = "/images/crops/mustard.jpg",
                    SuitableConditions = "Cool dry weather, well-drained soil",
                    GrowingPeriod = "90-120 days"
                },
                new Crop
                {
                    CropName = "Sugarcane",
                    Description = "Sugarcane requires heavy irrigation and grows best in tropical climates year-round.",
                    SoilType = "Loamy",
                    PHMin = 6.0,
                    PHMax = 7.5,
                    Season = "Zaid",
                    NeedsIrrigation = true,
                    ImageUrl = "/images/crops/sugarcane.jpg",
                    SuitableConditions = "Tropical climate, heavy irrigation, deep loamy soil",
                    GrowingPeriod = "10-18 months"
                },
                new Crop
                {
                    CropName = "Chickpea (Bengal Gram)",
                    Description = "Chickpea is a legume that grows in cool dry conditions on black and loamy soils.",
                    SoilType = "Black",
                    PHMin = 6.0,
                    PHMax = 8.0,
                    Season = "Rabi",
                    NeedsIrrigation = false,
                    ImageUrl = "/images/crops/chickpea.jpg",
                    SuitableConditions = "Cool dry weather, black or clay soil",
                    GrowingPeriod = "90-110 days"
                },
                new Crop
                {
                    CropName = "Maize (Corn)",
                    Description = "Maize is a versatile crop that adapts to multiple seasons with moderate water needs.",
                    SoilType = "Loamy",
                    PHMin = 5.5,
                    PHMax = 7.5,
                    Season = "Kharif",
                    NeedsIrrigation = true,
                    ImageUrl = "/images/crops/maize.jpg",
                    SuitableConditions = "Well-drained loamy soil, moderate rainfall",
                    GrowingPeriod = "80-110 days"
                }
            );
        }

        // Seed Pest Advisories if none exist
        if (!db.PestAdvisories.Any())
        {
            db.PestAdvisories.AddRange(
                new PestAdvisory
                {
                    PestName = "Rice Stem Borer",
                    Symptoms = "Dead hearts in young plants, white ears at heading stage, entry holes on stem",
                    PreventiveMeasures = "Use resistant varieties, timely planting, avoid excess nitrogen fertilizer",
                    Treatment = "Apply Chlorpyrifos or Cartap Hydrochloride. Remove and destroy affected plants.",
                    Severity = "High",
                    ImageUrl = "/images/pests/stem_borer.jpg"
                },
                new PestAdvisory
                {
                    PestName = "Cotton Bollworm",
                    Symptoms = "Holes in bolls, shedding of flower buds, larvae inside bolls",
                    PreventiveMeasures = "Use Bt cotton varieties, install pheromone traps, timely sowing",
                    Treatment = "Spray Spinosad or Emamectin benzoate. Use light traps to monitor adult moths.",
                    Severity = "High",
                    ImageUrl = "/images/pests/bollworm.jpg"
                },
                new PestAdvisory
                {
                    PestName = "Aphids",
                    Symptoms = "Yellowing and curling of leaves, sticky honeydew on plant surface, sooty mold",
                    PreventiveMeasures = "Encourage natural predators, avoid over-fertilizing with nitrogen",
                    Treatment = "Spray Imidacloprid or Dimethoate. Wash plants with water jet for mild infestations.",
                    Severity = "Medium",
                    ImageUrl = "/images/pests/aphids.jpg"
                },
                new PestAdvisory
                {
                    PestName = "Whitefly",
                    Symptoms = "White powdery insects on leaf undersides, yellowing leaves, stunted growth",
                    PreventiveMeasures = "Use yellow sticky traps, avoid planting near infested fields",
                    Treatment = "Apply Thiamethoxam or Acetamiprid. Remove heavily infested leaves.",
                    Severity = "Medium",
                    ImageUrl = "/images/pests/whitefly.jpg"
                },
                new PestAdvisory
                {
                    PestName = "Powdery Mildew",
                    Symptoms = "White powdery coating on leaves and stems, stunted growth, leaf drop",
                    PreventiveMeasures = "Plant resistant varieties, ensure good air circulation, avoid overhead watering",
                    Treatment = "Spray Sulphur-based fungicide or Propiconazole. Remove infected plant parts.",
                    Severity = "Medium",
                    ImageUrl = "/images/pests/powdery_mildew.jpg"
                },
                new PestAdvisory
                {
                    PestName = "Locust Attack",
                    Symptoms = "Rapid defoliation, swarms of insects, complete crop destruction possible",
                    PreventiveMeasures = "Monitor locust forecasts, coordinate with district agricultural office",
                    Treatment = "Contact district agricultural authorities immediately. Use Malathion sprays as directed.",
                    Severity = "High",
                    ImageUrl = "/images/pests/locust.jpg"
                }
            );
        }

        // Seed Advisories
        if (!db.Advisories.Any())
        {
            db.Advisories.AddRange(
                new Advisory
                {
                    Title = "Pre-Kharif Soil Preparation",
                    Content = "Plow fields to a depth of 20-25 cm before monsoon. Add farmyard manure for better soil health.",
                    Level = "green",
                    Category = "General"
                },
                new Advisory
                {
                    Title = "Monsoon Alert",
                    Content = "Heavy rainfall expected this week. Ensure proper field drainage to prevent waterlogging.",
                    Level = "yellow",
                    Category = "Weather"
                }
            );
        }

        db.SaveChanges();

        if (initialAdminCreated)
            logger.LogInformation("Initial admin account was provisioned from configured credentials.");
    }
}
