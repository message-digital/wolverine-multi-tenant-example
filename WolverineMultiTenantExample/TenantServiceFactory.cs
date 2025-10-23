using System;
using System.Collections.Generic;
using System.Text;
using WolverineMultiTenantExample.Data;
using WolverineMultiTenantExample.Models;

namespace WolverineMultiTenantExample
{
    public interface ITenantServiceFactory<TService> 
        where TService : class
    {
        TService GetService(Tenant tenantContext);
    }

    public class TenantServiceFactory<TService> : ITenantServiceFactory<TService>
        where TService : class
    {
        private readonly Func<IServiceProvider, Tenant, TService> _builder;
        private readonly IServiceProvider _serviceProvider;

        public TenantServiceFactory(IServiceProvider serviceProvider, Func<IServiceProvider, Tenant, TService> builder)
        {
            _builder = builder;
            _serviceProvider = serviceProvider;
        }

        public TenantServiceFactory(IServiceProvider serviceProvider)
        {
            _builder = DefaultBuilder;
            _serviceProvider = serviceProvider;
        }

        public TService GetService(Tenant tenantContext)
        {
            return _builder(_serviceProvider, tenantContext);
        }

        public static ITenantServiceFactory<TService> CreateDefault<TImplementation>(IServiceProvider serviceProvider)
            where TImplementation : class, TService
        {
            return new TenantServiceFactory<TService>(serviceProvider, DefaultBuilder);
        }

        private static TService DefaultBuilder(IServiceProvider serviceProvider, Tenant tenant)
        {
            // Use DI and the provided tenant ID to create an instance
            return ActivatorUtilities.CreateInstance<TService>(serviceProvider, tenant);
        }
    }
}
