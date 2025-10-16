# WolverineMultiTenantExample

A .NET 10 Worker Service project demonstrating technical challenges with WolverineFx in multi-tenant scenarios.

## Overview

This project implements a **tenant-per-database** pattern where:
- Each tenant has a separate SQLite database
- `ITenantContext` (scoped) maintains current tenant during message processing  
- `TenantDbContext` requires resolved tenant context for connection string
- `TenantContextMiddleware` sets tenant context from message metadata

## Problem #1: Dependency Resolution Timing

**Issue**: Generated handlers resolve dependencies before middleware execution, breaking tenant-scoped services.

**Example Non-Working Code**:
```csharp
public OrderHandler(
    ITenantContext tenantContext,  // Resolved before middleware sets tenant
    /* other dependencies */)
{
    // Dependencies resolved - tenant context is empty
    _tenantContext = tenantContext;
}

public override async Task HandleAsync(MessageContext context, CancellationToken cancellation)
{
    ...
}
```
**Impact**: `TenantDbContext` registration fails because tenant context is empty when dependencies are resolved.

**Current Workaround**: Use service locator pattern in handlers instead of constructor injection.
```csharp
public OrderHandler(IServiceProvider services /* other dependencies */)
{
    // Resolve tenant context at runtime, after middleware, gets tennant context correctly.
    _tenantContext = services.GetRequiredService<ITenantContext>();
}

public override async Task HandleAsync(MessageContext context, CancellationToken cancellation)
{
    ...
}
```
## Problem #2: Type-Specific Middleware Registration

**Issue**: `ForMessagesOfType<T>()` middleware registration appears non-functional.

**Non-Working Code**:
```csharp
// Has no effect
opts.Policies.ForMessagesOfType<ITenantScoped>().AddMiddleware<TenantContextMiddleware>();
```

**Working Workaround**:
```csharp
// Works but applies to ALL messages
opts.Policies.AddMiddleware<TenantContextMiddleware>();
```

## Problem #3: Durable Persistence in Fan-Out Scenarios

**Issue**: Unique constraint violations when multiple handlers process the same message type with durable inbox/outbox enabled.

**Scenario**: Multiple handlers listening to same message type from service bus topic via multiple subscriptions without filters.

## Project Structure

```
WolverineMultiTenantExample/
├── Data/
│   ├── TenantDbContext.cs          # Demonstrates timing issue
│   └── WolverineDbContext.cs       
├── Models/
│   └── MessageModels.cs            # ITenantScoped interface
├── Services/
│   └── TenantServices.cs           # Tenant management
├── GeneratedCode/                  # Generated handlers showing issues
├── MessageHandlers.cs              # Service locator workaround
├── TenantContextMiddleware.cs      # Middleware that runs too late
└── Program.cs                      # Configuration attempts
```