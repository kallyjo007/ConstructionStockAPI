# Construction Stock API

ASP.NET Core Web API for construction stock management across multiple sites.

## Current Scope

- Backend-only repository (`ConstructionStockAPI`)
- JWT authentication
- EF Core database-first with SQL Server
- Core transaction recording and stock visibility endpoints

## Tech Stack

- ASP.NET Core Web API (v10.0)
- SQL Server + Entity Framework Core (database-first)
- JWT Authentication
- BCrypt password verification
- QuestPDF (for report generation)

## Current Project Structure

```text
ConstructionStockAPI/
├── Controllers/
│   ├── AlertsController.cs
│   ├── AuthController.cs
│   ├── ItemsController.cs
│   ├── ReportsController.cs
│   ├── SuppliersController.cs
│   ├── TransactionsController.cs
│   └── UsersController.cs
├── Data/
│   └── ConstructionStockDbContext.cs
├── DTOs/
│   ├── AlertDTOs.cs
│   ├── AuthDTOs.cs
│   ├── ItemDTOs.cs
│   ├── ReportDTOs.cs
│   ├── SupplierDTOs.cs
│   ├── TransactionDTOs.cs
│   └── UserDTOs.cs
├── Helpers/
│   └── ApiResponse.cs
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
├── Models/
│   ├── Item.cs
│   ├── LowStockAlert.cs
│   ├── Site.cs
│   ├── StockTransaction.cs
│   ├── Supplier.cs
│   └── User.cs
├── Services/
│   ├── AlertService.cs
│   ├── ReportService.cs
│   └── TokenService.cs
├── Program.cs
└── appsettings.json
```

## Progress Tracker (Single Developer)

### Phase 1 - Backend API

- [x] Project setup
- [x] EF Core database-first context and models
- [x] JWT authentication wiring in `Program.cs`
- [x] `DTOs/AuthDTOs.cs`
- [x] `Services/TokenService.cs`
- [x] `Controllers/AuthController.cs` (login implemented)
- [x] `DTOs/TransactionDTOs.cs`
- [x] `Controllers/TransactionsController.cs` (record, log, item/supplier helper endpoints, stock status)
- [x] Remove temporary `/api/auth/hashpassword/{password}` endpoint
- [x] Remove any leftover WeatherForecast artifacts (none present in repository)
- [x] Add `Helpers/ApiResponse.cs` standard response wrapper
- [x] Add `Middleware/ErrorHandlingMiddleware.cs`
- [x] Register custom middleware in `Program.cs`
- [x] Add DTO input validation attributes/rules for implemented DTOs (`AuthDTOs`, `TransactionDTOs`)
- [ ] Standardize role-based authorization across all endpoints
- [x] Add baseline test project `ConstructionStockAPI.Tests` with core happy-path tests

### Phase 1 - Remaining Backend Modules

- [x] `Controllers/ItemsController.cs` + `DTOs/ItemDTOs.cs`
- [x] `Controllers/AlertsController.cs` + `DTOs/AlertDTOs.cs` + `Services/AlertService.cs`
- [x] `Controllers/ReportsController.cs` + `DTOs/ReportDTOs.cs` + `Services/ReportService.cs`
- [x] `Controllers/SuppliersController.cs` + `DTOs/SupplierDTOs.cs`
- [x] `Controllers/UsersController.cs` + `DTOs/UserDTOs.cs`

### Phase 2 - Frontend (Separate Module)

- [ ] Frontend project files (`index.html`, dashboards, pages, JS modules, CSS)
- [ ] API integration layer (`api.js`, auth/guard handling)
- [ ] Role-based page access behavior

### Phase 3 - Future Enhancements

- [ ] Consumption trend analysis
- [ ] Restock date prediction
- [ ] Nearest supplier recommendation using GPS
- [ ] Auto-alert with supplier suggestion

## Implemented API Endpoints

### Auth

- `POST /api/auth/login`

### Transactions

- `GET /api/transactions/stock-status` (StockManager)
- `POST /api/transactions/record` (authorized users)
- `GET /api/transactions/log` (StockManager = site-wide, Storekeeper = own records)
- `GET /api/transactions/items`
- `GET /api/transactions/suppliers`

### Items

- `GET /api/items` (authorized users)
- `GET /api/items/{id}` (StockManager)
- `POST /api/items` (StockManager)
- `PUT /api/items/{id}` (StockManager)
- `DELETE /api/items/{id}` (StockManager, soft delete)

### Alerts

- `GET /api/alerts` (StockManager)
- `PUT /api/alerts/{id}/resolve` (StockManager)

### Reports

- `GET /api/reports/daily?date=YYYY-MM-DD` (StockManager)
- `GET /api/reports/daily/export?date=YYYY-MM-DD` (StockManager, PDF)
- `GET /api/reports/stock-summary` (StockManager)

### Suppliers

- `GET /api/suppliers` (StockManager)
- `GET /api/suppliers/{id}` (StockManager, includes recent delivery history)
- `POST /api/suppliers` (StockManager)
- `PUT /api/suppliers/{id}` (StockManager)

### Users

- `GET /api/users` (StockManager, same-site users)
- `GET /api/users/{id}` (StockManager, same-site user detail)

## Business Rules in Place

- Site scoping comes from JWT claim (`SiteId`)
- User identity comes from JWT claim (`NameIdentifier`)
- Storekeeper sees only own transaction records in log endpoint
- Stock OUT is blocked when quantity exceeds current stock
- Supplier is required for stock IN transactions

## Environment Configuration

Use `appsettings.json` for local development only.

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryHours`

For production, move secrets to environment variables or secret manager.

## Run Locally

```bash
dotnet restore
dotnet run
```

Default URL is provided by ASP.NET launch settings / runtime output.

## Next Priority Tasks

1. Standardize role-based authorization policy usage across all modules.
2. Expand test coverage (integration tests + negative-path authorization tests).
3. Improve production hardening (secret management, health checks, CI/CD).
4. Add frontend module and connect all API screens.
5. Add environment-specific deployment and runtime docs.
