# Inovatiqa

A full-featured ASP.NET Core e-commerce platform for **Inovatiqa Corp** — a US-based online store (Stafford, TX). Built on a custom MVC architecture with Clean Architecture influences, it provides a complete storefront, admin panel, Elasticsearch-powered search, FedEx shipping integration, and Square payment processing.

**Production URL:** https://app-inovatiqa-prod-01.azurewebsites.net

---

## ⚠️ Security Notice

> **Critical:** Live Elasticsearch credentials (`ElasticPassword`) and an encryption key (`EncryptionKey`) are currently hardcoded in `Inovatiqa.Core/InovatiqaDefaults.cs` and committed to this public repository. **These must be rotated immediately and moved to environment variables or Azure Key Vault.** See the [Configuration](#configuration) section for the correct approach.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Customer Roles & Tiers](#customer-roles--tiers)
- [Integrations](#integrations)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Deployment Notes](#deployment-notes)
- [Admin Panel](#admin-panel)
- [Background Tasks](#background-tasks)

---

## Overview

Inovatiqa is a B2C and B2B e-commerce platform with support for tiered customer pricing, multi-vendor management, FedEx real-time shipping rates, Square payment processing, and Elasticsearch full-text product search. The platform is deployed to Azure App Service and uses Azure Elastic Cloud for search.

---

## Architecture

The solution is split into four projects with layered dependencies:

```
┌──────────────────────────────────────────────────────┐
│                  Inovatiqa.Web                       │
│   MVC Controllers, Views, Admin Area, Startup, DI   │
├──────────────────────────────────────────────────────┤
│                Inovatiqa.Services                    │
│  Business logic: Catalog, Orders, Shipping, Auth,    │
│  Payments, Email, SEO, Tasks, Vendors, etc.          │
├──────────────────────────────────────────────────────┤
│                Inovatiqa.Database                    │
│  EF Core DbContext, Repository pattern, Models       │
├──────────────────────────────────────────────────────┤
│                  Inovatiqa.Core                      │
│  Domain enums, interfaces, helpers, defaults,        │
│  caching abstractions, file provider                 │
└──────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (MVC + Razor Pages) |
| Language | C# |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Search | Elasticsearch (NEST client) via Azure Elastic Cloud |
| Caching | In-Memory Cache (`IStaticCacheManager`) |
| Auth | Cookie-based authentication + custom permission system |
| Payments | Square Payment Gateway |
| Shipping | FedEx Web Services (real-time rates) |
| Media | DB-stored images with on-the-fly thumbnail generation |
| Email | SMTP via queued email system |
| Hosting | Azure App Service |
| Monitoring | Azure Application Insights |
| PDF | Custom PDF invoice generation (Roboto font) |
| Mapping | AutoMapper |
| Session | ASP.NET Core distributed session |

---

## Project Structure

```
Inovatiqa/
│
├── Inovatiqa.Core/                     # Domain layer — no external dependencies
│   ├── Caching/                        # Cache key definitions, IStaticCacheManager
│   ├── Directory/                      # Currency/exchange rate models
│   ├── Http/Extensions/                # Session extensions
│   ├── Interfaces/                     # IPagedList, IWebHelper, IPriceFormatter, etc.
│   ├── Rss/                            # RSS feed models
│   ├── InovatiqaDefaults.cs            # ⚠️ Central constants (see Security Notice)
│   ├── InovatiqaFileProvider.cs        # Custom file provider
│   ├── PriceFormatter.cs               # Price display formatting
│   ├── WebHelper.cs                    # HTTP/request helpers
│   └── [Domain enums]                  # OrderStatus, PaymentStatus, ShippingStatus, etc.
│
├── Inovatiqa.Database/                 # Data access layer
│   ├── DbContexts/                     # EF Core InovatiqaContext
│   ├── Extensions/                     # IQueryable extensions
│   ├── Interfaces/                     # IRepository<T>
│   ├── Models/                         # 80+ EF Core entity classes
│   └── Repository.cs                   # Generic repository implementation
│
├── Inovatiqa.Services/                 # Business logic layer
│   ├── Authentication/                 # Cookie auth service
│   ├── Caching/                        # Cache-aware service decorators
│   ├── Catalog/                        # Products, categories, manufacturers, attributes
│   ├── Common/                         # Generic attributes, address service
│   ├── Customers/                      # Customer registration, roles, attributes
│   ├── Directory/                      # Countries, states, currencies
│   ├── Discounts/                      # Discount rules and calculations
│   ├── Events/                         # Domain event publishing
│   ├── Helpers/                        # Date/time helpers
│   ├── Logging/                        # Activity log service
│   ├── Media/                          # Picture service (store/retrieve images)
│   ├── Messages/                       # Email queuing and template system
│   ├── News/                           # News articles service
│   ├── Orders/                         # Order processing, shopping cart, checkout
│   ├── Payments/                       # Square payment integration
│   ├── Security/                       # Permission service, encryption
│   ├── Seo/                            # URL record / slug service
│   ├── Settings/                       # Dynamic settings service
│   ├── Shipping/                       # FedEx rate calculator, shipping methods
│   ├── Tasks/                          # Background task services
│   ├── Topics/                         # CMS topic/page service
│   ├── Vendors/                        # Multi-vendor support
│   └── WorkContext/                    # Current customer/store/language context
│
└── Inovatiqa.Web/                      # Presentation layer
    ├── Areas/
    │   └── Admin/                      # Full admin panel (Controllers, Views, Models)
    ├── Controllers/                    # Public storefront controllers
    ├── Components/                     # View components
    ├── Factories/                      # Model factory pattern (build view models)
    ├── Framework/                      # MVC filters, model binders
    ├── Models/                         # View models
    ├── Views/                          # Razor views (Catalog, Checkout, Customer, etc.)
    ├── Routing/                        # Custom SEO-friendly slug routing
    ├── TagHelpers/                     # Custom Razor tag helpers
    ├── UI/                             # UI utilities
    ├── sqlscripts/                     # SQL migration/seed scripts
    ├── ElasticSearchExtensions.cs      # Elasticsearch DI setup and index creation
    ├── InovatiqaSettings.cs            # Elasticsearch model settings
    ├── Startup.cs                      # DI configuration and middleware pipeline
    ├── Program.cs                      # App entry point
    └── appsettings.json                # Connection strings and logging config
```

---

## Features

### Storefront
- 🛍️ Full product catalog with categories, manufacturers, and specification attributes
- 🔍 Elasticsearch-powered product search with autocomplete
- 🛒 Shopping cart and wishlist
- 💳 One-page checkout with Square payment
- 📦 FedEx real-time shipping rate calculation
- ⭐ Product reviews and ratings (requires purchase)
- 🔄 Return requests
- 📰 News / blog section
- 📄 CMS topics (static pages)
- 🏷️ Discount codes and gift cards
- 🏆 Reward points system
- 🆚 Product comparison
- 🕐 Recently viewed products
- 📱 RSS feeds

### Customer Account
- Registration with address, phone, and company fields
- Order history and re-ordering
- Return request management
- Wishlist management
- Review history
- Tiered pricing based on customer role

### Admin Panel (`/Admin`)
- Product management (CRUD, attributes, images, tier pricing, inventory)
- Category and manufacturer management
- Order management and processing
- Customer management and role assignment
- Vendor management
- Return request handling
- Discount management
- Slider/banner management
- Activity log and audit trail
- Online customer monitoring
- Reports (bestsellers, order averages, search terms)
- Site settings management

---

## Customer Roles & Tiers

The platform supports a multi-tier B2B and retail pricing model:

| Role | Description |
|---|---|
| `Guests` | Unauthenticated visitors |
| `Registered` | Standard registered customer |
| `Retail` | Retail tier pricing |
| `Bronze` | Bronze loyalty tier |
| `Bronze Premier` | Bronze Premier tier |
| `Gold` | Gold loyalty tier |
| `Gold Premier` | Gold Premier tier |
| `Onyx` | Onyx loyalty tier |
| `Onyx Premier` | Onyx Premier tier |
| `Diamond` | Diamond loyalty tier |
| `Diamond Premier` | Diamond Premier tier |
| `Distributor` | Distributor pricing tier |
| `Distributor Premier` | Distributor Premier tier |
| `B2B` | Business-to-business customers |
| `PO` | Purchase order customers |
| `Vendors` | Multi-vendor sellers |
| `Administrators` | Full admin access |

Tier pricing is applied automatically at checkout based on the customer's assigned role.

---

## Integrations

### Square Payments
- Production and sandbox modes configurable via `InovatiqaDefaults`
- Supports authorize-only and immediate charge flows
- 3DS authentication supported
- Recurring payment token storage per customer

### FedEx Shipping
- Real-time rate lookup via FedEx Web Services SOAP API
- Supported services: FEDEX_GROUND, PRIORITY_OVERNIGHT, STANDARD_OVERNIGHT, FEDEX_2_DAY, FEDEX_2_DAY_AM, FEDEX_EXPRESS_SAVER, FEDEX_1_DAY_FREIGHT, FEDEX_2_DAY_FREIGHT, FEDEX_3_DAY_FREIGHT, FIRST_OVERNIGHT
- Dimensions-based packing (by dimensions, one item per package, or volume)
- Free shipping over $150 (configurable)
- Max package weight: 150 lb

### Elasticsearch (Azure Elastic Cloud)
- Full-text product search and autocomplete
- Index: `inovatiqa`
- Endpoint: Azure Elastic Cloud (Central US region)
- Connected via NEST (official .NET Elasticsearch client)

### Azure Application Insights
- Telemetry and performance monitoring
- Configure via `InstrumentationKey` in `appsettings.json`

---

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) >= 5.0
- SQL Server (LocalDB, Express, or Azure SQL)
- An Elasticsearch instance (Azure Elastic Cloud or local)
- Visual Studio 2022 or VS Code with C# Dev Kit

### Clone & Setup

```bash
git clone https://github.com/aligenius-acme/Inovatiqa.git
cd Inovatiqa
```

1. Open `Inovatiqa.sln` in Visual Studio
2. Configure `appsettings.json` (see [Configuration](#configuration))
3. Move all secrets from `InovatiqaDefaults.cs` to environment variables / Azure Key Vault
4. Run the SQL scripts in `Inovatiqa.Web/sqlscripts/` to set up the database
5. Set `Inovatiqa.Web` as the startup project
6. Press F5 to run

---

## Configuration

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "InovatiqaConnection": "Server=.;Database=InovatiqaDb;Trusted_Connection=True;"
  },
  "Logging": {
    "ApplicationInsights": {
      "InstrumentationKey": "YOUR_APPINSIGHTS_KEY"
    }
  }
}
```

### Environment Variables (recommended for all secrets)

Move these out of `InovatiqaDefaults.cs` and configure as environment variables or Azure Key Vault secrets:

| Variable | Description |
|---|---|
| `Elastic__EndPoint` | Elasticsearch cluster URL |
| `Elastic__Username` | Elasticsearch username |
| `Elastic__Password` | Elasticsearch password ⚠️ **Rotate immediately** |
| `Encryption__Key` | App-level encryption key ⚠️ **Rotate immediately** |
| `FedEx__Key` | FedEx API key |
| `FedEx__Password` | FedEx API password |
| `FedEx__AccountNumber` | FedEx account number |
| `FedEx__MeterNumber` | FedEx meter number |
| `Square__ApplicationId` | Square application ID |
| `Square__AccessToken` | Square access token |
| `Square__LocationId` | Square location ID |
| `Captcha__SiteKey` | Google reCAPTCHA site key |
| `Captcha__PrivateKey` | Google reCAPTCHA private key |

> In Azure App Service, set these under **Configuration → Application Settings**.

---

## Deployment Notes

From `Depoyment Notes.txt` — required after each fresh deployment:

1. **Store URL:** Verify `InovatiqaDefaults.StoreUrl` matches your deployment domain
2. **Shipping origin:** Query the `Settings` table for `shippingsettings.shippingoriginaddressid` and update `InovatiqaDefaults.ShippingOriginAddressId` to match the ID in your database
3. **Company info:** Confirm the company details in `InovatiqaDefaults` match records in the database:
   - Address: 12815 Capricorn St, Stafford, TX 77477, United States
   - Phone: (346) 229-4142
   - Fax: 281-220-1350
4. **Email accounts:** Update SMTP settings in the `EmailAccount` table for the account responsible for sending transactional emails

---

## Admin Panel

The admin area is accessible at `/Admin` (Administrators role required).

| Section | URL | Description |
|---|---|---|
| Dashboard | `/Admin` | Sales overview |
| Products | `/Admin/Product` | Product CRUD and inventory |
| Categories | `/Admin/Category` | Category hierarchy |
| Manufacturers | `/Admin/Manufacturer` | Brand/manufacturer management |
| Orders | `/Admin/Order` | Order processing and history |
| Customers | `/Admin/Customer` | Customer management |
| Customer Roles | `/Admin/CustomerRole` | Role and permission management |
| Vendors | `/Admin/Vendor` | Multi-vendor management |
| Return Requests | `/Admin/ReturnRequest` | Process customer returns |
| Discounts | `/Admin/Discount` (via settings) | Discount code management |
| Sliders | `/Admin/Slider` | Homepage banner/slider management |
| Reports | `/Admin/Report` | Bestsellers, order averages |
| Activity Log | `/Admin/ActivityLog` | Admin action audit trail |
| Online Customers | `/Admin/OnlineCustomer` | Real-time visitor monitoring |
| Security | `/Admin/Security` | Permission matrix |
| Settings | `/Admin/Preferences` | Site-wide settings |

---

## Background Tasks

The platform runs several background services via hosted `BackgroundTaskService` implementations:

| Task | Description |
|---|---|
| `EmailSenderTaskService` | Processes the queued email queue and sends via SMTP |
| `DeleteGuestsTaskService` | Cleans up guest customer records older than 2000 minutes |
| `UpdateRolesForEachCategoryTaskService` | Syncs role-based category access |
| `UpdateEntityCounterTaskService` | Refreshes entity count caches |
| `UpdateRootCategoryIdsForProductsTaskService` | Maintains root category ID denormalisation |
| `AdServiceTaskService` | Ad tracking/affiliate data sync |
| `AdTractionTaskService` | AdTraction affiliate network integration |
| `FinansPortalenTaskService` | Finansportalen data integration |
| `QredTaskService` | Qred business loan integration |
| `SearchIndexTaskService` | Elasticsearch index synchronisation |

---

## License

Private repository. All rights reserved by [aligenius-acme](https://github.com/aligenius-acme).
