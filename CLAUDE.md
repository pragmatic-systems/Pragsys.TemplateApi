# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Template.TestedApi is a .NET 8.0 minimal API for managing a Todo list, following a CQRS pattern via MediatR. It authenticates users via JWT/OIDC (Keycloak or AWS Cognito) and persists data to PostgreSQL.

## Solution Structure

```
Template.TestedApi.sln
├── src/
│   ├── Template.TestedApi.Api/          -- ASP.NET Core minimal API entrypoint
│   │   ├── Program.cs                   -- App startup, DI, middleware pipeline
│   │   ├── ConfigurationExtensions.cs   -- All DI extension methods (Serilog, Postgres, MediatR, OIDC, Swagger, health checks, metrics)
│   │   ├── Controllers/TodoListController.cs -- REST endpoints (GET/POST) that delegate to MediatR
│   │   ├── Middleware/AuthMiddleware.cs -- JWT validation, AWS IAM claim mapping, auth skip rules
│   │   ├── Middleware/JwtValidators.cs  -- Custom audience validation supporting client_id fallback
│   │   ├── HostedServices/PostgresInitService.cs -- Runs DB migrations on startup
│   │   ├── Constants.cs (Roles)         -- Role constants: TodoList:Read, TodoList:Write
│   │   └── appsettings.json
│   ├── Template.TestedApi.Core/          -- Business logic layer
│   │   ├── Handlers/InsertTodoItemHandler.cs -- CQRS command handler + InsertTodo request record
│   │   ├── Handlers/SelectTodoItemsHandler.cs -- CQRS query handler + SelectTodo request record
│   │   └── Validators/InsertTodoValidator.cs -- FluentValidation rules for InsertTodo
│   └── Template.TestedApi.Database/      -- Data access layer
│       ├── ApplicationDbContext.cs      -- EF Core DbContext with Npgsql + snake_case naming
│       ├── Model/TodoRecord.cs          -- Domain record: ItemId, Title, Description, DueDate, Open, ClosedDate, Version
│       ├── Configurations/TodoRecordConfiguration.cs -- EF config: todo_list table, xmin as concurrency token (xid)
│       ├── Migrator.cs                  -- DbUp migration runner with Polly retry
│       └── Scripts/0001_Create_Tables.sql -- Embedded migration script
├── test/
│   ├── Template.TestedApi.UnitTests/    -- xUnit + FluentAssertions (validator tests)
│   ├── Template.TestedApi.IntegrationTests/ -- xUnit + WebApplicationFactory + Testcontainers (Postgres)
│   │   └── Infrastructure/
│   │       ├── TestRuntime.cs           -- Test fixture: spins up Postgres container, mocks OIDC, configures WebApplicationFactory
│   │       ├── TestContext.cs           -- Fluent test API: users, JWT auth, HTTP helpers with retry
│   │       └── Auth/                   -- Mock OIDC server: PEM cert, discovery doc, access token builder
│   └── Template.TestedApi.Benchmark/    -- Benchmark tests
└── docker-compose.infra.yml             -- Infrastructure docker compose
└── docker-compose.apps.yml              -- Application docker compose
```

## Architecture

### Request Flow

1. HTTP request hits `TodoListController` (e.g., GET /todolist or POST /todolist)
2. Controller delegates to `IMediator.Send()` with a request record (`SelectTodo` or `InsertTodo`)
3. MediatR routes to the corresponding handler (`SelectTodoItemsHandler` or `InsertTodoItemHandler`)
4. Handler uses `ApplicationDbContext` (EF Core) to query/insert into PostgreSQL
5. Response returns the `TodoRecord` or `IEnumerable<TodoRecord>`

### Auth Flow

- `AuthMiddleware` runs before authentication middleware, intercepting requests
- Validates JWT bearer token against OIDC provider (Keycloak/Cognito)
- Extracts roles from `scope` claim (prefix `todolist-permissions/`) and adds them as role claims
- Authorization policies (`TodoList:Read`, `TodoList:Write`) enforce role-based access on controller actions
- Auth is skipped for `/_system/*` paths and `/favicon.ico`
- Cognito support: `JwtValidators.ValidateAudienceOrClientId` falls back to `client_id` claim for audience validation

### Database

- PostgreSQL with EF Core, snake_case naming via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Migrations handled by DbUp from embedded SQL scripts in `src/Template.TestedApi.Database/Scripts/`
- Concurrency uses PostgreSQL `xmin` (xid type) as optimistic concurrency token
- `PostgresInitService` (IHostedService) runs migrations on app startup

### Observability

- Swagger UI at root path (development only)
- Prometheus metrics at `/_system/metrics`
- Health checks: `/_system/ping` (always healthy), `/_system/health` (full check including DB and OIDC)

## Commands

### Build
```
dotnet build Template.TestedApi.sln
```

### Run Unit Tests
```
dotnet test test/Template.TestedApi.UnitTests/Template.TestedApi.UnitTests.csproj
```

### Run Integration Tests
```
dotnet test test/Template.TestedApi.IntegrationTests/Template.TestedApi.IntegrationTests.csproj
```

### Run All Tests
```
dotnet test Template.TestedApi.sln
```

### Run the API
```
dotnet run --project src/Template.TestedApi.Api/Template.TestedApi.Api.csproj
```

## Adding New Features

- **New command/query**: Create a request record (`record Xyz : IRequest<T>`) and handler (`IRequestHandler<Xyz, T>`) in `Template.TestedApi.Core/Handlers/`. MediatR auto-discovers them.
- **New validator**: Create in `Template.TestedApi.Core/Validators/`. MediatR registers validators from all loaded assemblies automatically.
- **New endpoint**: Add to `TodoListController` or create a new controller. Use `[Authorize(Policy = Roles.Xxx)]` for role-based access.
- **New DB migration**: Add a new SQL script to `src/Template.TestedApi.Database/Scripts/` following the `NNNN_Description.sql` naming convention. DbUp runs them in order.
- **New auth role**: Add a constant to `Roles.cs` and a corresponding policy in `WithAuthorizationPolicy()`.
