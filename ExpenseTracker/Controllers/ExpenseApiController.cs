/*
 * File: Controllers/ExpenseApiController.cs
 * Purpose: Minimal Web API controller exposing CRUD endpoints for `Expense` entities.
 * Responsibilities:
 *  - Provide JSON endpoints for listing, retrieving, creating, updating and deleting expenses
 *  - Return appropriate HTTP status codes (200, 201, 204, 400, 404)
 * Important notes:
 *  - This controller is `ApiController`-annotated and uses route `api/expenseapi`
 *  - Input validation and authentication (if needed) should be enforced by middleware or filters
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    /// <summary>
    /// API controller exposing RESTful CRUD operations for `Expense`.
    /// Endpoints: GET all, GET by id, POST create, PUT update, DELETE remove.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/expenseapi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetAll()
        {
            return await _context.Expenses.ToListAsync();
        }

        // GET: api/expenseapi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetById(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return NotFound();

            return expense;
        }

        // POST: api/expenseapi
        [HttpPost]
        public async Task<ActionResult<Expense>> Create(Expense expense)
        {
            expense.CreatedAt = DateTime.Now;

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = expense.Id }, expense);
        }

        // PUT: api/expenseapi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Expense expense)
        {
            if (id != expense.Id)
                return BadRequest();

            _context.Entry(expense).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/expenseapi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}