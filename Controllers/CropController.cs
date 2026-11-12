using CropAdvisory.Data;
using CropAdvisory.Models;
using CropAdvisory.Services;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CropAdvisory.Controllers;

[Authorize(Roles = "Farmer")]
public class CropController : Controller
{
    private readonly ICropService _cropService;
    private readonly IRecommendationService _recService;
    private readonly ApplicationDbContext _db;

    public CropController(ICropService cropService, IRecommendationService recService, ApplicationDbContext db)
    {
        _cropService = cropService;
        _recService = recService;
        _db = db;
    }

    // Step 1 – Soil Type
    [HttpGet] public IActionResult SoilType() => View("Step1SoilType", new SoilTypeStepViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SoilType(SoilTypeStepViewModel model)
    {
        if (!ModelState.IsValid) return View("Step1SoilType", model);
        TempData["SoilType"] = model.SelectedSoilType;
        TempData.Keep("SoilType");
        return RedirectToAction("SoilPH");
    }

    // Step 2 – Soil pH
    [HttpGet] public IActionResult SoilPH() => View("Step2SoilPH", new SoilPHStepViewModel { PHValue = 7.0 });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SoilPH(SoilPHStepViewModel model)
    {
        if (!ModelState.IsValid) return View("Step2SoilPH", model);
        TempData["SoilType"] = TempData.Peek("SoilType");
        TempData["SoilPH"] = model.PHValue.ToString();
        TempData.Keep();
        return RedirectToAction("Season");
    }

    // Step 3 – Season
    [HttpGet] public IActionResult Season() => View("Step3Season", new SeasonStepViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Season(SeasonStepViewModel model)
    {
        if (!ModelState.IsValid) return View("Step3Season", model);
        TempData.Keep();
        TempData["Season"] = model.SelectedSeason;
        return RedirectToAction("Irrigation");
    }

    // Step 4 – Irrigation
    [HttpGet] public IActionResult Irrigation() => View("Step4Irrigation", new IrrigationStepViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Irrigation(IrrigationStepViewModel model)
    {
        if (!ModelState.IsValid) return View("Step4Irrigation", model);

        var soilType = TempData["SoilType"] as string ?? "Loamy";
        var phStr = TempData["SoilPH"] as string ?? "7.0";
        var season = TempData["Season"] as string ?? "Kharif";
        var ph = double.TryParse(phStr, out var phVal) ? phVal : 7.0;

        var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var farmer = await _db.Farmers.FindAsync(farmerId);

        var matches = await _cropService.GetRecommendationsAsync(soilType, ph, season, model.IrrigationAvailable);

        // Save recommendation history
        foreach (var crop in matches)
        {
            _db.CropRecommendations.Add(new CropRecommendation
            {
                FarmerId = farmerId,
                CropId = crop.CropId,
                SoilType = soilType,
                SoilPH = ph,
                Season = season,
                IrrigationAvailable = model.IrrigationAvailable,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();

        var vm = new RecommendationResultViewModel
        {
            SelectedSoilType = soilType,
            SelectedPH = ph,
            SelectedSeason = season,
            IrrigationAvailable = model.IrrigationAvailable,
            FarmerName = farmer?.Name ?? "",
            MatchingCrops = matches
        };
        return View("Result", vm);
    }

    // History
    public async Task<IActionResult> History()
    {
        var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var recs = await _recService.GetRecentRecommendationsAsync(farmerId);
        return View(recs);
    }
}
