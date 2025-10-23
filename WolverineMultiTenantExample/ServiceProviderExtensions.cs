using WolverineMultiTenantExample;
using WolverineMultiTenantExample.Models;

namespace System
{
    public static class ServiceProviderExtensions
    {

        public static IServiceCollection AddScopedTenantServiceFactory<TService>(this IServiceCollection serviceProvider, Func<IServiceProvider, Tenant, TService> builder) where TService : class
        {
            return serviceProvider.AddScoped<ITenantServiceFactory<TService>>(sp => new TenantServiceFactory<TService>(sp, builder));
        }

        public static IServiceCollection AddScopedTenantServiceFactory<TService, TImplementation>(this IServiceCollection serviceProvider)
            where TService : class
            where TImplementation : class, TService
        {
            return serviceProvider.AddScoped<ITenantServiceFactory<TService>>(sp => TenantServiceFactory<TService>.CreateDefault<TImplementation>(sp))
            // This isn't used here but in an Http Context this could be registered to utilize Tenant pushed to the scope
            // from aspnetcore middleware.
            .AddScoped<TService>(sp =>
            {
                var tenant = sp.GetRequiredService<Tenant>();
                var factory = sp.GetRequiredService<ITenantServiceFactory<TService>>();

                return factory.GetService(tenant);
            });
        }

    }
}
