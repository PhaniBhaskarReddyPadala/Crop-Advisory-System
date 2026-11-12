using CropAdvisory.Data;
using CropAdvisory.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CropAdvisory.Services;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetRecentRecommendationsAsync(int farmerId);
}

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _db;

    public RecommendationService(ApplicationDbContext db) => _db = db;

    public async Task<List<RecommendationDto>> GetRecentRecommendationsAsync(int farmerId) =>
        await _db.CropRecommendations
            .Include(r => r.Crop)
            .Where(r => r.FarmerId == farmerId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecommendationDto
            {
                RecommendationId = r.RecommendationId,
                CropName = r.Crop != null ? r.Crop.CropName : "Unknown",
                Date = r.CreatedAt,
                SoilType = r.SoilType,
                Season = r.Season
            })
            .ToListAsync();
}
