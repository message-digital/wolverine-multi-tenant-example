using WolverineMultiTenantExample.Models;

namespace WolverineMultiTenantExample.Services;


public interface ITenantService
{
    Tenant? GetTenant(string tenantId);
    IEnumerable<Tenant> GetAllTenants();
}

public class TenantService : ITenantService
{
    private readonly Dictionary<string, Tenant> _tenants;

    public TenantService()
    {
        // Initialize with some sample tenants
        _tenants = new Dictionary<string, Tenant>
        {
            ["tenant1"] = new Tenant 
            { 
                Id = "tenant1", 
                Name = "Tenant 1", 
                ConnectionString = "Data Source=_app_data\\tenant1.db;Cache=Shared" 
            },
            ["tenant2"] = new Tenant 
            { 
                Id = "tenant2", 
                Name = "Tenant 2", 
                ConnectionString = "Data Source=_app_data\\tenant2.db;Cache=Shared"
            },
            ["tenant3"] = new Tenant 
            { 
                Id = "tenant3", 
                Name = "Tenant 3", 
                ConnectionString = "Data Source=_app_data\\tenant3.db;Cache=Shared"
            }
        };
    }

    public Tenant? GetTenant(string tenantId)
    {
        _tenants.TryGetValue(tenantId, out var tenant);
        return tenant;
    }

    public IEnumerable<Tenant> GetAllTenants()
    {
        return _tenants.Values.AsEnumerable();
    }
}