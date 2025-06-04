namespace ControleFluxoCaixa.Domain.Entities
{
    /// <summary>
    /// Representa o histórico de execuções de scripts de seed (dados iniciais).
    /// Essa entidade permite controlar quais seeds já foram aplicados,
    /// quem executou e se foram bem-sucedidos.
    /// </summary>
    public class SeedHistory
    {
        /// <summary>
        /// Chave primária da tabela SeedHistory, gerada como GUID.
        /// Garante unicidade global para cada execução registrada.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nome único do seed executado (ex: "SeedUsuarioAdmin").
        /// Usado para impedir reexecuções do mesmo seed.
        /// </summary>
        public string SeedName { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que o seed foi executado.
        /// Permite auditoria e rastreabilidade.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Nome do usuário ou sistema que executou o seed.
        /// Pode ser "Sistema", "Admin", etc.
        /// </summary>
        public string ExecutedBy { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o seed foi executado com sucesso.
        /// true = sucesso | false = falha (registrada mesmo assim).
        /// </summary>
        public bool Succeeded { get; set; }
    }
}
