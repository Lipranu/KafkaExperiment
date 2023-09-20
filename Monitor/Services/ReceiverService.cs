using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Monitor.Hubs;
using Shared;

namespace Monitor.Services
{
    public class ReceiverService : BackgroundService
    {
        private const string FactoryInfoTopicName = "FactoryInfo";
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
            _consumer.Subscribe(FactoryInfoTopicName);
            while (!ct.IsCancellationRequested)
            {

                try
                {
                    var info = _consumer.Consume(ct).Message.Value;
                    //_consumer.Consume()
                    //FactoryInfo? info = new FactoryInfo()
                    //{
                    //    ID = Guid.Parse("dd789adc-d242-44e6-a36b-b93f1e3a7908"),
                    //    State = FactoryState.Running,
                    //    Status = Random.Shared.NextDouble() * 10
                    //};
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
