using Confluent.Kafka;
//using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Factory;
using Microsoft.Extensions.Options;
using Confluent.Kafka.SyncOverAsync;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var hostConfig = hostContext.Configuration;
        services.Configure<SchemaRegistryConfig>(hostConfig.GetSection("SchemaRegistry"));
        services.Configure<ConsumerConfig>(hostConfig.GetSection("KafkaConsumer"));
        services.Configure<ProducerConfig>(hostConfig.GetSection("KafkaProducer"));

        services.AddSingleton(sp => new Factory.Factory());

        services.AddSingleton<ISchemaRegistryClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
            return new CachedSchemaRegistryClient(config.Value);
        });

        services.AddSingleton<IConsumer<Guid, string>>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ConsumerConfig>>();
            return new ConsumerBuilder<Guid, string>(config.Value)
                .SetValueDeserializer(new JsonDeserializer<string>().AsSyncOverAsync())
                .Build();
        });

        services.AddSingleton<IProducer<Guid, string>>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
            var schema = sp.GetRequiredService<ISchemaRegistryClient>();
            return new ProducerBuilder<Guid, string>(config.Value)
                .SetValueSerializer(new JsonSerializer<string>(schema))
                .Build();
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
