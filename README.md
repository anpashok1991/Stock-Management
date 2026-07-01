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

### Manufacturing / Production Management
- **Bill of Materials (BOM)** - Create, edit, copy, activate, deactivate, and delete BOMs
  - BOM versioning support for future revisions
  - Raw material specifications with quantity, unit, wastage %, optional flag
  - BOM status management (Draft / Active / Inactive)
- **Manufacturing Orders** - Full production lifecycle management
  - Auto-generated manufacturing numbers (MFG-YYYY-XXXXXX)
  - Production status tracking (Draft → Planned → In Progress → Completed / Cancelled)
  - BOM auto-loading with material requirement calculation
  - Production quantity input with automatic material scaling
- **Stock Validation** - Pre-production inventory validation
  - Automatic stock availability check before manufacturing
  - Insufficient stock indicators with missing material list
  - Manufacturing blocked until sufficient stock available
- **Raw Material Consumption** - Automatic inventory deduction
  - Real-time stock deduction on manufacturing completion
  - Stock movement tracking for all consumed materials
  - Wastage percentage calculation for accurate consumption
- **Finished Product Stock** - Automatic stock increment
  - Finished product stock increased after successful manufacturing
  - Product StockQuantity updated automatically
- **Inventory Transactions** - Complete audit trail
  - Transaction types: ConsumedInManufacturing, Manufactured
  - Before/After quantity tracking for every stock movement
  - Manufacturing number linkage for traceability
- **Cost Calculation** - Automatic production cost computation
  - Total Raw Material Cost
  - Additional Manufacturing Cost (optional)
  - Labour Cost (optional)
  - Packaging Cost (optional)
  - = Finished Product Cost
- **Manufacturing History** - Complete production history with filtering
  - Search by manufacturing number, status, date range
  - View detailed material consumption breakdown
  - Material usage summary with cost breakdown
- **Multi-Shop Support** - Tenant-scoped manufacturing
  - Stock affected only within the manufacturing shop
  - Complete data isolation between tenants

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
