namespace ControleFluxoCaixa.Application.DTOs.Auth
{
    public class RegisterDto
    {
        /// <summary>
        /// E-mail do usuário (também será usado como UserName)
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Senha para login
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string? FullName { get; set; }
    }
}