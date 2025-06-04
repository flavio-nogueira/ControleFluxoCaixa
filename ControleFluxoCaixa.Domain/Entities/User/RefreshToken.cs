namespace ControleFluxoCaixa.Domain.Entities.User
{
    /// <summary>
    /// Representa um token de atualização (refresh token) associado a um usuário.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Identificador único do refresh token (GUID).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// String que contém o valor do token gerado.
        /// </summary>
        public string Token { get; set; } = default!;

        /// <summary>
        /// Data e hora em que este token expira.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Identificador do usuário (ApplicationUser) a quem pertence este token.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Navegação para a entidade de usuário associada a este refresh token.
        /// </summary>
        public ApplicationUser User { get; set; } = default!;

        /// <summary>
        /// Indica se este token já foi utilizado (consumido).
        /// </summary>
        public bool Used { get; set; }

        /// <summary>
        /// Data e hora em que este refresh token foi criado.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Endereço IP de onde o token foi solicitado/gerado.
        /// </summary>
        public string CreatedByIp { get; set; } = default!;
    }
}
