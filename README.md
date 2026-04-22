Construction Stock Management System
Tech Stack: ASP.NET Core Web API · SQL Server (SSMS) · Entity Framework Core (database-first) · HTML/CSS/JS · JWT Authentication · BCrypt · QuestPDF

Project Goal
A web-based system that helps a Stock Manager monitor and manage construction materials across sites. It eliminates stock shortages that cause workers to stand idle — and every idle minute is money wasted. The system automates daily reporting, sends low-stock alerts, and in a future phase predicts when to restock and which supplier to contact based on GPS location.

Roles
RoleAccessStock ManagerFull site dashboard, all transactions, all alerts, daily reports, supplier management, user overviewStorekeeperRecord IN/OUT only, view own entries only, scoped to their site

Project Structure
ConstructionStock/
├── ConstructionStockAPI/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ItemsController.cs
│   │   ├── TransactionsController.cs
│   │   ├── AlertsController.cs
│   │   ├── ReportsController.cs
│   │   ├── SuppliersController.cs
│   │   └── UsersController.cs
│   ├── Data/
│   │   └── ConstructionStockDbContext.cs
│   ├── DTOs/
│   │   ├── AuthDTOs.cs
│   │   ├── TransactionDTOs.cs
│   │   ├── ItemDTOs.cs
│   │   ├── AlertDTOs.cs
│   │   ├── ReportDTOs.cs
│   │   ├── SupplierDTOs.cs
│   │   └── UserDTOs.cs
│   ├── Helpers/
│   │   └── ApiResponse.cs
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs
│   ├── Models/
│   │   ├── Site.cs
│   │   ├── User.cs
│   │   ├── Item.cs
│   │   ├── Supplier.cs
│   │   ├── StockTransaction.cs
│   │   └── LowStockAlert.cs
│   ├── Services/
│   │   ├── TokenService.cs
│   │   ├── ReportService.cs
│   │   └── AlertService.cs
│   ├── appsettings.json
│   └── Program.cs
│
└── ConstructionStock.Web/
    ├── index.html                  ← Login page
    ├── dashboard.html              ← Stock Manager dashboard
    ├── storekeeper.html            ← Storekeeper dashboard
    ├── transactions.html           ← Full transaction log (Manager)
    ├── my-transactions.html        ← Storekeeper own records
    ├── items.html                  ← Items list + stock levels
    ├── alerts.html                 ← Low stock alerts
    ├── reports.html                ← Daily report + export
    ├── suppliers.html              ← Supplier management
    ├── users.html                  ← Site users (Manager only)
    ├── css/
    │   └── style.css
    └── js/
        ├── api.js                  ← Shared fetch wrapper with JWT header
        ├── auth.js                 ← Login, logout, token management
        ├── guard.js                ← Route protection, redirect if no token
        ├── dashboard.js
        ├── storekeeper.js
        ├── transactions.js
        ├── alerts.js
        ├── reports.js
        ├── items.js
        ├── suppliers.js
        └── users.js

Database (SQL Server)
TablePurposeSitesConstruction sites (top-level scope for all data)UsersStock Managers and Storekeepers, each tied to a siteItemsMaterial catalog per site (Cement, Steel Rods, Sand, etc.)SuppliersSupplier directory with GPS coordinates for future predictionStockTransactionsEvery IN and OUT movement with timestamp and recorderLowStockAlertsAuto-created by DB trigger when stock drops to or below minimum
Key trigger: trg_AfterStockTransaction fires on every insert into StockTransactions. It auto-updates CurrentQuantity on Items and raises a LowStockAlert if stock hits the minimum threshold. Only one unresolved alert per item is allowed at a time.

Full API Endpoints
Auth
MethodEndpointRoleDescriptionPOST/api/auth/loginPublicLogin, returns JWT token
Items
MethodEndpointRoleDescriptionGET/api/itemsManager, StorekeeperAll items for the user's site with stock statusGET/api/items/{id}ManagerSingle item detailPOST/api/itemsManagerAdd a new item to the sitePUT/api/items/{id}ManagerUpdate item name, unit, or minimum quantityDELETE/api/items/{id}ManagerSoft delete (sets IsActive = false)
Transactions
MethodEndpointRoleDescriptionPOST/api/transactions/recordManager, StorekeeperRecord a stock IN or OUTGET/api/transactions/logManager = site-wide, Storekeeper = own onlyTransaction logGET/api/transactions/stock-statusManagerCurrent stock levelsGET/api/transactions/itemsManager, StorekeeperItems dropdown helperGET/api/transactions/suppliersManager, StorekeeperSuppliers dropdown helper
Alerts
MethodEndpointRoleDescriptionGET/api/alertsManagerAll unresolved low stock alerts for the sitePUT/api/alerts/{id}/resolveManagerMark an alert as resolved
Reports
MethodEndpointRoleDescriptionGET/api/reports/daily?date=YYYY-MM-DDManagerDaily IN/OUT summary for a specific dateGET/api/reports/daily/export?date=YYYY-MM-DDManagerExport daily report as PDFGET/api/reports/stock-summaryManagerCurrent stock levels for all items on site
Suppliers
MethodEndpointRoleDescriptionGET/api/suppliersManagerList all active suppliersGET/api/suppliers/{id}ManagerSingle supplier with delivery historyPOST/api/suppliersManagerAdd a new supplierPUT/api/suppliers/{id}ManagerUpdate supplier details
Users
MethodEndpointRoleDescriptionGET/api/usersManagerList all users on the manager's siteGET/api/users/{id}ManagerSingle user detail

