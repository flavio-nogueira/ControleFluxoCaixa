using ControleFluxoCaixa.Domain.Enums;

namespace ControleFluxoCaixa.Messaging.Publishers
{
    /// <summary>
    /// Interface para publicar mensagens no RabbitMQ de forma genérica e desacoplada.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser publicada.</typeparam>
    public interface IRabbitMqPublisher<T>
    {
        /// <summary>
        /// Publica uma mensagem no RabbitMQ utilizando o tipo da fila (ex: Inclusao ou Exclusao).
        /// </summary>
        /// <param name="message">Mensagem a ser publicada.</param>
        /// <param name="tipoFila">Tipo da fila (ex: Inclusao ou Exclusao).</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        Task PublishAsync(T message, TipoFila tipoFila, CancellationToken cancellationToken = default);
    }
}
