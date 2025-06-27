using System.ComponentModel.DataAnnotations;

namespace ControleFluxoCaixa.BFF.Dtos.Auth
{
    /// <summary>
    /// Payload enviado pelo cliente ao tentar autenticar-se.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// E-mail (ou nome de usuário) do usuário que está se autenticando.
        /// </summary>
        [Required(ErrorMessage = "O campo 'Email' é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo 'Email' deve ser um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha em texto puro.
        /// </summary>
        [Required(ErrorMessage = "O campo 'Password' é obrigatório.")]
        [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