Overall Progress
Backend — Nearly Complete

 Project setup and solution structure
 EF Core database-first scaffold (all 6 models + DbContext)
 JWT authentication wired in Program.cs
 Helpers/ApiResponse.cs standard response wrapper
 Middleware/ErrorHandlingMiddleware.cs registered
 DTOs/AuthDTOs.cs + Services/TokenService.cs
 Controllers/AuthController.cs (login working)
 DTOs/TransactionDTOs.cs + Controllers/TransactionsController.cs
 DTOs/ItemDTOs.cs + Controllers/ItemsController.cs
 DTOs/AlertDTOs.cs + Services/AlertService.cs + Controllers/AlertsController.cs
 DTOs/ReportDTOs.cs + Services/ReportService.cs + Controllers/ReportsController.cs
 DTOs/SupplierDTOs.cs + Controllers/SuppliersController.cs
 DTOs/UserDTOs.cs + Controllers/UsersController.cs
 Input validation on AuthDTOs and TransactionDTOs
 Test project with core happy-path tests
 REMAINING: Standardize role-based authorization across all controllers
 REMAINING: Input validation on remaining DTOs (Item, Alert, Supplier, User, Report)
 REMAINING: Expand test coverage (negative-path + authorization tests)

Frontend — Not Started

 All HTML pages
 All JS modules
 CSS styling
 API integration
 Route protection


Team & Task Assignments

Work is running in parallel. Backend gap closes while frontend is being built.
Target: both tracks done at the same time.


Developer 1 (Project Lead) — Backend Authorization + Testing
Track: Backend gap closure + quality assurance
Works on: ConstructionStockAPI/
Immediate tasks

 Standardize [Authorize(Roles = "StockManager")] and [Authorize(Roles = "Storekeeper")] consistently across all 7 controllers
 Verify every endpoint rejects the wrong role with a proper 403 Forbidden response
 Add input validation attributes to remaining DTOs: ItemDTOs.cs, AlertDTOs.cs, SupplierDTOs.cs, UserDTOs.cs, ReportDTOs.cs
 Expand test project: negative-path tests (wrong role, invalid token, OUT exceeds stock)
 Write authorization integration tests for at least: login, record transaction, resolve alert
 Review and merge all pull requests from Developers 2, 3, and 4
 Final dotnet build and smoke test before frontend integration

Business rules to enforce in tests

Storekeeper calling a Manager-only endpoint must return 403
Unauthenticated request to any protected endpoint must return 401
Stock OUT quantity greater than CurrentQuantity must return 400 with a clear message
SiteId in JWT must always scope data — user must never see another site's data


Developer 2 — Frontend Lead (Dashboard + Core Pages)
Track: Frontend — shared utilities + Manager-facing pages
Works on: ConstructionStock.Web/
Setup first (everyone on frontend depends on these)

 css/style.css — base layout, navigation bar, table styles, status pills (OK = green, LOW = red), alert badge, button styles, responsive basics
 js/api.js — base apiFetch(endpoint, method, body) wrapper that automatically attaches Authorization: Bearer <token> header, handles 401 by redirecting to login
 js/auth.js — login(username, password) posts to /api/auth/login, saves token, role, userId, siteId, fullName to localStorage, logout() clears storage and redirects
 js/guard.js — on every page load checks for token in localStorage, redirects to index.html if missing or expired, also checks role and redirects if wrong role for that page

Pages to build

 index.html — login form, calls auth.js login(), on success redirects to dashboard.html (Manager) or storekeeper.html (Storekeeper) based on role
 dashboard.html + js/dashboard.js — stock levels table with OK/LOW pill per item, unresolved alert count badge in nav, site name and logged-in user shown in header, links to all other pages
 transactions.html + js/transactions.js — full site transaction log table (Manager only), columns: date, type, item, quantity, recorded by, supplier, remarks. Filter by date range.
 reports.html + js/reports.js — date picker defaulting to today, IN summary table, OUT summary table, total counts, Export PDF button calling /api/reports/daily/export

Key rules for frontend

Token, role, userId, siteId are always read from localStorage — never hardcoded
api.js is the only file that touches fetch — all other JS files call apiFetch
Every page starts with guard.js check before rendering anything
Manager pages redirect Storekeepers away immediately


