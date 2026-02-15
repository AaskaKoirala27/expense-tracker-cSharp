using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers
{
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
