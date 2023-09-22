using Confluent.Kafka;
using Factory.Model;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.Services
{
    public class ListenerService : BackgroundService
    {
        private readonly Logger<ListenerService> _logger;
        private readonly IConsumer<string, Null> _consumer;
        private readonly FactoryModel _factory;

        public ListenerService(
            Logger<ListenerService> logger, 
            IConsumer<string, Null> consumer, 
            FactoryModel factory)
        {
            _logger = logger;
            _consumer = consumer;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("ReceiverService started");
            _consumer.Subscribe(new[] { TopicHelper.FactorySwitchTopic, TopicHelper.FactoryRepairTopic });

            while (!ct.IsCancellationRequested)
            {

                try
                {
                    var result = _consumer.Consume(ct);
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
