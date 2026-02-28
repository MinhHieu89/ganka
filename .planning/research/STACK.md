# Stack Research

**Domain:** Ophthalmology Clinic Management System (HIS + Pharmacy + Optical + Finance)
**Researched:** 2026-02-28
**Confidence:** HIGH (core stack user-decided; library versions verified via NuGet/npm)

---

## Recommended Stack

### Backend Core

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET 9 | 9.0.x (STS) | Runtime and SDK | User-decided. STS release supported until Nov 2026. Current patch: 9.0.4. EF Core 9 targets .NET 8+ so runs fine on .NET 9. |
| ASP.NET Core 9 | 9.0.x | Web API framework | Built-in to .NET 9. REST controllers + SignalR hubs. Native OpenAPI support (no Swashbuckle needed). |
| WolverineFx | 5.16.4 | Mediator, message bus, command/query/event handler | User-decided over MediatR. Built-in transactional outbox/inbox, SQL Server transport, EF Core integration, FluentValidation middleware. All-in-one: replaces MediatR + MassTransit + separate outbox libraries. Actively maintained (last release 2026-02-26). Supports .NET 8/9/10. |
| WolverineFx.SqlServer | 5.16.4 | SQL Server persistence + message transport | Provides durable inbox/outbox backed by SQL Server. No need for RabbitMQ or Azure Service Bus for a single-clinic system. |
| WolverineFx.EntityFrameworkCore | 5.16.4 | EF Core transactional middleware | Automatic SaveChangesAsync + outbox message flush in handlers. Maps envelope storage into EF Core model. |
| WolverineFx.Http | 5.16.4 | HTTP endpoint handlers (optional) | Alternative to controllers for thin endpoints. Can coexist with standard ASP.NET controllers. Use selectively for simple CRUD, keep controllers for complex endpoints. |
| WolverineFx.FluentValidation | 4.12.3 | Automatic validation in message handlers | Middleware that stops invalid messages before reaching handlers. Integrates with FluentValidation validators. NOTE: Version trails core Wolverine -- check for 5.x release. |
| EF Core 9 | 9.0.12 | ORM for SQL Server | User-decided. Latest patch 9.0.12 (2026-01-13). Supports schema-per-module via `HasDefaultSchema()`. Targets .NET 8+. |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.12 | SQL Server EF Core provider | Must match EF Core version exactly. All `Microsoft.EntityFrameworkCore.*` packages must be same version. |
| Microsoft.EntityFrameworkCore.Design | 9.0.12 | EF Core migrations tooling | Required for `dotnet ef migrations` commands. Dev dependency only. |
| FluentValidation | 12.1.1 | Command/query validation | Strongly-typed validation rules. Integrates with Wolverine middleware. Latest stable (2025-12-03). |
| FluentValidation.AspNetCore | 12.1.1 | ASP.NET Core integration | Auto-registration of validators. Model-state integration for controllers. |

### Authentication & Authorization

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.x | Identity persistence with EF Core | User-decided. Built-in to ASP.NET Core 9. Provides user/role/claim management backed by SQL Server. |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.x | JWT token validation middleware | User-decided. Part of ASP.NET Core shared framework. Validates access tokens on API requests. |
| System.IdentityModel.Tokens.Jwt | 8.16.0 | JWT token creation/parsing | Token generation for login endpoints. Supported through .NET 9 LTS lifetime (May 2026). |

### Database & Storage

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| SQL Server 2022 | Latest | Primary database | User-decided. Single DB with schema-per-module. Azure SQL for production, SQL Server Express/Developer for local dev. |
| Azure.Storage.Blobs | 12.27.0 | Medical image storage (Blob Storage) | User-decided. SAS tokens for secure, time-limited access. Cost-effective for large images (Fluorescein, Meibography, OCT). |
| Azure.Identity | Latest | Azure service authentication | DefaultAzureCredential for Blob Storage and other Azure services. Managed identity in production, developer credentials locally. |

### Real-Time Communication

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| ASP.NET Core SignalR | 9.0.x (built-in) | Server-side real-time hub | Built into ASP.NET Core 9. Used for appointment board updates, notification toasts, queue status. No separate NuGet needed. |
| @microsoft/signalr | 10.0.0 | JavaScript client for SignalR | npm package for frontend. Connects to ASP.NET Core SignalR hubs. Compatible with ASP.NET Core 9 server. |

