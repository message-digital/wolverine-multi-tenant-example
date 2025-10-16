using Wolverine;
using WolverineMultiTenantExample.Models;

namespace WolverineMultiTenantExample.Services;

public interface IMessagePublisher
{
    Task PublishSampleMessagesAsync(string tenantId);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IMessageBus _messageBus;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(
        IMessageBus messageBus,
        ITenantService tenantService,
        ILogger<MessagePublisher> logger)
    {
        _messageBus = messageBus;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task PublishSampleMessagesAsync(string tenantId)
    {
        // Get tenant information for generating realistic sample data
        var tenant = _tenantService.GetTenant(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found, skipping message publishing", tenantId);
            return;
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is not active, skipping message publishing", tenantId);
            return;
        }

        _logger.LogInformation("Publishing sample messages for tenant {TenantId} - {TenantName}", 
            tenant.Id, tenant.Name);

        // Publish some sample order messages
        var orderMessage = new ProcessOrderMessage
        {
            TenantId = tenantId,
            OrderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Amount = Random.Shared.Next(10, 1000),
            CustomerName = $"Customer-{Random.Shared.Next(100, 999)}"
        };

        await _messageBus.PublishAsync(orderMessage);
        _logger.LogInformation("Published order message {OrderId} for tenant {TenantId}", 
            orderMessage.OrderId, tenantId);

        // Publish some sample email messages
        var emailMessage = new SendEmailMessage
        {
            TenantId = tenantId,
            To = $"customer{Random.Shared.Next(100, 999)}@{tenant.Name.Replace(" ", "").ToLower()}.com",
            Subject = "Order Confirmation",
            Body = $"Your order {orderMessage.OrderId} has been processed."
        };

        await _messageBus.PublishAsync(emailMessage);
        _logger.LogInformation("Published email message for tenant {TenantId}", tenantId);
    }
}