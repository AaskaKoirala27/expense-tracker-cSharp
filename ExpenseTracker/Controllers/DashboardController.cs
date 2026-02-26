/*
 * File: Controllers/DashboardController.cs
 * Purpose: Provides the user dashboard view which aggregates key metrics for the logged-in user.
 * Responsibilities:
 *  - Determine the current user identity and role (Admin vs regular user)
 *  - Retrieve the dashboard view model via `DashboardService`
 *  - Ensure only authenticated users can access the dashboard
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers
{
    /// <summary>
    /// Dashboard controller that returns personalized or admin-level dashboard information.
    /// Primary action: `Index` which returns a `DashboardViewModel` produced by `DashboardService`.
    /// </summary>
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await _dashboardService.GetDashboardAsync(userId, isAdmin);
            return View(viewModel);
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
    }
}
