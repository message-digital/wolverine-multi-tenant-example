using WolverineMultiTenantExample;
using WolverineMultiTenantExample.Models;

namespace System
{
    public static class ServiceProviderExtensions
    {

        public static IServiceCollection AddScopedTenantServiceFactory<TService>(this IServiceCollection serviceProvider, Func<IServiceProvider, Tenant, TService> builder) 
            where TService : class
        {
            return serviceProvider.AddScoped<ITenantServiceFactory<TService>>(sp => new TenantServiceFactory<TService>(sp, builder))
            // This isn't used here but in an Http Context this could be registered to utilize Tenant pushed to the scope
            // from aspnetcore middleware.
            .AddScoped<TService>(DefaultTenantServiceBuilder<TService>);
        }

        public static IServiceCollection AddScopedTenantServiceFactory<TService, TImplementation>(this IServiceCollection serviceProvider, Func<IServiceProvider, Tenant, TService> builder) 
            where TService : class
            where TImplementation : class, TService
        {
            return serviceProvider.AddScoped<ITenantServiceFactory<TService>>(sp => new TenantServiceFactory<TService>(sp, builder))
            // This isn't used here but in an Http Context this could be registered to utilize Tenant pushed to the scope
            // from aspnetcore middleware.
            .AddScoped<TService>(DefaultTenantServiceBuilder<TService>);
        }

        public static IServiceCollection AddScopedTenantServiceFactory<TService, TImplementation>(this IServiceCollection serviceProvider)
            where TService : class
            where TImplementation : class, TService
        {
            return serviceProvider.AddScoped<ITenantServiceFactory<TService>>(DefaultFactoryBuilder<TService, TImplementation>)
            // This isn't used here but in an Http Context this could be registered to utilize Tenant pushed to the scope
            // from aspnetcore middleware.
            .AddScoped<TService>(DefaultTenantServiceBuilder<TService>);
        }


        private static ITenantServiceFactory<TService> DefaultFactoryBuilder<TService, TImplementation>(IServiceProvider serviceProvider)
            where TService : class
            where TImplementation : class, TService
        {
            // Use DI and the provided tenant ID to create an instance
            return new TenantServiceFactory<TService>(serviceProvider, DefaultTenantServiceBuilder<TService, TImplementation>);
        }

        private static TService DefaultTenantServiceBuilder<TService, TImplementation>(IServiceProvider serviceProvider, Tenant tenant)
            where TService : class
            where TImplementation : class, TService
        {
            // Use DI and the provided tenant ID to create an instance
            return ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider, tenant);
        }

        private static TService DefaultTenantServiceBuilder<TService>(IServiceProvider serviceProvider)
            where TService : class
        {
            var tenant = serviceProvider.GetRequiredService<Tenant>();
            var factory = serviceProvider.GetRequiredService<ITenantServiceFactory<TService>>();

            return factory.GetService(tenant);
        }

    }
}
