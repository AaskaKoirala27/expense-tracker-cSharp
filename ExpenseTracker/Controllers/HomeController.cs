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
public class HomeController : Controller
{
    private readonly HomeSummaryService _summaryService;

    public HomeController(HomeSummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = GetUserIdFromSessionOrClaims();

        if (!isAdmin && userId == null)
        {
            return RedirectToAction("Login", "Account");
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
