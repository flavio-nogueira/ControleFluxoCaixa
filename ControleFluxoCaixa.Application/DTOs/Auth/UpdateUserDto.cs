namespace ControleFluxoCaixa.Application.DTOs.Auth
{
    public class UpdateUserDto
    {
        /// <summary>
        /// ID do usuário a ser atualizado
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Novo e-mail (opcional)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Novo nome completo (opcional)
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Nova senha (opcional)
        /// </summary>
        public string? NewPassword { get; set; }
    }
}
