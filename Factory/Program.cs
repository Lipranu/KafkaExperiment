using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Factory;
using Shared;
using Microsoft.Extensions.Options;

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
            var logger = sp.GetRequiredService<ILogger<Factory.Factory>>();
            return new Factory.Factory(id, logger);
        });

        services.AddSingleton<ISchemaRegistryClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
            return new CachedSchemaRegistryClient(config.Value);
        });

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
        //services.AddSingleton(sp =>
        //{
        //    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
        //    //var schema = sp.GetRequiredService<ISchemaRegistryClient>();
        //    return new ProducerBuilder<int, int>(config.Value)
        //        .SetKeySerializer(Serializers.Int32)
        //        .SetValueSerializer(Serializers.Int32)
        //        .Build();
        //});
        services.AddHostedService<Worker>();
        services.AddHostedService<Notifier>();
    })
    .Build();

await host.RunAsync();
