using Confluent.Kafka;
using Shared;

namespace Factory
{
    public class Notifier : BackgroundService
    {
        private const string FactoryInfoTopicName = "FactoryInfo";
        private readonly ILogger<Notifier> _logger;
        private readonly IProducer<int, FactoryInfo> _producer;
        //private readonly IProducer<int, int> _producer;
        private readonly Factory _factory;

        public Notifier(
            ILogger<Notifier> logger, 
            IProducer<int, FactoryInfo> producer,
            //IProducer<int, int> producer,
            Factory factory)
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
                //_producer.
                _logger.LogInformation("Sending Message");
                var message = new Message<int, FactoryInfo>()
                {
                    Key = (int)factoryInfo.State,
                    Value = factoryInfo
                };
                //var message = new Message<int, int> { Key = (int)factoryInfo.State, Value = (int)factoryInfo.Status };
                try
                {
                    var result = await _producer.ProduceAsync(FactoryInfoTopicName, message, ct);
                    _producer.Flush(ct);

                    _logger.LogInformation(
                    "{status} is writed to partition {partition}",
                    result.Value,//.Status,
                    result.Partition.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                await Task.Delay(1000, ct);
            }
        }
    }
}
