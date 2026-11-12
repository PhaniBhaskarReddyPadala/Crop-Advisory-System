using CropAdvisory.Data;
using CropAdvisory.Models;
using CropAdvisory.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CropAdvisory.Services;

public interface IPestService
{
    Task<List<PestAdvisory>> GetAllPestsAsync();
    Task<List<PestAdvisory>> GetPestsByCropAsync(int cropId);
    Task<PestAdvisory?> GetPestByIdAsync(int id);
    Task CreatePestAsync(PestAdvisory pest);
    Task UpdatePestAsync(PestAdvisory pest);
    Task DeletePestAsync(int id);
}

public class PestService : IPestService
{
    private readonly ApplicationDbContext _db;

    public PestService(ApplicationDbContext db) => _db = db;

    public async Task<List<PestAdvisory>> GetAllPestsAsync() =>
        await _db.PestAdvisories
            .Include(p => p.Crop)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PestName)
            .ToListAsync();

    public async Task<List<PestAdvisory>> GetPestsByCropAsync(int cropId) =>
        await _db.PestAdvisories
            .Where(p => p.IsActive && (p.CropId == cropId || p.CropId == null))
            .ToListAsync();

    public async Task<PestAdvisory?> GetPestByIdAsync(int id) =>
        await _db.PestAdvisories.Include(p => p.Crop).FirstOrDefaultAsync(p => p.PestId == id);

    public async Task CreatePestAsync(PestAdvisory pest)
    {
        _db.PestAdvisories.Add(pest);
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePestAsync(PestAdvisory pest)
    {
        _db.PestAdvisories.Update(pest);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePestAsync(int id)
    {
        var pest = await _db.PestAdvisories.FindAsync(id);
        if (pest != null)
        {
            pest.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
