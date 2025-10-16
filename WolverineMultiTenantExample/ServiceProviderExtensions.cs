using WolverineMultiTenantExample;
using WolverineMultiTenantExample.Models;

namespace System
{
    public static class ServiceProviderExtensions
    {
        public static IServiceScope CreateTenantScope(this IServiceProvider serviceProvider, Tenant tenant)
        {
            var scope = serviceProvider.CreateScope();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenant);
            return scope;
        }
    }
}
