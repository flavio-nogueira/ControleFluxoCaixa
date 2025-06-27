using ControleFluxoCaixa.Messaging.Contracts;
using ControleFluxoCaixa.Messaging.MessagingSettings;
using ControleFluxoCaixa.MongoDB.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Globalization;
using System.Text;
// Alias para JsonConvert
using NewtonsoftJson = Newtonsoft.Json.JsonConvert;

namespace ControleFluxoCaixa.WorkerRabbitMq.Consumers
{
    public class MovimentoSaldoConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory; 
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MovimentoSaldoConsumer> _logger;
        private readonly RabbitMqSettings _rabbitCfg;
        private IConnection _connection;
        private IModel _channel;

        public MovimentoSaldoConsumer(
            IConnectionFactory factory,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqSettings> rabbitOptions,
            ILogger<MovimentoSaldoConsumer> logger)
        {
            _factory = factory;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _rabbitCfg = rabbitOptions.Value;

            _logger.LogInformation("Tentando conectar ao RabbitMQ em {Uri}", _rabbitCfg.Inclusao.AmqpUri);

            // Define parâmetros de resiliência
            if (_factory is ConnectionFactory concreteFactory)
            {
                concreteFactory.AutomaticRecoveryEnabled = true;
                concreteFactory.RequestedHeartbeat = TimeSpan.FromSeconds(30);
            }

            const int maxTentativas = 5;
            for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation("Conexão estabelecida com RabbitMQ");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Tentativa {Tentativa}/{Max} falhou ao conectar: {Erro}",
                        tentativa, maxTentativas, ex.Message);

                    if (tentativa == maxTentativas)
                    {
                        _logger.LogCritical("Conexão com RabbitMQ falhou após {MaxTentativas} tentativas.", maxTentativas);
                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            // Declara as exchanges e filas de inclusão
            var inc = _rabbitCfg.Inclusao;
            _channel.ExchangeDeclare(inc.ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(inc.QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(inc.QueueName, inc.ExchangeName, inc.RoutingKey);

            // Declara as exchanges e filas de exclusão
            var exc = _rabbitCfg.Exclusao;
            _channel.ExchangeDeclare(exc.ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(exc.QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(exc.QueueName, exc.ExchangeName, exc.RoutingKey);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Subscribe(_rabbitCfg.Inclusao.QueueName);
            Subscribe(_rabbitCfg.Exclusao.QueueName);
            return Task.CompletedTask;
        }
        private void Subscribe(string queueName)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Mensagem recebida da fila {Queue}: {Json}", queueName, json);

                try
                {

                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy",
                        Culture = CultureInfo.InvariantCulture
                    };
              
                    var dto = NewtonsoftJson.DeserializeObject<SaldoDto>(json, settings)!;


                    if (dto.Tipo != 0 && dto.Tipo != 1)
                    {
                        _logger.LogWarning("Tipo inválido: {Tipo} recebido na fila {Queue}.", dto.Tipo, queueName);
                        _channel.BasicNack(ea.DeliveryTag, false, false); 
                        return;
                    }

                    // Aqui decidimos dinamicamente se é entrada ou saída
                    bool isEntrada = dto.Tipo == 1;

                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ISaldoDiarioConsolidadoRepository>();
                    await repo.UpdateAsync(dto.Data.Date, dto.Valor, queueName, isEntrada, CancellationToken.None);

                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("{Tipo} processado: {Data} - {Valor}",
                        isEntrada ? "Entrada" : "Saída",
                        dto.Data.ToString("yyyy-MM-dd"), dto.Valor);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem '{Message}'", json);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
