// Required namespaces for Entity Framework Core and database operations
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Services;

// Create the web application builder with default configuration
var builder = WebApplication.CreateBuilder(args);

// Store app startup time to invalidate cookies from previous sessions
var appStartTime = DateTimeOffset.UtcNow;

// Add MVC services to the dependency injection container
// This enables the use of Controllers and Views in the application
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/NotAuthorized";
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.Cookie.MaxAge = null; // Session cookie - expires when browser closes
        
        // Validate tickets to reject cookies from before app restart
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = context =>
            {
                var issuedUtc = context.Properties.IssuedUtc;
                if (issuedUtc.HasValue && issuedUtc.Value < appStartTime)
                {
                    // Cookie was issued before app restart - reject it
                    context.RejectPrincipal();
                    context.HttpContext.Session.Clear();
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Keep a role for legacy 'User' entries
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    // Allow access when user is in role User OR username is superadmin
    options.AddPolicy("UserOrSuperAdmin", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("User") || string.Equals(context.User.Identity?.Name, "superadmin", StringComparison.OrdinalIgnoreCase)
    ));
    // Superadmin-only policy (based on username)
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireAssertion(context =>
        string.Equals(context.User.Identity?.Name, "superadmin", StringComparison.OrdinalIgnoreCase)
    ));
});

// Configure the database connection for the application
// First tries to get the connection string from appsettings.json
// Falls back to a default SQLite database file if not found
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=ExpenseTracker.db";

// Register the AppDbContext with dependency injection
// Configures Entity Framework Core to use SQLite as the database provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<HomeSummaryService>();

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

app.UseSession();

app.UseAuthentication();

// Enable authorization middleware for access control
app.UseAuthorization();

// Map static files (CSS, JavaScript, images) from wwwroot folder
app.MapStaticAssets();

// Configure superadmin route
app.MapControllerRoute(
    name: "superadmin",
    pattern: "superadmin",
    defaults: new { controller = "Admin", action = "Login" });

// Configure the default route pattern for MVC
// Pattern: /{controller}/{action}/{id}
// Default: /Account/Login (shows the login page on startup)
// The {id?} parameter is optional and used for operations on specific expenses
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.SeedAsync(services);
}

// Start the application and begin listening for incoming HTTP requests
app.Run();
