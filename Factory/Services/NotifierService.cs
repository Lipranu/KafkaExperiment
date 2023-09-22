using Confluent.Kafka;
using Shared;
using Factory.Model;

namespace Factory.Services
{
    public class NotifierService : BackgroundService
    {
        private readonly ILogger<NotifierService> _logger;
        private readonly IProducer<int, FactoryInfo> _producer;
        private readonly FactoryModel _factory;

        public NotifierService(
            ILogger<NotifierService> logger,
            IProducer<int, FactoryInfo> producer,
            FactoryModel factory)
        {
            _logger = logger;
            _producer = producer;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Notifier service started");
            while (!ct.IsCancellationRequested)
            {
                _logger.LogInformation("Getting state");
                var factoryInfo = await _factory.GetInfoAsync(ct);
                _logger.LogInformation("State: {}", factoryInfo.State);
                _logger.LogInformation("Sending Message");
                var message = new Message<int, FactoryInfo>()
                {
                    Key = (int)factoryInfo.State,
                    Value = factoryInfo
                };
      
                try
                {
                    var result = await _producer.ProduceAsync(TopicHelper.FactoryInfoTopic, message, ct);
                    _producer.Flush(ct);

                    _logger.LogInformation(
                    "{status} is writed to partition {partition}",
                    result.Value,
                    result.Partition.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                await Task.Delay(1000, ct);
            }
        }
    }
}
