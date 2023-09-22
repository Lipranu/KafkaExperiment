using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Shared;
using Microsoft.Extensions.Options;
using Factory.Services;
using Factory.Model;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var hostConfig = hostContext.Configuration;
        services.Configure<SchemaRegistryConfig>(hostConfig.GetSection("SchemaRegistry"));
        services.Configure<ConsumerConfig>(hostConfig.GetSection("KafkaConsumer"));
        services.Configure<ProducerConfig>(hostConfig.GetSection("KafkaProducer"));

        services.AddSingleton(sp =>
        {
            var id = Guid.Parse(Environment.GetEnvironmentVariable("FACTORY_ID")!);
            var logger = sp.GetRequiredService<ILogger<FactoryModel>>();
            return new FactoryModel(id, logger);
        });

        services.AddSingleton<ISchemaRegistryClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
            return new CachedSchemaRegistryClient(config.Value);
        });

        //services.A
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ConsumerConfig>>();
            return new ConsumerBuilder<Ignore, string>(config.Value)
                .SetKeyDeserializer(Deserializers.Ignore)
                .SetValueDeserializer(new JsonDeserializer<string>().AsSyncOverAsync())
                .Build();
        });

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
            var schema = sp.GetRequiredService<ISchemaRegistryClient>();
            return new ProducerBuilder<int, FactoryInfo>(config.Value)
                .SetKeySerializer(Serializers.Int32)
                .SetValueSerializer(new JsonSerializer<FactoryInfo>(schema))
                .Build();
        });
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
            return new ConsumerBuilder<string, Null>(config.Value)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Null)
                .Build();
        });

        services.AddHostedService<WorkerService>();
        services.AddHostedService<NotifierService>();
        services.AddHostedService<ListenerService>();
    })
    .Build();

await host.RunAsync();
