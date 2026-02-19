using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    /// <summary>
    /// Controller responsible for handling all expense-related operations.
    /// Implements full CRUD (Create, Read, Update, Delete) functionality for expense management.
    /// </summary>
    [Authorize(Policy = "UserOrSuperAdmin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ExpenseController : Controller
    {
        // Database context for accessing and manipulating expense data
        private readonly AppDbContext _context;
        private readonly ExpenseGraphService _graphService;

        /// <summary>
        /// Constructor that initializes the controller with the database context.
        /// The context is injected via dependency injection configured in Program.cs.
        /// </summary>
        /// <param name="context">Database context for expense operations</param>
        /// <param name="graphService">Service for generating expense graph data</param>
        public ExpenseController(AppDbContext context, ExpenseGraphService graphService)
        {
            _context = context;
            _graphService = graphService;
        }

        /// <summary>
        /// Displays the main dashboard showing all expenses.
        /// GET: Expense/Index
        /// </summary>
        /// <returns>View with a list of all expenses including summary information</returns>
        public async Task<IActionResult> Index()
        {
            var isSuperAdmin = string.Equals(User.Identity?.Name, "superadmin", StringComparison.OrdinalIgnoreCase);
            var userId = GetUserIdFromSessionOrClaims();

            if (!isSuperAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var expensesQuery = _context.Expenses.AsNoTracking().AsQueryable();

            if (!isSuperAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var expenses = await expensesQuery.ToListAsync();
            return View(expenses);
        }

        /// <summary>
        /// Displays an expense graph with custom date filtering.
        /// For regular users: shows their personal expenses
        /// For superadmin: shows system-wide expenses
        /// GET: Expense/Graph
        /// </summary>
        /// <param name="startDate">Filter start date (optional)</param>
        /// <param name="endDate">Filter end date (optional)</param>
        /// <returns>View with expense graph data</returns>
        public async Task<IActionResult> Graph(DateTime? startDate, DateTime? endDate)
        {
            var isSuperAdmin = string.Equals(User.Identity?.Name, "superadmin", StringComparison.OrdinalIgnoreCase);
            var userId = GetUserIdFromSessionOrClaims();

            // Allow access to superadmin or logged-in users
            if (!isSuperAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate dates - prevent future dates
            if (startDate.HasValue && startDate.Value > DateTime.Now)
            {
                ModelState.AddModelError("startDate", "Start date cannot be in the future.");
                startDate = null;
            }

            if (endDate.HasValue && endDate.Value > DateTime.Now)
            {
                ModelState.AddModelError("endDate", "End date cannot be in the future.");
                endDate = null;
            }

            // Ensure start date is before end date
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                ModelState.AddModelError("startDate", "Start date must be before end date.");
            }

            // Get graph data based on user type
            ExpenseGraphViewModel graphData;
            if (isSuperAdmin)
            {
                graphData = await _graphService.GetSystemExpenseGraphDataAsync(startDate, endDate);
            }
            else
            {
                graphData = await _graphService.GetExpenseGraphDataAsync(userId.Value, startDate, endDate);
            }

            return View(graphData);
        }

        /// <summary>
        /// Displays detailed information for a specific expense.
        /// GET: Expense/Details/5
        /// </summary>
        /// <param name="id">The ID of the expense to display</param>
        /// <returns>View with complete expense details or NotFound if expense doesn't exist</returns>
        public async Task<IActionResult> Details(int? id)
        {
            // Check if ID was provided in the request
            if (id == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var expensesQuery = _context.Expenses
                .Include(e => e.User)
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var expense = await expensesQuery.FirstOrDefaultAsync(m => m.Id == id);
            
            // Return NotFound if no matching expense is found
            if (expense == null)
            {
                return NotFound();
            }

            // Pass the expense to the view for display
            return View(expense);
        }

        /// <summary>
        /// Displays the form for creating a new expense.
        /// GET: Expense/Create
        /// </summary>
        /// <returns>View with an empty form for entering expense details</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes the submitted form data to create a new expense.
        /// POST: Expense/Create
        /// </summary>
        /// <param name="expense">The expense object populated from the form data</param>
        /// <returns>Redirects to Index if successful, or returns to the form with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]  // Prevents Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Category,Date,Notes")] Expense expense)
        {
            var userId = GetUserIdFromSessionOrClaims();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            expense.UserId = userId;
            expense.CreatedAt = DateTime.Now;

            // Remove User navigation property from ModelState since we're setting UserId manually
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            // Check if all validation requirements are met
            if (ModelState.IsValid)
            {
                try
                {
                    // Add the new expense to the context
                    _context.Add(expense);
                    // Save changes to the database asynchronously
                    await _context.SaveChangesAsync();
                    // Set success message
                    TempData["SuccessMessage"] = $"Expense '{expense.Description}' created successfully!";
                    // Redirect to the Index page to show the updated list
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    // Handle database errors
                    ModelState.AddModelError("", "An error occurred while saving the expense. Please try again.");
                    return View(expense);
                }
            }
            // If validation failed, return to the form with the entered data and error messages
            return View(expense);
        }

        /// <summary>
        /// Displays the form for editing an existing expense.
        /// GET: Expense/Edit/5
        /// </summary>
        /// <param name="id">The ID of the expense to edit</param>
        /// <returns>View with the expense form pre-populated with existing data</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var expensesQuery = _context.Expenses
                .Include(e => e.User)
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var expense = await expensesQuery.FirstOrDefaultAsync(e => e.Id == id);
            if (expense == null)
            {
                return NotFound();
            }
            // Return the view with the expense data for editing
            return View(expense);
        }

        /// <summary>
        /// Processes the submitted form data to update an existing expense.
        /// POST: Expense/Edit/5
        /// </summary>
        /// <param name="id">The ID of the expense being updated</param>
        /// <param name="expense">The expense object with updated values from the form</param>
        /// <returns>Redirects to Index if successful, or returns to the form with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]  // Prevents Cross-Site Request Forgery attacks
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Category,Date,Notes,CreatedAt")] Expense expense)
        {
            // Verify that the ID in the URL matches the expense ID in the form
            if (id != expense.Id)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Remove User navigation property from ModelState since we don't bind it
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            // Check if all validation requirements are met
            if (ModelState.IsValid)
            {
                try
                {
                    var existingExpenseQuery = _context.Expenses.AsQueryable();

                    if (!isAdmin)
                    {
                        existingExpenseQuery = existingExpenseQuery.Where(e => e.UserId == userId);
                    }

                    var existingExpense = await existingExpenseQuery.FirstOrDefaultAsync(e => e.Id == id);
                    if (existingExpense == null)
                    {
                        return NotFound();
                    }

                    existingExpense.Description = expense.Description;
                    existingExpense.Amount = expense.Amount;
                    existingExpense.Category = expense.Category;
                    existingExpense.Date = expense.Date;
                    existingExpense.Notes = expense.Notes;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle the case where the expense was deleted by another user
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Re-throw if it's a different concurrency issue
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    // Handle database errors
                    ModelState.AddModelError("", "An error occurred while updating the expense. Please try again.");
                    return View(expense);
                }
                // Set success message
                TempData["SuccessMessage"] = $"Expense '{expense.Description}' updated successfully!";
                // Redirect to the Index page after successful update
                return RedirectToAction(nameof(Index));
            }
            // If validation failed, return to the form with error messages
            return View(expense);
        }

        /// <summary>
        /// Displays a confirmation page before deleting an expense.
        /// GET: Expense/Delete/5
        /// </summary>
        /// <param name="id">The ID of the expense to delete</param>
        /// <returns>View showing the expense details with a delete confirmation prompt</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var expensesQuery = _context.Expenses
                .Include(e => e.User)
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var expense = await expensesQuery.FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            // Show the confirmation view with expense details
            return View(expense);
        }

        /// <summary>
        /// Processes the deletion of an expense after user confirmation.
        /// POST: Expense/Delete/5
        /// </summary>
        /// <param name="id">The ID of the expense to delete</param>
        /// <returns>Redirects to Index after successful deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]  // Prevents Cross-Site Request Forgery attacks
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserIdFromSessionOrClaims();

            if (!isAdmin && userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var expensesQuery = _context.Expenses.AsQueryable();

            if (!isAdmin)
            {
                expensesQuery = expensesQuery.Where(e => e.UserId == userId);
            }

            var expense = await expensesQuery.FirstOrDefaultAsync(e => e.Id == id);
            if (expense != null)
            {
                // Remove the expense from the database context
                _context.Expenses.Remove(expense);
                // Commit the deletion to the database
                await _context.SaveChangesAsync();
                // Set success message
                TempData["SuccessMessage"] = $"Expense '{expense.Description}' deleted successfully!";
            }
            // Redirect to the Index page to show the updated list
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if an expense with the given ID exists in the database.
        /// Used to handle concurrency issues during updates.
        /// </summary>
        /// <param name="id">The ID of the expense to check</param>
        /// <returns>True if the expense exists, false otherwise</returns>
        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }

        private int? GetUserIdFromSessionOrClaims()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue)
            {
                return sessionUserId.Value;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Session.SetInt32("UserId", userId);
                return userId;
            }

            return null;
        }
    }
}
