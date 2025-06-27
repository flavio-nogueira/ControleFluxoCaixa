using ControleFluxoCaixa.Messaging.MessagingSettings;         // RabbitMqSettings
using ControleFluxoCaixa.Mongo.Repositories;
using ControleFluxoCaixa.Mongo.Settings;
using ControleFluxoCaixa.MongoDB.Interfaces;
using ControleFluxoCaixa.WorkerRabbitMq.Consumers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        cfg.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // MongoDB
        services.Configure<MongoDbSettings>(cfg.GetSection("Mongo"));
        services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoSettings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(mongoSettings.ConnectionString);
        });
        services.AddScoped(sp =>
            sp.GetRequiredService<IMongoClient>()
              .GetDatabase(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.DatabaseName));
        services.AddScoped<ISaldoDiarioConsolidadoRepository, SaldoDiarioConsolidadoRepository>();

        // RabbitMQ
        services.Configure<RabbitMqSettings>(cfg.GetSection("RabbitMqSettings"));
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var r = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            return new ConnectionFactory
            {
                Uri = new Uri(r.Inclusao.AmqpUri),
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true, 
                RequestedHeartbeat = TimeSpan.FromSeconds(30)
            };
        });

        // Consumer background service
        services.AddHostedService<MovimentoSaldoConsumer>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await builder.RunAsync();
