using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Messaging.Contracts;
using ControleFluxoCaixa.MongoDB.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using NewtonsoftJson = Newtonsoft.Json.JsonConvert;

namespace ControleFluxoCaixa.WorkerRabbitMq.Consumers
{
    public class MovimentoSaldoConsumer : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MovimentoSaldoConsumer> _logger;
        private IModel _channel;

        public MovimentoSaldoConsumer(
            IConnectionFactory factory,
            IServiceProvider serviceProvider,
            ILogger<MovimentoSaldoConsumer> logger)
        {
            _factory = factory;
            _serviceProvider = serviceProvider;    // você já injeta aqui
            _logger = logger;

            var connection = _factory.CreateConnection();
            _channel = connection.CreateModel();
            // declara filas...
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Subscribe(FilaNames.Incluir, isEntrada: true);
            //Subscribe(FilaNames.Excluir, isEntrada: false);
            return Task.CompletedTask;
        }

        private void Subscribe(string queueName, bool isEntrada)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var msg = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var dto = NewtonsoftJson.DeserializeObject<SaldoDto>(msg)!;

                    // Aqui você usa o _serviceProvider, não Program
                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ISaldoDiarioConsolidadoRepository>();

                    await repo.UpdateAsync(
                        data: dto.Data.Date,
                    valor: dto.Valor,
                      tipoFila: queueName,
                        isEntrada: isEntrada,
                        cancellationToken: CancellationToken.None);

                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("{Tipo} processado: {Date} {Valor}",
                        isEntrada ? "Entrada" : "Saída",
                        dto.Data.ToString("yyyy-MM-dd"),
                        dto.Valor);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro processando fila {Fila}: {Msg}", queueName, msg);
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };
            _channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
        }

        public override void Dispose()
        {
            _channel?.Close();
            base.Dispose();
        }
    }
}