### API Documentation

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Microsoft.AspNetCore.OpenApi | 9.0.x (built-in) | OpenAPI document generation | Built into .NET 9 templates. Replaces Swashbuckle (deprecated/unmaintained). First-party, actively maintained. |
| Scalar.AspNetCore | 2.12.50 | API reference UI | Beautiful, modern API docs UI. Replaces Swagger UI. MIT license, actively maintained (updated 2026-02-27). Supports .NET 8/9/10. |

### Logging & Observability

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Serilog | 4.3.0 | Structured logging framework | Industry standard for .NET structured logging. Enrichers, sinks, correlation IDs. |
| Serilog.AspNetCore | Latest | ASP.NET Core integration | Request logging, host builder integration, middleware pipeline logging. |
| Serilog.Sinks.Console | Latest | Console output | Dev environment logging. |
| Serilog.Sinks.MSSqlServer | 9.0.2 | SQL Server log sink | Write structured logs to SQL Server. Query logs alongside application data. Good for audit trail supplementation. |
| Serilog.Settings.Configuration | Latest | appsettings.json config | Configure sinks/enrichers via appsettings without code changes. |
| Serilog.Enrichers.Environment | Latest | Machine/process enrichers | Add machine name, process ID to log events. |
| Serilog.Enrichers.Thread | Latest | Thread enrichers | Add thread ID/name to log events. |

### Object Mapping

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Riok.Mapperly | 4.3.1 | DTO/Entity mapping via source generation | Zero runtime overhead -- generates mapping code at compile time. No reflection. Free and MIT licensed. Preferred over AutoMapper (now commercial) and Mapster (stalled maintenance). |

### PDF Generation & Printing

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| QuestPDF | 2026.2.2 | PDF generation (prescriptions, invoices, Rx) | Fluent C# API, pixel-perfect output, no dependencies. Generates prescriptions, optical Rx, invoices, consent forms, pharmacy labels. Free for revenue < $1M (Community License). |

### Excel Export

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| ClosedXML | 0.105.0 | Excel export for reports | Intuitive API over OpenXML. Generate revenue reports, data exports for research, anonymized patient data. .NET Standard 2.0 compatible. |

### Medical Domain Libraries

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| ICD-10 (self-hosted data) | 2026 dataset | Diagnosis code lookup | No good NuGet library exists. Import CMS ICD-10-CM dataset (CSV/XML) into SQL Server lookup table. ~98,100 codes. Build searchable index with Vietnamese translations. Updated annually (FY2026 effective Oct 2025). |
| Hl7.Fhir.R4 | 6.0.2 | HL7 FHIR data models (future-readiness) | NOT for MVP. Install only when So Y Te connection requires FHIR format. The Firely SDK provides R4 models. Keep architecture FHIR-ready but don't add dependency until needed. |
| fo-dicom | 5.2.5 | DICOM image handling (future) | NOT for MVP. Ophthalmology devices (OCT, slit lamp) may export DICOM. Architecture should support adding later. Don't install until device integration phase. |

### Testing Stack

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| xunit.v3 | 3.2.2 | Test framework | User-decided (xUnit). v3 is current (2026-01-14). Supports .NET 8+. All future features on v3 only. v2 is security-patches-only. |
| xunit.runner.visualstudio | 3.1.5 | VS Test Explorer integration | Required for running xUnit tests in Visual Studio and `dotnet test`. |
| Moq | 4.20.72 | Mocking library | User-decided. SponsorLink controversy resolved (removed in 4.20.2). Still most widely used .NET mocking library. Pin to >= 4.20.2 to avoid telemetry. |
| Testcontainers | 4.10.0 | Docker-based test infrastructure | Spin up real SQL Server containers for integration tests. No shared test DB, no cleanup issues. |
| Testcontainers.MsSql | 4.9.0 | SQL Server container module | Pre-configured SQL Server container. Auto-pulls mcr.microsoft.com/mssql/server image. |
| Respawn | 7.0.0 | Database cleanup between tests | Intelligent table truncation (respects FK order). Use with Testcontainers for fast integration tests. By Jimmy Bogard (same author as MediatR). |
| Bogus | 35.6.5 | Fake data generation | Generate realistic patient names, phone numbers, addresses for tests. Supports Vietnamese locale. No dependencies. |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.x | WebApplicationFactory for integration tests | In-memory test server. Test API endpoints without network. Built into ASP.NET Core. |
| Microsoft.Playwright | 1.58.0 | E2E browser testing | User-decided. Cross-browser testing (Chromium, Firefox, WebKit). Auto-wait, network interception. |
| Microsoft.Playwright.Xunit | 1.58.0 | Playwright + xUnit integration | Base classes for xUnit test lifecycle. Page-per-test isolation. |

