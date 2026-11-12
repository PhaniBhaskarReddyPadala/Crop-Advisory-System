using CropAdvisory.Data;
using CropAdvisory.Models;
using CropAdvisory.Services;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CropAdvisory.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICropService _cropService;
    private readonly IPestService _pestService;

    public AdminController(ApplicationDbContext db, ICropService cropService, IPestService pestService)
    {
        _db = db;
        _cropService = cropService;
        _pestService = pestService;
    }

    // ── Dashboard ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var vm = new AdminStatsViewModel
        {
            TotalFarmers = await _db.Farmers.CountAsync(f => f.Role == "Farmer"),
            TotalCrops = await _db.Crops.CountAsync(c => c.IsActive),
            TotalPests = await _db.PestAdvisories.CountAsync(p => p.IsActive),
            TotalRecommendations = await _db.CropRecommendations.CountAsync(),
            TotalAdvisories = await _db.Advisories.CountAsync(a => a.IsActive),
            ActiveAdvisories = await _db.Advisories.Where(a => a.IsActive).OrderByDescending(a => a.CreatedAt).Take(5).ToListAsync(),
            RecentFarmers = await _db.Farmers.Where(f => f.Role == "Farmer").OrderByDescending(f => f.CreatedAt).Take(10).ToListAsync()
        };
        return View(vm);
    }

    // ── Crops ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Crops() => View(await _cropService.GetAllCropsAsync());

    public IActionResult CreateCrop() => View(new Crop());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCrop(Crop crop)
    {
        if (!ModelState.IsValid) return View(crop);
        await _cropService.CreateCropAsync(crop);
        TempData["Success"] = $"Crop '{crop.CropName}' created successfully!";
        return RedirectToAction("Crops");
    }

    public async Task<IActionResult> EditCrop(int id)
    {
        var crop = await _cropService.GetCropByIdAsync(id);
        if (crop == null) return NotFound();
        return View(crop);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCrop(Crop crop)
    {
        if (!ModelState.IsValid) return View(crop);
        await _cropService.UpdateCropAsync(crop);
        TempData["Success"] = $"Crop '{crop.CropName}' updated successfully!";
        return RedirectToAction("Crops");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCrop(int id)
    {
        await _cropService.DeleteCropAsync(id);
        TempData["Success"] = "Crop deleted successfully!";
        return RedirectToAction("Crops");
    }

    // ── Pests ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Pests() => View(await _pestService.GetAllPestsAsync());

    public async Task<IActionResult> CreatePest()
    {
        ViewBag.Crops = await _cropService.GetAllCropsAsync();
        return View(new PestAdvisory());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePest(PestAdvisory pest)
    {
        ModelState.Remove("Crop");
        if (!ModelState.IsValid)
        {
            ViewBag.Crops = await _cropService.GetAllCropsAsync();
            return View(pest);
        }
        await _pestService.CreatePestAsync(pest);
        TempData["Success"] = $"Pest advisory '{pest.PestName}' created!";
        return RedirectToAction("Pests");
    }

    public async Task<IActionResult> EditPest(int id)
    {
        var pest = await _pestService.GetPestByIdAsync(id);
        if (pest == null) return NotFound();
        ViewBag.Crops = await _cropService.GetAllCropsAsync();
        return View(pest);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPest(PestAdvisory pest)
    {
        ModelState.Remove("Crop");
        if (!ModelState.IsValid)
        {
            ViewBag.Crops = await _cropService.GetAllCropsAsync();
            return View(pest);
        }
        await _pestService.UpdatePestAsync(pest);
        TempData["Success"] = $"Pest advisory '{pest.PestName}' updated!";
        return RedirectToAction("Pests");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePest(int id)
    {
        await _pestService.DeletePestAsync(id);
        TempData["Success"] = "Pest advisory deleted!";
        return RedirectToAction("Pests");
    }

    // ── Farmers ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Farmers() =>
        View(await _db.Farmers.Where(f => f.Role == "Farmer").OrderByDescending(f => f.CreatedAt).ToListAsync());

    // ── Advisories ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Advisories() =>
        View(await _db.Advisories.OrderByDescending(a => a.CreatedAt).ToListAsync());

    public IActionResult CreateAdvisory() => View(new Advisory());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdvisory(Advisory advisory)
    {
        if (!ModelState.IsValid) return View(advisory);
        _db.Advisories.Add(advisory);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Advisory created!";
        return RedirectToAction("Advisories");
    }

    public async Task<IActionResult> EditAdvisory(int id)
    {
        var advisory = await _db.Advisories.FindAsync(id);
        if (advisory == null) return NotFound();
        return View(advisory);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdvisory(Advisory advisory)
    {
        if (!ModelState.IsValid) return View(advisory);
        _db.Advisories.Update(advisory);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Advisory updated!";
        return RedirectToAction("Advisories");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdvisory(int id)
    {
        var advisory = await _db.Advisories.FindAsync(id);
        if (advisory != null) { advisory.IsActive = false; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Advisory deleted!";
        return RedirectToAction("Advisories");
    }
}
