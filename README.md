# Stock Management ERP

Enterprise-grade, modular, multi-tenant ERP solution for retail businesses built with .NET 8, Blazor Server, MudBlazor, and SQLite.

## Architecture

```
StockManagement.sln
├── src/
│   ├── StockManagement.Domain          # Entities, Enums, Domain Exceptions
│   ├── StockManagement.Application     # CQRS (MediatR), DTOs, Validators, Interfaces
│   ├── StockManagement.Infrastructure  # EF Core, Identity, Repositories, Seed Data
│   └── StockManagement.Web             # Blazor Server + MudBlazor UI
└── tests/
    └── StockManagement.Domain.Tests    # xUnit domain + validator tests
```

**Design Patterns:** Clean Architecture, CQRS, Repository + Unit of Work, Mediator (MediatR), Global Query Filters (multi-tenant), FluentValidation Pipeline

## Quick Start

### Prerequisites
- .NET 8 SDK
- (Optional) Docker + Docker Compose

### Run Locally

```bash
dotnet restore StockManagement.sln
dotnet run --project src/StockManagement.Web
```

Open `https://localhost:5001` (or `http://localhost:5000`)

### Run with Docker

```bash
docker-compose up --build
```

Open `http://localhost:5000`

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | admin@stockmgmt.com | Admin@123 |
| Shop Admin | shop@stockmgmt.com | Shop@123 |

## Features Implemented

### Core ERP
- Multi-tenant architecture with data isolation
- Role-based access control (10 roles)
- Identity with cookie auth + password lockout
- Audit logging
- Serilog structured logging

### Product Management
- Full CRUD with CQRS (MediatR)
- SKU, Barcode, Cost/Selling price, Tax, Discount
- Dynamic attributes (ProductAttributeDefinitions)
- Category, Brand, Supplier management
- Product search with filters (price range, category, brand, status)

### Inventory
- Stock items per warehouse
- Stock movements (purchase in, sale out, returns, adjustments, transfers)
- Low stock alerts
- Warehouse management

### Sales & Orders
- Order creation with auto-numbering
- Order lifecycle (Pending → Confirmed → Packed → Shipped → Delivered)
- Automatic stock deduction on order
- Stock restoration on cancellation
- Payment status tracking
- Multi-payment mode support

### Customer Management
- Customer profiles with loyalty points
- Total spend tracking
- Address management

### Dashboards
- Role-specific dashboard with KPIs
- Revenue, orders, products, customers, low-stock alerts
- Recent orders and top products

### Customer-Facing Store
- Product catalog with search, filters, pagination
- Product detail page with pricing
- MudBlazor-powered responsive UI

### Tech Stack
- .NET 8 + ASP.NET Core
- Blazor Server (Interactive)
- MudBlazor 6.20
- Entity Framework Core 8 (SQLite)
- ASP.NET Core Identity
- MediatR (CQRS)
- FluentValidation
- Serilog
- SignalR (Notifications)
- xUnit (Tests)

## Seed Data

On first run, the app automatically seeds:
- 1 Tenant (Default Shop)
- 2 Users (Super Admin, Shop Admin)
- 10 Roles
- 4 Categories (Electronics, Grocery, Clothing, Home & Kitchen)
- 4 Brands
- 6 Products
- 1 Warehouse with stock items
- 1 Sample Customer

## Configuration

Edit `src/StockManagement.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=stockmanagement.db"
  }
}
```

Switch to PostgreSQL/SQL Server by changing the EF Core provider in `StockManagement.Infrastructure.DependencyInjection.cs`.

## Health Check

GET `/health` returns healthy status with EF Core check.

## License

MIT