### Guard Clauses & Domain Primitives

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Ardalis.GuardClauses | 5.0.0 | Domain input validation guards | `Guard.Against.NullOrEmpty()`, `Guard.Against.OutOfRange()`, etc. Clean domain entity constructors. Extensible for custom guards (e.g., `Guard.Against.InvalidPatientId()`). |

---

## Frontend Core

### Framework & Routing

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| @tanstack/react-start | 1.163.2 | Full-stack React framework (TanStack Start) | User-decided. Built on TanStack Router + Vite. SSR for public pages (booking, results), SPA for staff dashboard. Type-safe routing, server functions. NOTE: RC stage, v1.0 imminent. Package migrated from `@tanstack/start` (deprecated) to `@tanstack/react-start`. |
| @tanstack/react-router | 1.163.2 | Type-safe client routing | Core of TanStack Start. File-based routing, search param validation, route loaders. Keep versions in sync with @tanstack/react-start. |
| React | 19.x | UI library | Required by TanStack Start. React 19 is current stable. |
| Vite | 6.x | Build tool & dev server | Bundled with TanStack Start. Fast HMR, optimized builds. |

### UI Components & Styling

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| shadcn/ui (CLI) | 3.8.5 | Component library (copy-paste model) | User-decided. Not an npm dependency -- components are copied into your project via `npx shadcn@latest add`. Maia style, Stone base, Green theme per PROJECT.md. Uses Radix UI primitives underneath. |
| radix-ui | Latest unified | Accessible UI primitives | shadcn/ui February 2026 update: unified `radix-ui` package replaces individual `@radix-ui/react-*` packages. Follow new-york style. |
| tailwindcss | 4.2.1 | Utility-first CSS | Required by shadcn/ui. v4 is current (released early 2025). CSS-first config, no tailwind.config.js needed. |
| @tabler/icons-react | 3.37.1 | Icon library | User-decided (Tabler icons). 5900+ MIT icons. Tree-shakable ES modules. |
| Inter (Google Fonts) | Latest | Typography | User-decided. Clean, readable at small sizes. Important for medical data density. |

### Data Fetching & State

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| @tanstack/react-query | 5.90.21 | Server state management | Cache API responses, background refetching, optimistic updates. Pairs naturally with TanStack ecosystem. Handles appointment list refreshing, patient search caching. |
| @tanstack/react-query-devtools | 5.x | Query debugging in dev | Visual devtools for inspecting query cache. Dev dependency only. |
| zustand | 5.x | Client state management | Lightweight (1.16KB), hook-based, no Provider wrapper. Use for UI state: sidebar open/close, active tab, selected patient context. Don't use for server data (that's TanStack Query's job). |
| nuqs | 2.8.8 | URL search param state | Type-safe URL state like useState. Use for table filters, pagination, date ranges in reports. Supports TanStack Router. 6KB gzipped, zero dependencies. |

### Forms & Validation

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| @tanstack/react-form | Latest | Form state management | Type-safe form handling. Deep TanStack ecosystem integration. Handles complex medical forms (exam templates, prescriptions). Granular field-level validation. |
| zod | 4.3.6 | Schema validation | TypeScript-first validation. Shared schemas between frontend validation and API contracts. 2KB core bundle. Zero dependencies. |

### Tables & Data Display

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| @tanstack/react-table | 8.21.3 | Headless table logic | Sorting, filtering, pagination, column visibility. Use with shadcn/ui DataTable component. Essential for patient lists, pharmacy inventory, financial reports. |

