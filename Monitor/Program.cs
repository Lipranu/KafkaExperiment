using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Monitor.Hubs;
using Monitor.Services;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Serdes;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.Configure<ConsumerConfig>(config.GetSection("KafkaConsumer"));
services.Configure<ProducerConfig>(config.GetSection("KafkaProducer"));

services.AddRazorPages();
services.AddServerSideBlazor();
services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          new[] { "application/octet-stream" });
});

services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<ConsumerConfig>>().Value;
    return new ConsumerBuilder<Ignore, FactoryInfo>(config)
        .SetKeyDeserializer(Deserializers.Ignore)
        .SetValueDeserializer(new JsonDeserializer<FactoryInfo>().AsSyncOverAsync())
        .Build();
});

services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>().Value;
    return new ProducerBuilder<Guid, Null>(config)
        .SetKeySerializer(new GuidSerializer())
        .SetValueSerializer(Serializers.Null)
        .Build();
});

services.AddHostedService<ReceiverService>();

var app = builder.Build();

app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<FactoryInfoHub>("/factory_info_hub");
app.MapFallbackToPage("/_Host");

app.Run();
