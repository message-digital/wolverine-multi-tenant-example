using JasperFx.MultiTenancy;
using Wolverine.Runtime;
using WolverineMultiTenantExample.Models;
using WolverineMultiTenantExample.Services;

namespace WolverineMultiTenantExample;

public class TenantContextMiddleware
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(
        ITenantService tenantService,
        ILogger<TenantContextMiddleware> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<Tenant> BeforeAsync(MessageContext context, TenantId tenantId)
    {
        // Extract the tenant-scoped message from the context
        if (context.Envelope?.Message is not ITenantScoped tenantMessage)
        {
            _logger.LogWarning("Message is not tenant-scoped, skipping tenant context setup");
            throw new InvalidOperationException("Tenant ID is required for tenant-scoped messages");
        }

        // Set the tenant context for this message processing
        
        if (string.IsNullOrEmpty(tenantId.Value))
        {
            _logger.LogWarning("Message does not have a tenant ID specified");
            throw new InvalidOperationException("Tenant ID is required for tenant-scoped messages");
        }

        // Verify tenant exists
        var tenant = _tenantService.GetTenant(tenantId.Value);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId.Value);
            throw new InvalidOperationException($"Tenant '{tenantId.Value}' not found");
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is not active", tenantId);
            throw new InvalidOperationException($"Tenant '{tenantId}' is not active");
        }

        _logger.LogInformation("Processing message for tenant {TenantId} - {TenantName}", 
            tenant.Id, tenant.Name);

        return tenant;
    }

    public void Finally()
    {        
        _logger.LogDebug("Cleared tenant context for current message");
    }
}