### Charts & Reporting

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| recharts | 2.x | Chart rendering | Used by shadcn/ui chart components directly (not wrapped). Revenue dashboards, OSDI trend charts, treatment effectiveness graphs. D3-based. |

### Internationalization

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| react-i18next | 16.5.4 | Bilingual UI (Vietnamese/English) | Most mature React i18n solution. 7M weekly downloads. JSON translation files. Namespace support for module-level translations. Lazy loading of locale bundles. |
| i18next | Latest | i18n core engine | Required peer dependency of react-i18next. |

### Date & Time

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| date-fns | 4.1.0 | Date manipulation | Tree-shakable (only import what you use). Functional API. Locale support for Vietnamese date formatting. Preferred over dayjs for bundle size with tree-shaking. |

### Printing

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| react-to-print | Latest | Browser printing | Trigger print dialog for prescriptions, invoices, optical Rx. CSS @media print styles for layout control. Copies styles to print window. |

### Real-Time (Frontend)

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| @microsoft/signalr | 10.0.0 | SignalR client | Connect to ASP.NET Core hubs. Appointment board real-time updates, notification toasts when glasses are ready. |

---

## Notifications (Zalo OA)

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Zalo OA API v3 | v3 (REST) | Patient notifications | User-decided (direct API, no aggregator). REST API for sending template messages. Requires OA account creation + template approval. From Jan 2026, ZBS Template Message consolidates ZNS/OA messaging. |
| HttpClient (.NET) | Built-in | HTTP client for Zalo API | Use typed HttpClient with `IHttpClientFactory`. Polly for retry/circuit-breaker. No third-party Zalo SDK needed -- API is straightforward REST. |

**Zalo Integration Notes:**
- Zalo OA must be created and verified before API access
- Message templates require pre-approval by Zalo
- ZNS can send to phone numbers without requiring OA follow
- ZBS Template Message (effective Jan 2026) is the new unified messaging format
- OAuth 2.0 for access token management
- Build a thin `IZaloOAClient` abstraction for testability

---

## Infrastructure & DevOps

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Azure App Service | Basic/Standard | Web hosting | User-decided. $50-80/month budget. Start with Basic B1, scale to Standard S1 as traffic grows. |
| Azure SQL Database | Basic/Standard | Managed SQL Server | User-decided. Automatic backups, TDE encryption at rest. Start with Basic DTU tier. |
| Azure Blob Storage | Latest | Medical image storage | User-decided. Hot tier for recent images, Cool tier for >6 month old images (cost optimization). |
| Azure DevOps Pipelines | N/A | CI/CD | User-decided. YAML pipelines. Build, test, deploy stages. Environment approvals for production. |
| Docker | Latest | Local development + testing | Run SQL Server locally via Docker. Testcontainers uses Docker for integration tests. |

---

## Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| Visual Studio 2022 / Rider | .NET IDE | Full .NET 9 support. Rider for cross-platform dev. |
| VS Code | Frontend development | TanStack, TypeScript, Tailwind extensions. |
| Azure Data Studio | SQL Server management | Cross-platform, modern UI. Alternative to SSMS. |
| Docker Desktop | Container runtime | Required for Testcontainers. SQL Server dev instance. |
| dotnet ef CLI | EF Core migrations | `dotnet tool install --global dotnet-ef` |
| Azurite | Local Azure Storage emulator | Test Blob Storage locally without Azure subscription. |

---

## Installation Commands

### Backend (.NET)