Developer 3 — Frontend (Storekeeper View + Items + Alerts)
Track: Frontend — Storekeeper pages + item and alert management
Works on: ConstructionStock.Web/
Depends on: Developer 2 finishing api.js, auth.js, guard.js, and style.css first
Pages to build

 storekeeper.html + js/storekeeper.js — Storekeeper landing page, shows a quick IN/OUT record form at the top (item dropdown, type toggle, quantity, supplier for IN, remarks), below it shows a table of the storekeeper's own recent entries from /api/transactions/log
 my-transactions.html (linked from storekeeper dashboard) — full own-records table with date, type, item, quantity, supplier, remarks
 items.html + js/items.js — items table for Manager: ItemName, Unit, CurrentQuantity, MinimumQuantity, Status pill. Add Item button opens an inline form (POST /api/items). Edit button per row (PUT /api/items/{id}). Soft delete button per row with confirmation prompt.
 alerts.html + js/alerts.js — unresolved alerts table: site, item, quantity at alert, date. Resolve button per row calls PUT /api/alerts/{id}/resolve and removes the row. Show "No active alerts" when empty. Auto-refresh every 60 seconds.

Key rules

Item dropdown on record form loads from /api/transactions/items
Supplier dropdown loads from /api/transactions/suppliers and only shows when type is IN
Quantity field must be a positive integer — validate before submitting
Alert count badge in nav must update after resolving an alert


Developer 4 — Frontend (Suppliers + Users + Integration QA)
Track: Frontend — supplier and user pages + end-to-end testing
Works on: ConstructionStock.Web/
Depends on: Developer 2 finishing api.js, auth.js, guard.js, and style.css first
Pages to build

 suppliers.html + js/suppliers.js — supplier list table: name, contact person, phone, email. Add Supplier form (POST /api/suppliers). Edit button per row (PUT /api/suppliers/{id}). Click a row to expand delivery history from /api/suppliers/{id}.
 users.html + js/users.js — site users table (Manager only): full name, username, role badge (StockManager = blue, Storekeeper = gray), active status, created date. Read-only — no add/edit on this page.

Integration QA tasks (once all pages are built)

 Test full login flow for both roles — confirm correct redirect per role
 Test route guard — manually clear localStorage and confirm every page redirects to login
 Test Stock Manager sees full transaction log, Storekeeper sees only own records
 Test recording IN transaction — confirm stock quantity increases on dashboard
 Test recording OUT transaction — confirm stock quantity decreases, LOW pill appears if below minimum
 Test recording OUT that exceeds stock — confirm API returns error and page shows message
 Test resolving an alert — confirm it disappears from alerts page
 Test PDF export — confirm file downloads and contains correct date's data
 Test add/edit item — confirm changes reflect on items page
 Test add/edit supplier — confirm changes reflect on suppliers page
 Test logout — confirm token cleared and all pages redirect to login


Key Business Rules

A Storekeeper can only see transactions they personally recorded (RecordedByUserId).
A Stock Manager sees all data scoped strictly to their SiteId — never another site.
Stock OUT cannot exceed current quantity — validated in the API before insert.
SiteId and UserId are always read from JWT token claims, never from the request body.
LowStockAlert is raised automatically by the database trigger — never created manually.
Only one unresolved alert per item at a time — duplicate suppression is in the trigger.
Passwords are hashed with BCrypt — plain text passwords are never stored or logged.
Deleting an item is a soft delete (IsActive = false) — history is never lost.
All API responses follow the standard ApiResponse<T> wrapper shape.
JWT token expires after 8 hours — user must log in again.


Standard API Response Shape
Success:
json{
  "success": true,
  "message": "OK",
  "data": { }
}
Error:
json{
  "success": false,
  "message": "Stock OUT quantity exceeds current stock.",
  "data": null
}

Running the Project
API:
bashcd ConstructionStockAPI
dotnet run
# Listening on http://localhost:5287
Frontend:
Open ConstructionStock.Web/index.html with Live Server in VS Code.
Runs on http://127.0.0.1:5500

Default Test Credentials
UsernamePasswordRoleSitejpierrePassword123StockManagerSite A - Kigali CBDauwasePassword123StorekeeperSite A - Kigali CBDbmugishaPassword123StockManagerSite B - NyamirambogiradukPassword123StorekeeperSite B - Nyamirambo

NuGet Packages
bashdotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
dotnet add package QuestPDF

Environment Configuration (appsettings.json)
json{
  "ConnectionStrings": {
    "DefaultConnection": "Server=Jonathan-PC\\SQLEXPRESS;Database=ConstructionStockDB;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_MakeItLongAndRandom_32chars+",
    "Issuer": "ConstructionStockAPI",
    "Audience": "ConstructionStockClient",
    "ExpiryHours": 8
  },
  "AllowedHosts": "*"
}

Phase 3 — Future (Prediction Module)

 Consumption trend analysis from transaction history
 Restock forecast — predict when stock will hit minimum quantity
 Nearest supplier suggestion using GPS coordinates from Suppliers table
 Automated alert with suggested supplier name and contact details


Important Notes

JWT key must be secret — use environment variables or Azure Key Vault in production.
Never expose PasswordHash in any API response DTO.
All SiteId values must come from JWT claims — never trust client-supplied site IDs.
The prediction module needs at least 30 days of transaction history for meaningful results.
Deploy API to IIS or as a self-hosted Windows Service; frontend is served as static files
