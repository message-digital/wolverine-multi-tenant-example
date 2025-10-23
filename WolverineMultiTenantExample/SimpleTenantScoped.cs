using System;
using System.Collections.Generic;
using System.Text;
using WolverineMultiTenantExample.Models;

namespace WolverineMultiTenantExample
{
    public interface ISimpleTenantScoped
    {
        void DisplayTenantInfo();
    }

    /// <summary>
    /// Example implementation of a tenant-scoped context for operations that require awareness of the
    /// current tenant. This was created primarily so that I could validate that multiple ITenantServiceFactory instances
    /// of different generic types could be injected into a wolverine handler.
    /// </summary>
    /// <remarks>This class provides basic tenant scoping functionality by associating operations with a
    /// specific tenant.</remarks>
    public class SimpleTenantScoped : ISimpleTenantScoped
    {
        private string TenantName { get; set; }

        public SimpleTenantScoped(Tenant tenant)
        {
            TenantName = tenant.Name;
        }

        public void DisplayTenantInfo()
        {
            Console.WriteLine($"Operating in the context of tenant: {TenantName}");
        }
    }
}
