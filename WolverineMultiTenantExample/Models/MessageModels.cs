namespace WolverineMultiTenantExample.Models;

public interface ITenantScoped
{
    string TenantId { get; }
}

public class TenantScopedMessage : ITenantScoped
{
    public string TenantId { get; set; } = string.Empty;
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProcessOrderMessage : TenantScopedMessage
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

public class SendEmailMessage : TenantScopedMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}