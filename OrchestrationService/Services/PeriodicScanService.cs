namespace OrchestrationService.Services
{
    record PeriodicScanServiceState(bool IsEnabled);
    public class PeriodicScanService : BackgroundService
    {
        public bool IsEnabled { get; set; } = true;
        /// TODO
        public TimeSpan Period = TimeSpan.FromMinutes(10);
        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly ILogger<PeriodicScanService> _logger;

        public PeriodicScanService(ILogger<PeriodicScanService> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            serviceScopeFactory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(Period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (IsEnabled)
                    {
                        await using AsyncServiceScope asyncScope = serviceScopeFactory.CreateAsyncScope();
                        ScanService sampleService = asyncScope.ServiceProvider.GetRequiredService<ScanService>();
                        await sampleService.InvokeServices();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to execute Scan with exception {ex.Message}.");
                }
            }
        }
    }
}
