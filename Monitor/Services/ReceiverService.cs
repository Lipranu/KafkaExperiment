using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Monitor.Hubs;
using Shared;

namespace Monitor.Services
{
    public class ReceiverService : BackgroundService
    {
        private readonly ILogger<ReceiverService> _logger;
        private readonly IHubContext<FactoryInfoHub, IFactoryInfoHub> _hubContext;
        private readonly IConsumer<Ignore, FactoryInfo> _consumer;

        public ReceiverService(
            ILogger<ReceiverService> logger, 
            IHubContext<FactoryInfoHub, IFactoryInfoHub> hubContext, 
            IConsumer<Ignore, FactoryInfo> consumer)
        {
            _logger = logger;
            _hubContext = hubContext;
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("ReceiverService started");
            
            List<TopicPartitionOffset> assigment = new();
            for (int partition = 0; partition < 5; partition++)
            {
                TopicPartition tp = new(TopicHelper.FactoryInfoTopic, partition);
                assigment.Add(new TopicPartitionOffset(tp, Offset.End));
            }
            _consumer.Assign(assigment);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var info = _consumer.Consume(ct).Message.Value;
                    if (info != null)
                    {
                        await _hubContext.Clients.All.SendInfo(info!);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                await Task.Delay(25, ct);
            }

            _consumer.Close();
            _logger.LogInformation("ReceiverService stopped");
        }
    }
}
