# Construction Stock Management System

A comprehensive stock and material tracking API specifically designed for managing construction sites, powered by a .NET 10 Web API backend and an integrated Vanilla JS / HTML frontend.

## Features
- **Clean Architecture:** Organized structure segregating controllers, services, models, and data access.
- **Robust Authentication:** Secure JWT Bearer authentication coupled with BCrypt password hashing logic.
- **Role-Based Access Control:** Separate isolated workflows for 3 precise roles:
  - `Admin`: Global management dashboard to handle site creation, user assignments, vendor registries, and top-level analytic summaries.
  - `StockManager`: Site-level inventory oversight, capable of viewing alerts, transactions, items, and exporting real-time tracking PDF reports to visualize operations.
  - `Storekeeper`: Restricted operational role purely structured around safely importing (`IN`) and exporting (`OUT`) stock daily.
- **Dynamic PDF Reporting:** Daily operational site metrics converted to robust PDF exports via QuestPDF.
- **Alert System:** Immediate system notifications for any low-stock material dropping beneath configured minimum quantities.

## Tech Stack
- **Backend:** C# / .NET 10 Web API
- **Database:** Entity Framework Core (ORM) coupled with SQL Server
- **Frontend:** Vanilla HTML5, CSS3, JavaScript (Fetch API) securely housed in `ConstructionStock.Web`
- **Authentication:** JWT (JSON Web Tokens) & BCrypt.Net-Next
- **Document Generation:** QuestPDF (Community License)

## Local Development & Setup
1. **Database Setup**
   Modify the default connection string in your `appsettings.json` if necessary:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=...;Database=ConstructionStockDB;Integrated Security=True;TrustServerCertificate=True;"
   }
   ```
   Ensure SQL Server is active, and update your database by running migration scripts or regenerating the database if needed.

2. **Starting the Application**
   Run the .NET application from the root project folder:
   ```bash
   dotnet build
   dotnet run
   ```
   The backend API runs concurrently with the integrated frontend.

3. **Accessing the Portal**
   Navigate to `localhost:5287/index.html` (or your defined port) to open the login portal. 
   - Note: Users authenticate and strictly interact utilizing the newly deployed `index.html` front page securely routing them to their roles.

## Project Setup & Solution Structure
- EF Core database-first scaffold (all 6 models + DbContext)
- JWT authentication wired in Program.cs
- Middlewares: ErrorHandlingMiddleware.cs registered
- Auth: DTOs/AuthDTOs.cs & Services/TokenService.cs
- Controllers: AuthController.cs, TransactionsController.cs, ItemsController.cs, AlertsController.cs, ReportsController.cs, SuppliersController.cs, UsersController.cs

## Application Architecture
- `ConstructionStock.Web/` : Complete Client-Side logic, HTML files, CSS, and modular logic scripts per view.
- `Controllers/` : API entry points defining Http methods and structural Authorization rules.
- `Services/` : Business logic orchestration (Auth, Users, Items, Reports).
- `DTOs/` : Contract Models passing strictly typed payloads.
- `Models/` : EF Core Entities mapping to the SQL database.

## Important Notes
- JWT key must be secret — use environment variables or Azure Key Vault in production.
- Never expose PasswordHash in any API response DTO.
- All SiteId values must come from JWT claims — never trust client-supplied site IDs.
- Deploy API to IIS or as a self-hosted Windows Service; frontend is served as static files.