```bash
# Create solution
dotnet new sln -n Ganka28

# Core Wolverine packages (all must be same version)
dotnet add package WolverineFx --version 5.16.4
dotnet add package WolverineFx.SqlServer --version 5.16.4
dotnet add package WolverineFx.EntityFrameworkCore --version 5.16.4
dotnet add package WolverineFx.Http --version 5.16.4
dotnet add package WolverineFx.FluentValidation --version 4.12.3

# EF Core (all must be same version)
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.12
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.12
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.12

# Auth
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.16.0

# Validation
dotnet add package FluentValidation --version 12.1.1
dotnet add package FluentValidation.AspNetCore --version 12.1.1

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.MSSqlServer --version 9.0.2
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread

# Mapping
dotnet add package Riok.Mapperly --version 4.3.1

# PDF & Excel
dotnet add package QuestPDF --version 2026.2.2
dotnet add package ClosedXML --version 0.105.0

# Azure Storage
dotnet add package Azure.Storage.Blobs --version 12.27.0
dotnet add package Azure.Identity

# API Docs
dotnet add package Scalar.AspNetCore --version 2.12.50

# Guards
dotnet add package Ardalis.GuardClauses --version 5.0.0

# Testing
dotnet add package xunit.v3 --version 3.2.2
dotnet add package xunit.runner.visualstudio --version 3.1.5
dotnet add package Moq --version 4.20.72
dotnet add package Testcontainers --version 4.10.0
dotnet add package Testcontainers.MsSql --version 4.9.0
dotnet add package Respawn --version 7.0.0
dotnet add package Bogus --version 35.6.5
dotnet add package Microsoft.Playwright --version 1.58.0
dotnet add package Microsoft.Playwright.Xunit --version 1.58.0
```

### Frontend (npm)

```bash
# Create TanStack Start project
npm create @tanstack/start@latest

# Core framework (already included by create command)
npm install @tanstack/react-start @tanstack/react-router

# UI Components
npx shadcn@latest init
# Select: Maia style, Stone base, Green accent, no border radius
npm install tailwindcss @tabler/icons-react

# Data fetching & state
npm install @tanstack/react-query zustand nuqs

# Forms & validation
npm install @tanstack/react-form zod

# Tables
npm install @tanstack/react-table

# Charts (used by shadcn/ui chart component)
npm install recharts

# i18n
npm install react-i18next i18next

# Date utilities
npm install date-fns

# Printing
npm install react-to-print

# SignalR client
npm install @microsoft/signalr

# Dev dependencies
npm install -D @tanstack/react-query-devtools @tanstack/router-devtools
npm install -D @playwright/test
```

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not Alternative |
|----------|-------------|-------------|---------------------|
| Mediator/Bus | WolverineFx | MediatR | User-decided. Wolverine has built-in outbox, SQL Server transport, async support. MediatR is sync-only mediator -- would need MassTransit for messaging. |
| Mocking | Moq | NSubstitute | User-decided Moq. NSubstitute has cleaner syntax but Moq is specified in constraints. Pin to >= 4.20.2 to avoid SponsorLink telemetry. If team reconsiders, NSubstitute is excellent. |
| Object Mapping | Mapperly | AutoMapper | AutoMapper became commercial (April 2025). Mapperly is source-generated (zero reflection), free, and faster. |
| Object Mapping | Mapperly | Mapster | Mapster has stalled development/uncertain maintenance. Mapperly is actively maintained with frequent releases. |
| API Docs | Scalar | Swashbuckle | Swashbuckle is deprecated/unmaintained since .NET 9. Removed from templates. Scalar is the modern replacement with better UI. |
| API Docs | Scalar | NSwag | NSwag is heavier (client generation bundled). Scalar is focused on docs UI. Use Microsoft.AspNetCore.OpenApi for doc generation + Scalar for UI. |
| Database | SQL Server | PostgreSQL | User-decided SQL Server. Best EF Core support, Azure SQL available, team familiarity. PostgreSQL would mean losing Wolverine SQL Server transport (would need PostgreSQL transport instead). |
| State Management | Zustand | Redux Toolkit | Overkill for ~8 concurrent users. Zustand is simpler, smaller, sufficient for UI-only state. Server state handled by TanStack Query. |
| State Management | Zustand | Jotai | Jotai's atomic model is better for complex interdependent state. Clinic dashboard has simpler UI state needs. Zustand's centralized stores are easier to reason about here. |
| Forms | @tanstack/react-form | React Hook Form | Both are excellent. TanStack Form chosen for ecosystem consistency and stronger TypeScript type safety on nested medical form fields. RHF would also work fine. |
| i18n | react-i18next | Paraglide | Paraglide has TanStack Start support but smaller ecosystem. react-i18next is battle-tested with 7M weekly downloads, extensive locale support, lazy loading. |
| PDF | QuestPDF | iTextSharp | iTextSharp has complex AGPL licensing. QuestPDF Community License is free for revenue < $1M, covers clinic use case. |
| Excel | ClosedXML | EPPlus | EPPlus requires commercial license since v5. ClosedXML is free (MIT). |
| Charts | Recharts (via shadcn) | Chart.js | shadcn/ui ships chart components built on Recharts. Using Chart.js would mean custom components outside shadcn ecosystem. |
| Test Framework | xUnit v3 | NUnit / MSTest | User-decided xUnit. v3 is latest with active development. Good Wolverine test support. |
| Frontend Framework | TanStack Start | Next.js | User-decided. TanStack Start pairs with .NET backend without needing Node.js SSR server for staff SPA. Vite-based, type-safe routing. |

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Swashbuckle.AspNetCore | Deprecated, unmaintained, removed from .NET 9 templates. No official .NET 8+ release. | Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore |
| AutoMapper | Became commercial (paid license) since April 2025. | Riok.Mapperly (source-generated, free, faster) |
| MediatR | User explicitly chose Wolverine. MediatR is sync-only mediator without outbox/messaging. Would need MassTransit separately. | WolverineFx (all-in-one mediator + messaging + outbox) |
| Moq < 4.20.2 | Versions 4.20.0-4.20.1 contained SponsorLink telemetry that exfiltrated developer emails. | Moq >= 4.20.72 (SponsorLink removed) |
| @tanstack/start (npm) | Deprecated npm package name. Last published 9 months ago (v1.120.20). | @tanstack/react-start (v1.163.2, actively maintained) |
| moment.js | Deprecated by maintainers, massive bundle size (300KB+). | date-fns (tree-shakable) |
| Redux | Overkill for small team app (~8 users). Excessive boilerplate. | Zustand (client state) + TanStack Query (server state) |
| Hangfire | Heavyweight background job processor. Wolverine handles durable background processing natively. | WolverineFx durable local queues |
| EPPlus v5+ | Commercial license required since v5. | ClosedXML (MIT license) |
| iTextSharp | Complex AGPL licensing. | QuestPDF (Community License, free for < $1M revenue) |
| System.Data.SqlClient | Deprecated. No longer maintained by Microsoft. | Microsoft.Data.SqlClient (used by EF Core 9 internally) |
| @aspnet/signalr (npm) | Old package name. Deprecated. | @microsoft/signalr |
| react-query (npm) | Old package name (v3 era). | @tanstack/react-query |
| Third-party Zalo SDK | Adds unnecessary dependency. Zalo OA API is simple REST. | Direct HttpClient with IHttpClientFactory + typed client |

