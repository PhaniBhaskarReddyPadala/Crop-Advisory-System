using CropAdvisory.Data;
using CropAdvisory.Models;
using CropAdvisory.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CropAdvisory.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db) => _db = db;

    // GET: /Account/Login
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var farmer = _db.Farmers.FirstOrDefault(f => f.Email == model.Email);
        if (farmer == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var hasher = new PasswordHasher<Farmer>();
        var result = hasher.VerifyHashedPassword(farmer, farmer.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, farmer.FarmerId.ToString()),
            new(ClaimTypes.Name, farmer.Name),
            new(ClaimTypes.Email, farmer.Email),
            new(ClaimTypes.Role, farmer.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return farmer.Role == "Admin"
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Dashboard");
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (_db.Farmers.Any(f => f.Email == model.Email))
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        var farmer = new Farmer
        {
            Name = model.Name,
            Email = model.Email,
            Location = model.Location,
            PreferredLanguage = model.PreferredLanguage,
            Role = "Farmer",
            CreatedAt = DateTime.UtcNow
        };
        var hasher = new PasswordHasher<Farmer>();
        farmer.PasswordHash = hasher.HashPassword(farmer, model.Password);

        _db.Farmers.Add(farmer);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Registration successful! Please log in.";
        return RedirectToAction("Login");
    }

    // POST: /Account/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    // POST: /Account/SetLanguage
    [HttpPost]
    public IActionResult SetLanguage(string lang, string returnUrl = "/")
    {
        Response.Cookies.Append("CropLang", lang, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
        return Redirect(returnUrl);
    }
}
