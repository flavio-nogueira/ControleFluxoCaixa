namespace ControleFluxoCaixa.Domain.Enums
{
    /// <summary>
    /// Enumeração que define os tipos de filas utilizadas no sistema para envio de mensagens ao RabbitMQ.
    /// </summary>
    public enum TipoFila
    {
        /// <summary>
        /// Representa a fila usada para inclusão de dados (ex: novo lançamento).
        /// </summary>
        Inclusao,

        /// <summary>
        /// Representa a fila usada para exclusão de dados (ex: remoção de lançamento).
        /// </summary>
        Exclusao
    }
}
