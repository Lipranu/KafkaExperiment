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
            _consumer.Subscribe(TopicHelper.FactoryInfoTopic);
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
                await Task.Delay(100, ct);
            }
            _consumer.Close();
        }
    }
}
