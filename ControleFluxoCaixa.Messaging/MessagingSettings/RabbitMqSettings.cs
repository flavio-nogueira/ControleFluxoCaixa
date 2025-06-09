using ControleFluxoCaixa.Domain.Enums;

namespace ControleFluxoCaixa.Messaging.MessagingSettings
{
    /// <summary>
    /// Representa as configurações gerais de integração com o RabbitMQ,
    /// agrupando as definições de cada tipo de fila (ex: inclusão, exclusão).
    /// Essa classe é mapeada a partir da seção "RabbitMqSettings" do appsettings.json.
    /// </summary>
    public class RabbitMqSettings
    {
        /// <summary>
        /// Configurações da fila de inclusão de lançamentos.
        /// </summary>
        public RabbitMqQueueSettings Inclusao { get; set; } = new();

        /// <summary>
        /// Configurações da fila de exclusão de lançamentos.
        /// </summary>
        public RabbitMqQueueSettings Exclusao { get; set; } = new();

        /// <summary>
        /// Retorna dinamicamente as configurações da fila com base no tipo desejado (inclusao ou exclusao).
        /// </summary>
        /// <param name="tipo">Tipo de operação: "inclusao" ou "exclusao".</param>
        /// <returns>Configuração da fila correspondente.</returns>
        public RabbitMqQueueSettings GetSettingsFor(TipoFila tipo)
        {
            return tipo switch
            {
                TipoFila.Inclusao => Inclusao,
                TipoFila.Exclusao => Exclusao,
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), $"Tipo de fila não suportado: {tipo}")
            };
        }

    }

    /// <summary>
    /// Representa os parâmetros necessários para configurar uma fila específica do RabbitMQ,
    /// como URI de conexão, exchange, fila principal e fila de retry.
    /// </summary>
    public class RabbitMqQueueSettings
    {
        /// <summary>
        /// URI de conexão com o servidor RabbitMQ.
        /// Ex: "amqp://guest:guest@localhost:5672"
        /// </summary>
        public string AmqpUri { get; set; } = string.Empty;

        /// <summary>
        /// Nome da exchange responsável por rotear mensagens para a fila.
        /// Ex: "lancamento.exchange"
        /// </summary>
        public string ExchangeName { get; set; } = string.Empty;

        /// <summary>
        /// Nome da fila principal onde as mensagens serão consumidas.
        /// Ex: "lancamento.inclusao.queue"
        /// </summary>
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// Routing key associada à fila principal.
        /// Usada para rotear mensagens corretamente via exchange.
        /// Ex: "lancamento.inclusao"
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;

        /// <summary>
        /// Nome da fila de retry (reentrega), usada em caso de falhas no consumidor.
        /// Ex: "lancamento.inclusao.retry.queue"
        /// </summary>
        public string RetryQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Routing key utilizada para mensagens de retry.
        /// Ex: "lancamento.inclusao.retry"
        /// </summary>
        public string RetryRoutingKey { get; set; } = string.Empty;
    }
}
