namespace Factory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Factory _factory;

        public Worker(ILogger<Worker> logger, Factory factory)
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