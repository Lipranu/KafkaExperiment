using Confluent.Kafka;
using Factory.Models;
using Shared;

namespace Factory.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly IProducer<Guid, FactoryData> _producer;
        private readonly FactoryModel _factory;

        public WorkerService(
            ILogger<WorkerService> logger,
            IProducer<Guid, FactoryData> producer,
            FactoryModel factory)
        {
            _logger = logger;
            _factory = factory;
            _producer = producer;
        }


        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Worker servise started");
            while (!ct.IsCancellationRequested)
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                var data = await _factory.DoWorkAsync(ct);
                if (data is not null)
                {
                    var message = new Message<Guid, FactoryData> { Key = data.ID, Value = data };
                    try
                    {
                        var result = await _producer.ProduceAsync(TopicHelper.FactoryDataTopic, message, ct);
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }

                }
                await Task.Delay(100, ct);
            }
            _producer.Flush(ct);
            _logger.LogInformation("Worker servise stopped");
        }
    }
}