---

## Version Compatibility Matrix

| Package A | Must Match With | Notes |
|-----------|-----------------|-------|
| WolverineFx 5.16.4 | WolverineFx.SqlServer 5.16.4, WolverineFx.EntityFrameworkCore 5.16.4, WolverineFx.Http 5.16.4 | All WolverineFx.* core packages should be same version. WolverineFx.FluentValidation is at 4.12.3 (different release cadence). |
| EF Core 9.0.12 | Microsoft.EntityFrameworkCore.SqlServer 9.0.12, Microsoft.EntityFrameworkCore.Design 9.0.12 | ALL Microsoft.EntityFrameworkCore.* packages must be exact same version. Mixing versions causes runtime errors. |
| .NET 9.0.x | EF Core 9.0.x, ASP.NET Core 9.0.x | EF Core 9 targets .NET 8+ so runs on .NET 9. All Microsoft.AspNetCore.* packages come from shared framework. |
| @tanstack/react-start 1.163.x | @tanstack/react-router 1.163.x | Keep TanStack Start and Router versions in sync. Updated frequently (daily releases). Use `^` version range. |
| @tanstack/react-query 5.x | @tanstack/react-query-devtools 5.x | Devtools must match major version of react-query. |
| tailwindcss 4.x | shadcn/ui CLI 3.x | shadcn/ui components expect Tailwind CSS v4. v4 uses CSS-based config (not tailwind.config.js). |
| xunit.v3 3.2.x | xunit.runner.visualstudio 3.x | Runner must be v3 compatible for xUnit v3 tests. |
| Testcontainers 4.10.0 | Testcontainers.MsSql 4.9.0 | Minor version can differ; they are separate packages from same project. |
| Playwright 1.58.0 | Microsoft.Playwright.Xunit 1.58.0 | Keep Playwright packages at same version. |
| Moq 4.20.72 | xunit.v3 3.2.2 | Compatible. No known issues. |
| FluentValidation 12.1.1 | WolverineFx.FluentValidation 4.12.3 | WolverineFx.FluentValidation depends on FluentValidation. Check compatible range in NuGet dependencies. |

