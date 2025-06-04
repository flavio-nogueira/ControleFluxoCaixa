namespace ControleFluxoCaixa.Application.DTOs.Auth
{
    /// <summary>
    /// Payload enviado pelo cliente ao tentar autenticar-se.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// E-mail (ou nome de usuário) do usuário que está se autenticando.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Senha em texto puro.
        /// </summary>
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Resposta ao login contendo o token JWT gerado.
    /// </summary>
    public class LoginResultDto
    {
        /// <summary>
        /// JWT de acesso.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Expiração em UTC do token (opcional, mas útil ao cliente).
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
