using System.ComponentModel.DataAnnotations;

namespace ControleFluxoCaixa.BFF.Dtos.Auth
{
    /// <summary>
    /// DTO utilizado para registrar um novo usuário.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// E-mail do usuário (também será usado como UserName).
        /// </summary>
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha para login.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário (opcional).
        /// </summary>
        [StringLength(100, ErrorMessage = "O nome completo deve ter no máximo 100 caracteres.")]
        public string? FullName { get; set; }
    }
}
