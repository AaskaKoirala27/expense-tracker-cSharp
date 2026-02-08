using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    /// <summary>
    /// Controller responsible for handling all expense-related operations.
    /// Implements full CRUD (Create, Read, Update, Delete) functionality for expense management.
    /// </summary>
    public class ExpenseController : Controller
    {
        // Database context for accessing and manipulating expense data
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that initializes the controller with the database context.
        /// The context is injected via dependency injection configured in Program.cs.
        /// </summary>
        /// <param name="context">Database context for expense operations</param>
        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the main dashboard showing all expenses.
        /// GET: Expense/Index
        /// </summary>
        /// <returns>View with a list of all expenses including summary information</returns>
        public async Task<IActionResult> Index()
        {
            // Retrieve all expenses from the database asynchronously
            var expenses = await _context.Expenses.ToListAsync();
            return View(expenses);
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

            // Query the database for the expense with the specified ID
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.Id == id);
            
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
            // Check if all validation requirements are met
            if (ModelState.IsValid)
            {
                // Add the new expense to the context
                _context.Add(expense);
                // Save changes to the database asynchronously
                await _context.SaveChangesAsync();
                // Redirect to the Index page to show the updated list
                return RedirectToAction(nameof(Index));
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

            // Find the expense in the database by ID
            var expense = await _context.Expenses.FindAsync(id);
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

            // Check if all validation requirements are met
            if (ModelState.IsValid)
            {
                try
                {
                    // Mark the expense as modified and update in the database
                    _context.Update(expense);
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

            // Retrieve the expense from the database
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.Id == id);
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
            // Find the expense by ID
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                // Remove the expense from the database context
                _context.Expenses.Remove(expense);
                // Commit the deletion to the database
                await _context.SaveChangesAsync();
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
    }
}