---

## Stack Patterns by Module

**Patient Registration / HIS Module:**
- EF Core + SQL Server schema `his`
- Wolverine commands for patient CRUD
- FluentValidation for patient data validation
- Azure Blob Storage for medical images (SAS token URLs)
- ICD-10 lookup table (seeded from CMS dataset)

**Appointment Module:**
- EF Core + schema `scheduling`
- SignalR hub for real-time appointment board
- Wolverine domain events for cross-module notifications (e.g., AppointmentCreated -> notify Zalo)

**Pharmacy Module:**
- EF Core + schema `pharmacy`
- Wolverine domain events for stock alerts
- QuestPDF for prescription labels

**Optical Center Module:**
- EF Core + schema `optical`
- Barcode generation (consider ZXing.Net or similar)
- Wolverine events for order status changes -> SignalR push

**Finance/Billing Module:**
- EF Core + schema `finance`
- Full CQRS with denormalized read models for reporting
- ClosedXML for Excel exports
- Recharts on frontend for revenue dashboards

**Notification Module:**
- Wolverine durable local queues for reliable delivery
- Typed HttpClient for Zalo OA API
- Wolverine outbox ensures notifications survive crashes

**Reporting Module (Full CQRS):**
- Separate read-optimized DbContext
- Denormalized views/tables built via domain event handlers
- ClosedXML for data export
- QuestPDF for PDF reports

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Backend Core (.NET 9, EF Core, ASP.NET) | HIGH | Verified versions on NuGet. Standard well-documented stack. |
| Wolverine FX | HIGH | Verified v5.16.4 on NuGet (2026-02-26). Actively maintained. SQL Server transport and EF Core integration confirmed. |
| Frontend (TanStack Start) | MEDIUM | RC stage, not yet v1.0 stable. Actively developed with daily releases. Package name migration (@tanstack/start -> @tanstack/react-start) may cause initial confusion. API considered stable per RC announcement. |
| shadcn/ui + Tailwind v4 | HIGH | Verified. Feb 2026 update to unified radix-ui package. Well-documented component patterns. |
| Testing Stack | HIGH | All packages verified on NuGet/npm. Standard patterns. xUnit v3, Testcontainers, Playwright all actively maintained. |
| Zalo OA Integration | MEDIUM | REST API documented at developers.zalo.me. ZBS Template (Jan 2026) is new -- verify current API surface when implementing. No .NET SDK exists; direct HTTP calls needed. |
| Medical Libraries (ICD-10, FHIR, DICOM) | MEDIUM | ICD-10 is self-hosted data (no NuGet library). FHIR SDK exists but not needed for MVP. DICOM library exists but not needed until device integration. |
| Azure Infrastructure | HIGH | Standard Azure services. Well-documented. Budget-appropriate tiers identified. |

---

## Sources

