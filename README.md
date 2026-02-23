# expense-tracker-cSharp

## Overview

Expense Tracker is a comprehensive web application built with ASP.NET Core MVC that enables users to efficiently track, manage, and visualize their personal expenses. The application features a robust authentication system with role-based access control, allowing different levels of user permissions including regular users, admins, and super admins.

### Key Features

- **Expense Management**: Create, read, update, and delete expense records with detailed information
- **User Authentication & Authorization**: Secure login/registration system with password hashing and role-based permissions
- **Interactive Dashboard**: Visualize expense data through charts and graphs to gain insights into spending patterns
- **Superadmin Panel**: Comprehensive administrative interface for managing users, roles, and system menus
- **Multi-User Support**: Each user has their own isolated expense data with personalized menu access
- **Dynamic Menu System**: Customizable navigation menus based on user roles and permissions
- **Expense Analytics**: Track spending trends, view summaries, and generate reports
- **Responsive Design**: Modern, user-friendly interface accessible from various devices

### Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **Database**: Entity Framework Core with SQLite
- **Authentication**: Custom authentication with secure password hashing
- **Architecture**: MVC pattern with Service layer for business logic
- **View Components**: Reusable UI components for dynamic menu rendering
- **Frontend**: Razor views with HTML, CSS, and JavaScript

### User Roles

1. **Regular User**: Can manage their own expenses and view personal dashboards
2. **Super Admin**: Has full system access including menu and role management

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A code editor (e.g., [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/))
- Basic knowledge of C# and ASP.NET Core MVC

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/AaskaKoirala27/expense-tracker-cSharp.git
cd expense-tracker-cSharp
```

### 2. Navigate to Project Directory

```bash
cd ExpenseTracker
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

The project uses Entity Framework Core with SQLite. Run the following command to create the database:

```bash
dotnet ef database update
```

This will create an `ExpenseTracker.db` file in your project directory with all necessary tables.

### 5. Run the Application

```bash
dotnet run
```

The application will start and be accessible at:
- **HTTP**: `http://localhost:5182`

## Configuration

### Connection String

The database connection is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ExpenseTracker.db"
  }
}
```

You can modify this to use a different SQLite database file or path.

### Default Credentials

On first run, the application automatically seeds a super admin account:

- **Username**: `superadmin`
- **Password**: `SuperSecret123`

> ⚠️ **Security Note**: Change the default super admin password immediately after first login in a production environment.

## Project Structure

```
ExpenseTracker/
├── Controllers/         # MVC Controllers handling HTTP requests
│   ├── AccountController.cs      # Authentication & registration
│   ├── AdminController.cs        # Admin panel functionality
│   ├── DashboardController.cs    # User dashboard
│   ├── ExpenseController.cs      # Expense CRUD operations
│   ├── ExpenseApiController.cs   # RESTful API endpoints
│   └── HomeController.cs         # Home page
├── Data/                # Database context and seeding
│   ├── AppDbContext.cs           # EF Core DbContext
│   └── DataSeeder.cs             # Initial data seeding
├── Models/              # Entity models
│   ├── User.cs                   # User entity
│   ├── Expense.cs                # Expense entity
│   ├── Role.cs                   # Role entity
│   ├── Menu.cs                   # Menu entity
│   ├── UserRole.cs               # User-Role relationship
│   └── UserMenu.cs               # User-Menu relationship
├── Services/            # Business logic services
│   ├── DashboardService.cs       # Dashboard data aggregation
│   ├── ExpenseGraphService.cs    # Expense visualization
│   ├── HomeSummaryService.cs     # Home page summaries
│   ├── MenuService.cs            # Menu management
│   ├── PasswordHasher.cs         # Password hashing utility
│   └── SuperAdminService.cs      # Super admin operations
├── ViewComponents/      # Reusable view components
│   └── MainMenuViewComponent.cs  # Dynamic menu rendering
├── ViewModels/          # Data transfer objects for views
├── Views/               # Razor view templates
│   ├── Account/                  # Login/Register views
│   ├── Admin/                    # Admin panel views
│   ├── Dashboard/                # Dashboard views
│   ├── Expense/                  # Expense management views
│   ├── Home/                     # Home page views
│   └── Shared/                   # Shared layouts and partials
├── wwwroot/             # Static files (CSS, JS, images)
└── Migrations/          # EF Core database migrations
```

## Database Schema

### Core Entities

#### User
- `Id` (int, Primary Key)
- `Username` (string, Required)
- `PasswordHash` (string, Required)
- `IsActive` (bool)
- Relationships: UserRoles, Expenses, UserMenus

#### Expense
- `Id` (int, Primary Key)
- `UserId` (int, Foreign Key)
- `Description` (string, Required, Max 100 chars)
- `Amount` (decimal, Required)
- `Category` (string, Required, Max 50 chars)
- `Date` (DateTime)
- `CreatedAt` (DateTime)
- Relationships: User

#### Role
- `Id` (int, Primary Key)
- `RoleName` (string, Required)
- Relationships: UserRoles

#### Menu
- `Id` (int, Primary Key)
- `Title` (string)
- `Url` (string)
- Relationships: UserMenus

### Relationship Tables
- **UserRole**: Many-to-many between Users and Roles
- **UserMenu**: Many-to-many between Users and Menus

## Features in Detail

### For Regular Users
- **Register & Login**: Create an account and securely log in
- **Add Expenses**: Record expenses with description, amount, category, and date
- **View Expenses**: Browse all personal expenses in a table format
- **Edit/Delete Expenses**: Modify or remove expense records
- **Dashboard**: View spending analytics and charts
- **Expense Graphs**: Visualize spending patterns over time

### For Admins
- **User Management**: View and manage all registered users
- **View All Expenses**: Access expense records from all users
- **User Activation**: Enable or disable user accounts

### For Super Admin
- **Full Access**: All User and Admin features
- **Menu Management**: Configure navigation menus for different roles
- **Role Assignment**: Assign or modify user roles

## API Documentation

The application includes a RESTful API for expense management.

### Base URL
```
/api/expenseapi
```

### Endpoints

#### Get All Expenses
```http
GET /api/expenseapi
```
Returns all expenses in the system.

#### Get Expense by ID
```http
GET /api/expenseapi/{id}
```
Returns a specific expense by ID.

#### Create Expense
```http
POST /api/expenseapi
Content-Type: application/json

