using WolverineMultiTenantExample.Models;

namespace WolverineMultiTenantExample;

public interface ITenantContext
{
    Tenant? CurrentTenant { get; }
    void SetTenant(Tenant tenant);
    void ClearTenant();
}

public class TenantContext : ITenantContext
{
    private Tenant? _tenant;

    public Tenant? CurrentTenant => _tenant;

    public void SetTenant(Tenant tenant)
    {
        _tenant = tenant;
    }

    public void ClearTenant()
    {
        _tenant = null;
    }
}
