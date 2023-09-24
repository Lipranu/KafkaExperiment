using Confluent.Kafka;
using Factory.Models;
using Shared;
using Shared.Serdes;

namespace Factory.Services
{
    public class ListenerService : BackgroundService
    {
        private readonly ILogger<ListenerService> _logger;
        private readonly IConsumer<Guid, Null> _consumer;
        private readonly FactoryModel _factory;

        public ListenerService(
            ILogger<ListenerService> logger, 
            IConsumer<Guid, Null> consumer, 
            FactoryModel factory)
        {
            _logger = logger;
            _consumer = consumer;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("ListenerService started");

            var id = _factory.ID;
            var partition = MySuperConsistentPartitionSelectionAlgorithm.SelectPartition(id);
            TopicPartition switchTopic = new (TopicHelper.FactorySwitchTopic, new Partition(partition));
            TopicPartition repairTopic = new (TopicHelper.FactoryRepairTopic, new Partition(partition));
            
            _consumer.Assign(new[] { switchTopic, repairTopic });

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(100, ct);
                try
                {
                    var result = _consumer.Consume(ct);
                    if (result.Message is null)
                    {
                        _logger.LogInformation("Null message");
                        continue;
                    }

                    var key = result.Message.Key;
                    if (key != id)
                    {
                        _logger.LogCritical(
                            "My super algorithm failed! Expected {id}, but get {key}",
                            id,
                            key);
                        continue;
                    }

                    switch (result.Topic)
                    {
                        case TopicHelper.FactorySwitchTopic:
                            await _factory.SwitchState(ct);
                            break;
                        case TopicHelper.FactoryRepairTopic:
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex.Message);
                }
            }
            _consumer.Close();
            _logger.LogInformation("ListenerService stopped");
        }
    }
}
