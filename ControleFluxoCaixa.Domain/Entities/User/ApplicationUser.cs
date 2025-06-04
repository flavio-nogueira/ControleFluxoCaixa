using Microsoft.AspNetCore.Identity;

namespace ControleFluxoCaixa.Domain.Entities.User
{
    /// <summary>
    /// Representa um usuário da aplicação, estendendo <see cref="IdentityUser{TKey}"/> com chave GUID
    /// e adicionando o campo de nome completo.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Nome completo do usuário (opcional).
        /// </summary>
        public string? FullName { get; set; }
    }
}
// Este código define a entidade ApplicationUser, que representa um usuário da aplicação.