{
  "description": "Grocery shopping",
  "amount": 45.50,
  "category": "Food",
  "date": "2026-02-23T10:00:00"
}
```

#### Update Expense
```http
PUT /api/expenseapi/{id}
Content-Type: application/json

{
  "id": 1,
  "description": "Updated description",
  "amount": 50.00,
  "category": "Food",
  "date": "2026-02-23T10:00:00"
}
```

#### Delete Expense
```http
DELETE /api/expenseapi/{id}
```

## Development

### Running in Development Mode

The application is configured to use different settings in development:

```bash
dotnet run --environment Development
```

### Adding New Migrations

When you modify the database models, create a new migration:

```bash
dotnet ef migrations add YourMigrationName
```

Then apply it:

```bash
dotnet ef database update
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## Troubleshooting

### Database Issues

**Problem**: Database doesn't exist or migrations fail

**Solution**: 
```bash
# Remove existing database
rm ExpenseTracker.db

# Reapply all migrations
dotnet ef database update
```

### Login Issues

**Problem**: Cannot login with default credentials

**Solution**: Ensure the database has been seeded. Check that the `superadmin` user exists in the database. You may need to delete the database and rerun migrations.

### Port Already in Use

**Problem**: Port 5182 is already in use

**Solution**: Modify the port in `Properties/launchSettings.json` or kill the process using the port.

## Future Enhancements

- [ ] Export expenses to CSV/Excel
- [ ] Email notifications for budget limits
- [ ] Recurring expense tracking
- [ ] Multi-currency support
- [ ] Mobile app integration
- [ ] Budget planning and alerts
- [ ] Receipt image uploads
- [ ] Advanced filtering and search
- [ ] Two-factor authentication

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b new`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin new`)
5. Open a Pull Request

## License

This project is open source and available under the [MIT License](LICENSE).

## Author

**Aaska Koirala**
- GitHub: [@AaskaKoirala27](https://github.com/AaskaKoirala27)

**Aishmita Yonzan**
- GitHub: [@xaishmitax] (https://github.com/xaishmitax)

## Acknowledgments

- Built with ASP.NET Core MVC
- Uses Entity Framework Core for data access
- SQLite for local database storage

