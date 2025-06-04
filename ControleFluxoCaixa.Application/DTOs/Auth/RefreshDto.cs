namespace ControleFluxoCaixa.Application.DTOs.Auth
{
    /// <summary>
    /// DTO que representa os tokens de acesso e atualização (refresh), bem como a data de expiração do token de acesso.
    /// </summary>
    public class RefreshDto
    {
        /// <summary>
        /// Token de acesso JWT que será utilizado para autenticação em chamadas subsequentes.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Token de atualização (refresh token), utilizado para obter um novo AccessToken após sua expiração.
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// Data e hora em que o AccessToken expira.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Construtor vazio necessário para model binding e desserialização JSON.
        /// </summary>
        public RefreshDto() { }

        /// <summary>
        /// Construtor que inicializa todos os campos do DTO.
        /// </summary>
        /// <param name="accessToken">Token de acesso JWT.</param>
        /// <param name="refreshToken">Token de atualização.</param>
        /// <param name="expiresAt">Data e hora de expiração do AccessToken.</param>
        public RefreshDto(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }
    }
}

