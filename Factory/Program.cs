using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Shared;
using Microsoft.Extensions.Options;
using Factory.Services;
using Factory.Models;
using Shared.Serdes;
using System.Text;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var hostConfig = hostContext.Configuration;
        var id = Guid.Parse(Environment.GetEnvironmentVariable("FACTORY_ID")!);

        services.Configure<SchemaRegistryConfig>(hostConfig.GetSection("SchemaRegistry"));
        services.Configure<ConsumerConfig>(hostConfig.GetSection("KafkaConsumer"));
        services.Configure<ProducerConfig>(hostConfig.GetSection("KafkaProducer"));

        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FactoryModel>>();
            return new FactoryModel(id, logger);
        });

        services.AddSingleton<ISchemaRegistryClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
            return new CachedSchemaRegistryClient(config.Value);
        });

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ConsumerConfig>>().Value;
            var sid = new StringBuilder("Factory(")
                .Append(id.ToString())
                .Append(')')
                .ToString();
            config.GroupId = sid;
            config.ClientId = sid;

            return new ConsumerBuilder<Guid, Null>(config)
                .SetKeyDeserializer(new GuidDeserializer())
                .SetValueDeserializer(Deserializers.Null)
                .Build();
        });

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ProducerConfig>>().Value;
            config.ClientId = new StringBuilder("FactoryInfo(")
                .Append(id.ToString())
                .Append(')')
                .ToString();
            config.EnableIdempotence = false;
            config.Acks = Acks.None;
            config.BatchNumMessages = 1;
            config.MessageSendMaxRetries = 0;
            config.Partitioner = Partitioner.Consistent;

            var schema = sp.GetRequiredService<ISchemaRegistryClient>();

            return new ProducerBuilder<Guid, FactoryInfo>(config)
                .SetKeySerializer(new GuidSerializer())
                .SetValueSerializer(new JsonSerializer<FactoryInfo>(schema).AsSyncOverAsync())
                .Build();
        });

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ProducerConfig>>().Value;
            config.ClientId = new StringBuilder("FactoryData(")
                .Append(id.ToString())
                .Append(')')
                .ToString();
            config.EnableIdempotence = true;
            config.Acks = Acks.All;
            config.BatchNumMessages = 1000;
            config.MessageSendMaxRetries = 10;
            config.Partitioner = Partitioner.Murmur2;

            var schema = sp.GetRequiredService<ISchemaRegistryClient>();

            return new ProducerBuilder<Guid, FactoryData>(config)
                .SetKeySerializer(new GuidSerializer())
                .SetValueSerializer(new JsonSerializer<FactoryData>(schema))
                .Build();
        });

        services.AddHostedService<WorkerService>();
        services.AddHostedService<NotifierService>();
        services.AddHostedService<ListenerService>();
    })
    .Build();

await host.RunAsync();
