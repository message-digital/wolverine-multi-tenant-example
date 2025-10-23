using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WolverineMultiTenantExample.Services;

namespace WolverineMultiTenantExample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantService _tenantService;
        private int _cycleCount = 0;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, ITenantService tenantService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _tenantService = tenantService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started, waiting 5 seconds before publishing messages...");
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _cycleCount++;
                    _logger.LogInformation("=== Cycle {CycleCount} - Publishing sample messages at: {time} ===", 
                        _cycleCount, DateTimeOffset.Now);
                    
                    // Get all tenants
                    var tenants = _tenantService.GetAllTenants();
                    
                    // Process each tenant in its own scope
                    foreach (var tenant in tenants)
                    {
                        try
                        {

                            using var scope = _serviceProvider.CreateScope();
                            // Create a new scope for this tenant
                            var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                            
                            // Publish messages for this specific tenant
                            await messagePublisher.PublishSampleMessagesAsync(tenant.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error publishing messages for tenant {TenantId}", tenant.Id);
                            // Continue with other tenants even if one fails
                        }
                    }
                                        
                    // Wait 30 seconds before publishing next batch
                    await Task.Delay(30000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in worker execution");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}
