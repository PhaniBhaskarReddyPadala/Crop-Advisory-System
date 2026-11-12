using CropAdvisory.Data;
using CropAdvisory.Models;
using CropAdvisory.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CropAdvisory.Services;

public interface ICropService
{
    Task<List<CropResultDto>> GetRecommendationsAsync(string soilType, double ph, string season, bool irrigation);
    Task<List<Crop>> GetAllCropsAsync();
    Task<Crop?> GetCropByIdAsync(int id);
    Task CreateCropAsync(Crop crop);
    Task UpdateCropAsync(Crop crop);
    Task DeleteCropAsync(int id);
}

public class CropService : ICropService
{
    private readonly ApplicationDbContext _db;

    public CropService(ApplicationDbContext db) => _db = db;

    public async Task<List<CropResultDto>> GetRecommendationsAsync(string soilType, double ph, string season, bool irrigation)
    {
        var allCrops = await _db.Crops.Where(c => c.IsActive).ToListAsync();
        var results = new List<(Crop crop, int score)>();

        foreach (var crop in allCrops)
        {
            int score = 0;

            // Soil type match
            if (crop.SoilType == soilType) score += 40;
            else if (crop.SoilType == "Any") score += 20;
            else continue; // Skip if soil doesn't match at all

            // pH range match
            if (ph >= crop.PHMin && ph <= crop.PHMax) score += 30;
            else if (Math.Abs(ph - crop.PHMin) <= 0.5 || Math.Abs(ph - crop.PHMax) <= 0.5) score += 15;
            else continue;

            // Season match
            if (crop.Season == season) score += 20;
            else if (crop.Season == "Any") score += 10;
            else continue;

            // Irrigation match
            if (crop.NeedsIrrigation == irrigation) score += 10;
            else if (!crop.NeedsIrrigation && irrigation) score += 5; // irrigation available but not needed — still OK

            results.Add((crop, score));
        }

        return results
            .OrderByDescending(r => r.score)
            .Take(5)
            .Select(r => new CropResultDto
            {
                CropId = r.crop.CropId,
                CropName = r.crop.CropName,
                Description = r.crop.Description,
                ImageUrl = r.crop.ImageUrl,
                SoilType = r.crop.SoilType,
                Season = r.crop.Season,
                GrowingPeriod = r.crop.GrowingPeriod,
                SuitableConditions = r.crop.SuitableConditions,
                Score = r.score
            })
            .ToList();
    }

    public async Task<List<Crop>> GetAllCropsAsync() =>
        await _db.Crops.OrderBy(c => c.CropName).ToListAsync();

    public async Task<Crop?> GetCropByIdAsync(int id) =>
        await _db.Crops.FindAsync(id);

    public async Task CreateCropAsync(Crop crop)
    {
        _db.Crops.Add(crop);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCropAsync(Crop crop)
    {
        _db.Crops.Update(crop);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCropAsync(int id)
    {
        var crop = await _db.Crops.FindAsync(id);
        if (crop != null)
        {
            crop.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
