using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Shared;

namespace Monitor.Hubs
{
    public class FactoryInfoHub : Hub<IFactoryInfoHub>
    {
        private readonly ILogger<FactoryInfoHub> _logger;
        private readonly IProducer<Guid, Null> _producer;

        public FactoryInfoHub(
            ILogger<FactoryInfoHub> logger, 
            IProducer<Guid,Null> producer)
        {
            _logger = logger;
            _producer = producer;
        }

        public async Task Send(FactoryInfo info)
        {
            await Clients.All.SendInfo(info);
        }

        public async Task Switch(Guid id)
        {
            var message = new Message<Guid,Null> { Key = id };
            var partition = MySuperConsistentPartitionSelectionAlgorithm.SelectPartition(id);
            TopicPartition tp = new(TopicHelper.FactorySwitchTopic, new Partition(partition));

            try
            {
                var result = await _producer.ProduceAsync(tp, message);
                
                _logger.LogDebug("Send switch to partition: {id}", result.Partition);
                _producer.Flush();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
