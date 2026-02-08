// Required namespaces for Entity Framework Core and database operations
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;

// Create the web application builder with default configuration
var builder = WebApplication.CreateBuilder(args);

// Add MVC services to the dependency injection container
// This enables the use of Controllers and Views in the application
builder.Services.AddControllersWithViews();

// Configure the database connection for the application
// First tries to get the connection string from appsettings.json
// Falls back to a default SQLite database file if not found
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=ExpenseTracker.db";

// Register the AppDbContext with dependency injection
// Configures Entity Framework Core to use SQLite as the database provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Build the application from the configured services
var app = builder.Build();

// Configure the HTTP request pipeline based on the environment
if (!app.Environment.IsDevelopment())
{
    // In production, use a global error handler for better security
    app.UseExceptionHandler("/Home/Error");
    
    // Enable HTTP Strict Transport Security (HSTS) for enhanced security
    // Forces browsers to use HTTPS for all future requests to this domain
    app.UseHsts();
}

// Redirect all HTTP requests to HTTPS for secure communication
app.UseHttpsRedirection();

// Enable routing to match incoming requests to appropriate controllers
app.UseRouting();

// Enable authorization middleware for access control
app.UseAuthorization();

// Map static files (CSS, JavaScript, images) from wwwroot folder
app.MapStaticAssets();

// Configure the default route pattern for MVC
// Pattern: /{controller}/{action}/{id}
// Default: /Expense/Index (shows the expense list on startup)
// The {id?} parameter is optional and used for operations on specific expenses
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Expense}/{action=Index}/{id?}")
    .WithStaticAssets();

// Start the application and begin listening for incoming HTTP requests
app.Run();
