/*
 * File: Controllers/HomeController.cs
 * Purpose: Main landing controller for the application providing the home index and error/privacy pages.
 * Responsibilities:
 *  - Render `Index` with a `HomeSummary` for regular users or superadmin overview when applicable
 *  - Provide anonymous-accessible pages such as `Privacy` and error handling
 *  - Ensure authenticated access for primary `Index` route
 */

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]

/// Handles the application's home page and related public pages.
/// Main action: `Index` which selects the appropriate view model depending on role (superadmin/admin/user).

public class HomeController : Controller
{
    private readonly HomeSummaryService _summaryService;
    private readonly SuperAdminService _superAdminService;

    public HomeController(HomeSummaryService summaryService, SuperAdminService superAdminService)
    {
        _summaryService = summaryService;
        _superAdminService = superAdminService;
    }

    public async Task<IActionResult> Index()
    {
        var isSuperAdmin = User.Identity?.Name == "superadmin";
        var isAdmin = User.IsInRole("Admin");
        var userId = GetUserIdFromSessionOrClaims();

        if (!isSuperAdmin && !isAdmin && userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (isSuperAdmin)
        {
            var superAdminData = await _superAdminService.GetDashboardDataAsync();
            return View("Index", superAdminData);
        }

        var model = await _summaryService.GetSummaryAsync(userId, isAdmin);
        return View(model);
    }

    private int? GetUserIdFromSessionOrClaims()
    {
        var sessionUserId = HttpContext.Session.GetInt32("UserId");
        if (sessionUserId.HasValue)
        {
            return sessionUserId;
        }

        var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claimUserId, out var parsedUserId))
        {
            HttpContext.Session.SetInt32("UserId", parsedUserId);
            return parsedUserId;
        }

        return null;
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