**NuGet (package versions verified):**
- [WolverineFx 5.16.4](https://www.nuget.org/packages/WolverineFx/5.16.4)
- [WolverineFx.SqlServer 5.16.4](https://www.nuget.org/packages/WolverineFx.SqlServer)
- [WolverineFx.EntityFrameworkCore 5.16.4](https://www.nuget.org/packages/WolverineFx.EntityFrameworkCore)
- [WolverineFx.Http 5.16.4](https://www.nuget.org/packages/WolverineFx.Http)
- [WolverineFx.FluentValidation 4.12.3](https://www.nuget.org/packages/WolverineFx.FluentValidation)
- [Microsoft.EntityFrameworkCore 9.0.12](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/9.0.12)
- [FluentValidation 12.1.1](https://www.nuget.org/packages/FluentValidation)
- [Serilog 4.3.0](https://www.nuget.org/packages/Serilog)
- [QuestPDF 2026.2.2](https://www.nuget.org/packages/QuestPDF)
- [ClosedXML 0.105.0](https://www.nuget.org/packages/ClosedXML)
- [Riok.Mapperly 4.3.1](https://www.nuget.org/packages/Riok.Mapperly)
- [Scalar.AspNetCore 2.12.50](https://www.nuget.org/packages/Scalar.AspNetCore)
- [Azure.Storage.Blobs 12.27.0](https://www.nuget.org/packages/Azure.Storage.Blobs)
- [Testcontainers.MsSql 4.9.0](https://www.nuget.org/packages/Testcontainers.MsSql)
- [Respawn 7.0.0](https://www.nuget.org/packages/Respawn)
- [Bogus 35.6.5](https://www.nuget.org/packages/Bogus)
- [Microsoft.Playwright 1.58.0](https://www.nuget.org/packages/Microsoft.Playwright)
- [xunit.v3 3.2.2](https://www.nuget.org/packages/xunit.v3)
- [Ardalis.GuardClauses 5.0.0](https://www.nuget.org/packages/Ardalis.GuardClauses)

**npm (package versions verified):**
- [@tanstack/react-start](https://www.npmjs.com/package/@tanstack/react-start) - v1.163.2
- [@tanstack/react-query](https://www.npmjs.com/package/@tanstack/react-query) - v5.90.21
- [@tanstack/react-table](https://www.npmjs.com/package/@tanstack/react-table) - v8.21.3
- [@tabler/icons-react](https://www.npmjs.com/package/@tabler/icons-react) - v3.37.1
- [@microsoft/signalr](https://www.npmjs.com/package/@microsoft/signalr) - v10.0.0
- [tailwindcss](https://www.npmjs.com/package/tailwindcss) - v4.2.1
- [zod](https://www.npmjs.com/package/zod) - v4.3.6
- [react-i18next](https://www.npmjs.com/package/react-i18next) - v16.5.4
- [date-fns](https://www.npmjs.com/package/date-fns) - v4.1.0
- [nuqs](https://www.npmjs.com/package/nuqs) - v2.8.8
- [recharts](https://www.npmjs.com/package/recharts) - v2.x

**Official Documentation:**
- [Wolverine SQL Server Integration](https://wolverinefx.net/guide/durability/sqlserver)
- [Wolverine EF Core Outbox/Inbox](https://wolverinefx.net/guide/durability/efcore/outbox-and-inbox)
- [Wolverine FluentValidation Middleware](https://wolverinefx.net/guide/handlers/fluent-validation)
- [TanStack Start v1 RC Announcement](https://tanstack.com/blog/announcing-tanstack-start-v1)
- [shadcn/ui February 2026 Changelog](https://ui.shadcn.com/docs/changelog/2026-02-radix-ui)
- [.NET 9 OpenAPI (Swashbuckle replacement)](https://github.com/dotnet/aspnetcore/issues/54599)
- [EF Core 9 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew)
- [Zalo OA API Documentation](https://developers.zalo.me/docs/api/official-account-api-230)
- [ICD-10-CM 2026 Codes](https://www.icd10data.com/)
- [Hl7.Fhir.R4 SDK](https://fire.ly/products/firely-net-sdk/)

**Community / Blog Sources (MEDIUM confidence):**
- [Jeremy D. Miller - Wolverine + EF Core + SQL Server](https://jeremydmiller.com/2023/01/10/wolverine-meets-ef-core-and-sql-server/)
- [Jeremy D. Miller - Transactional Outbox](https://jeremydmiller.com/2024/12/08/build-resilient-systems-with-wolverines-transactional-outbox/)
- [AutoMapper Commercial Announcement](https://medium.com/@dino.cosic/automapper-is-now-commercial-should-net-developers-switch-to-mapster-25445581d38c)
- [Moq SponsorLink Controversy](https://checkmarx.com/blog/popular-nuget-package-moq-silently-exfiltrates-user-data-to-cloud-service/)

---
*Stack research for: Ganka28 Ophthalmology Clinic Management System*
*Researched: 2026-02-28*
