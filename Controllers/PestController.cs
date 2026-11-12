using CropAdvisory.Data;
using CropAdvisory.Services;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CropAdvisory.Controllers;

[Authorize(Roles = "Farmer")]
public class PestController : Controller
{
    private readonly IPestService _pestService;
    private readonly ApplicationDbContext _db;

    public PestController(IPestService pestService, ApplicationDbContext db)
    {
        _pestService = pestService;
        _db = db;
    }

    public async Task<IActionResult> Index(int? cropId)
    {
        var allPests = cropId.HasValue
            ? await _pestService.GetPestsByCropAsync(cropId.Value)
            : await _pestService.GetAllPestsAsync();

        var allCrops = await _db.Crops.Where(c => c.IsActive).OrderBy(c => c.CropName).ToListAsync();

        var vm = new PestListViewModel
        {
            Pests = allPests,
            Crops = allCrops,
            SelectedCropId = cropId
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var pest = await _pestService.GetPestByIdAsync(id);
        if (pest == null) return NotFound();
        return View(pest);
    }
}
