using Factory.Model;

namespace Factory.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly FactoryModel _factory;

        public WorkerService(ILogger<WorkerService> logger, FactoryModel factory)
        {
            _logger = logger;
            _factory = factory;
        }


        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Worker servise started");
            while (!ct.IsCancellationRequested)
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                await _factory.DoWorkAsync(ct);
                await Task.Delay(3000, ct);
            }
        }
    }
}