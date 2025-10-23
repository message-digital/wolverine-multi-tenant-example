using WolverineMultiTenantExample.Data;
using WolverineMultiTenantExample.Models;
using Microsoft.EntityFrameworkCore;

namespace WolverineMultiTenantExample;

public class OrderHandler
{
    private readonly Tenant _tenantContext;
    private readonly TenantDbContext _dbContext;
    private readonly ISimpleTenantScoped _simpleTenantScoped;
    private readonly ILogger<OrderHandler> _logger;

    public OrderHandler(
        Tenant tenant,
        ITenantServiceFactory<TenantDbContext> dbContextFactory,
        ITenantServiceFactory<ISimpleTenantScoped> tenantServiceFactory,
        ILogger<OrderHandler> logger)
    {
        _tenantContext = tenant;
        // QUESTION: Typically we would inject TenantDbContext directly but due to how dependencies are resolved in generated code we cannot
        // and must resolve it now or inject a factory. Is this acceptable?
        _dbContext = dbContextFactory.GetService(tenant);
        _simpleTenantScoped = tenantServiceFactory.GetService(tenant);
        _logger = logger;
    }

    public async Task Handle(ProcessOrderMessage message)
    {
        var tenantId = _tenantContext.Id;

        _simpleTenantScoped.DisplayTenantInfo();
        // Create and save order to tenant-specific database
        var order = new Order
        {
            Id = message.OrderId,
            TenantId = tenantId!,
            Amount = message.Amount,
            CustomerName = message.CustomerName,
            CreatedAt = message.CreatedAt,
            Status = "Processing"
        };

        _dbContext.Orders.Add(order);
        
        try
        {
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation(
                "Saved order {OrderId} to tenant database for tenant {TenantId}",
                order.Id,
                tenantId);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Log and ignore duplicate key violations - message was already processed
            _logger.LogWarning(
                "Order {OrderId} already exists for tenant {TenantId} - ignoring duplicate message",
                order.Id,
                tenantId);
        }
    }
    
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Check for common unique constraint violation error messages
        var errorMessage = ex.InnerException?.Message ?? ex.Message;
        return errorMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("primary key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("violation of primary key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}

public class EmailHandler
{
    private readonly Tenant _tenant;
    private readonly TenantDbContext _dbContext;
    private readonly ILogger<EmailHandler> _logger;

    public EmailHandler(
        Tenant tenant,
        ITenantServiceFactory<TenantDbContext> dbContextFactory,
        ILogger<EmailHandler> logger)
    {
        // QUESTION: Due to how dependencies are resolved in generated code we cannot inject TenantDbContext directly
        // and must resolve it now or inject a factory. Is this acceptable?
        _dbContext = dbContextFactory.GetService(tenant);
        _tenant = tenant;
        _logger = logger;
    }

    public async Task Handle(SendEmailMessage message)
    {
        var tenantId = _tenant.Id;

        // Simulate email sending
        await Task.Delay(50);
        
        // Log email to tenant-specific database
        var emailLog = new EmailLog
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId!,
            To = message.To,
            Subject = message.Subject,
            Body = message.Body,
            SentAt = DateTime.UtcNow,
        };

        _dbContext.EmailLogs.Add(emailLog);
        
        try
        {
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Logged email {EmailId} to tenant database for tenant {TenantId}",
                emailLog.Id,
                tenantId);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Log and ignore duplicate key violations - message was already processed
            _logger.LogWarning(
                "Email log {EmailId} already exists for tenant {TenantId} - ignoring duplicate message",
                emailLog.Id,
                tenantId);
        }
    }
    
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Check for common unique constraint violation error messages
        var errorMessage = ex.InnerException?.Message ?? ex.Message;
        return errorMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("primary key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("violation of primary key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}