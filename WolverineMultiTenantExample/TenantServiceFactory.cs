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

    public class TenantServiceFactory<TService>(IServiceProvider serviceProvider, Func<IServiceProvider, Tenant, TService> builder) : ITenantServiceFactory<TService>
        where TService : class
    {
        public TService GetService(Tenant tenantContext)
        {
            return builder(serviceProvider, tenantContext);
        }
    }
}
