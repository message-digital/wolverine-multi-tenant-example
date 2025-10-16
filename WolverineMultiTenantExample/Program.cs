using JasperFx.CodeGeneration;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using WolverineMultiTenantExample.Data;
using WolverineMultiTenantExample.Models;
using WolverineMultiTenantExample.Services;

namespace WolverineMultiTenantExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Add Entity Framework with SQLite for application data
            builder.Services.AddDbContext<WolverineDbContext>(options =>
                options.UseSqlite("Data Source=_app_data/wolverine.db;Cache=Shared"));

            // Add tenant services
            builder.Services.AddScoped<ITenantContext, TenantContext>();
            builder.Services.AddSingleton<ITenantService, TenantService>();
            builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

            builder.Services.AddDbContext<TenantDbContext>((sp, options) =>
            {
                var tenant = sp.GetRequiredService<ITenantContext>().CurrentTenant ?? throw new InvalidOperationException("No Tenant Context");
                options.UseSqlite(tenant.ConnectionString);
            });

            // Add the background worker
            builder.Services.AddHostedService<Worker>();

            // Configure Wolverine
            builder.UseWolverine(opts =>
            {
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
                // Enable source code writing to view generated code
                opts.CodeGeneration.SourceCodeWritingEnabled = true;
                        
                // Write generated code to a specific directory
                opts.CodeGeneration.GeneratedCodeOutputPath = "GeneratedCode";

                // Configure local queues for processing messages
                opts.LocalQueue("orders")
                    .Sequential()
                    .UseDurableInbox();

                opts.LocalQueue("emails")
                    .Sequential()
                    .UseDurableInbox();

                // Route messages to appropriate queues
                opts.PublishMessage<ProcessOrderMessage>().ToLocalQueue("orders");
                opts.PublishMessage<SendEmailMessage>().ToLocalQueue("emails");
                // Configure middleware policies
                // QUESTION: For some reason message type specific middleware registration is not working as expected
                // opts.Policies.ForMessagesOfType<ITenantScoped>().AddMiddleware<TenantContextMiddleware>();
                opts.Policies.AddMiddleware<TenantContextMiddleware>();
            });

            var host = builder.Build();

            // Ensure application database is created
            await host.Services.EnsureDatabaseCreatedAsync();
            
            // Ensure all tenant databases are created
            await EnsureTenantDatabasesCreatedAsync(host.Services);

            await host.RunJasperFxCommands(args);
        }

        private static async Task EnsureTenantDatabasesCreatedAsync(IServiceProvider serviceProvider)
        {
            var tenantService = new TenantService();
            var tenants = tenantService.GetAllTenants();
            
            foreach (var tenant in tenants)
            {
                using var scope = serviceProvider.CreateTenantScope(tenant);
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
                    await dbContext.Database.EnsureCreatedAsync();
                    logger.LogInformation("Ensured database exists for tenant {TenantId} - {TenantName}", tenant.Id, tenant.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create database for tenant {TenantId}", tenant.Id);
                }
            }
        }
    }
}
