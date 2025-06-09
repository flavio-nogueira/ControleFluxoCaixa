using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Messaging.MessagingSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ControleFluxoCaixa.Messaging.Publishers
{
    /// <summary>
    /// Implementação genérica do publicador de mensagens RabbitMQ.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser publicada.</typeparam>
    public class RabbitMqPublisher<T> : IRabbitMqPublisher<T>
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqPublisher<T>> _logger;

        /// <summary>
        /// Construtor com injeção das configurações e logger.
        /// </summary>
        public RabbitMqPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqPublisher<T>> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Publica uma mensagem no RabbitMQ utilizando as configurações com base no tipo da fila.
        /// Cria automaticamente o exchange, fila e binding se ainda não existirem.
        /// </summary>
        public Task PublishAsync(T message, TipoFila tipoFila, CancellationToken cancellationToken = default)
        {
            // Obtém a configuração baseada no tipo de operação (Inclusao ou Exclusao)
            var config = _settings.GetSettingsFor(tipoFila);

            // Cria uma nova fábrica de conexões RabbitMQ
            var factory = new ConnectionFactory
            {
                Uri = new Uri(config.AmqpUri),
                DispatchConsumersAsync = true
            };

            // Estabelece a conexão e canal
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Garante que o exchange exista
            channel.ExchangeDeclare(
                exchange: config.ExchangeName,
                type: "direct",
                durable: true,
                autoDelete: false
            );

            // Garante que a fila exista
            channel.QueueDeclare(
                queue: config.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Garante o vínculo entre exchange e fila via routing key
            channel.QueueBind(
                queue: config.QueueName,
                exchange: config.ExchangeName,
                routingKey: config.RoutingKey
            );

            // Serializa a mensagem para JSON
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Define as propriedades da mensagem
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Publica a mensagem
            channel.BasicPublish(
                exchange: config.ExchangeName,
                routingKey: config.RoutingKey,
                basicProperties: properties,
                body: body
            );

            // Log de sucesso
            _logger.LogInformation(
                "✅ Mensagem publicada no RabbitMQ → Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                config.ExchangeName,
                config.RoutingKey
            );

            return Task.CompletedTask;
        }
    }
}
