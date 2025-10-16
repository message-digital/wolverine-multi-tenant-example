using Wolverine.Runtime;
using WolverineMultiTenantExample.Models;
using WolverineMultiTenantExample.Services;

namespace WolverineMultiTenantExample;

public class TenantContextMiddleware
{
    private readonly ITenantContext _tenantContext;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(
        ITenantContext tenantContext,
        ITenantService tenantService,
        ILogger<TenantContextMiddleware> logger)
    {
        _tenantContext = tenantContext;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task BeforeAsync(MessageContext context)
    {
        // Extract the tenant-scoped message from the context
        if (context.Envelope?.Message is not ITenantScoped tenantMessage)
        {
            _logger.LogWarning("Message is not tenant-scoped, skipping tenant context setup");
            return;
        }

        // Set the tenant context for this message processing
        var tenantId = tenantMessage.TenantId;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Message does not have a tenant ID specified");
            throw new InvalidOperationException("Tenant ID is required for tenant-scoped messages");
        }

        // Verify tenant exists
        var tenant = _tenantService.GetTenant(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            throw new InvalidOperationException($"Tenant '{tenantId}' not found");
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is not active", tenantId);
            throw new InvalidOperationException($"Tenant '{tenantId}' is not active");
        }

        // Set the tenant context
        _tenantContext.SetTenant(tenant);
        
        _logger.LogInformation("Processing message for tenant {TenantId} - {TenantName}", 
            tenant.Id, tenant.Name);
    }

    public void Finally()
    {
        // Clear the tenant context after processing (success or failure)
        _tenantContext.ClearTenant();
        
        _logger.LogDebug("Cleared tenant context for current message");
    }
}