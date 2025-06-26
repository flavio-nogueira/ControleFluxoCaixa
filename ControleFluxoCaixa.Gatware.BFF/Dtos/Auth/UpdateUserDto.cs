using System.ComponentModel.DataAnnotations;

namespace ControleFluxoCaixa.BFF.Dtos.Auth
{
    /// <summary>
    /// DTO utilizado para atualizar os dados de um usuário existente.
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// ID do usuário a ser atualizado.
        /// </summary>
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Novo e-mail (opcional).
        /// </summary>
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Novo nome completo (opcional).
        /// </summary>
        [StringLength(100, ErrorMessage = "O nome completo deve ter no máximo 100 caracteres.")]
        public string? FullName { get; set; }

        /// <summary>
        /// Nova senha (opcional).
        /// </summary>
        [MinLength(6, ErrorMessage = "A nova senha deve ter ao menos 6 caracteres.")]
        public string? NewPassword { get; set; }
    }
}
