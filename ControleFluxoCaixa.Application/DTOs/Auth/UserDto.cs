namespace ControleFluxoCaixa.Application.DTOs
{
    /// <summary>
    /// DTO que representa os dados retornados de um usuário autenticado ou registrado.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Identificador único do usuário.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Nome completo do usuário (pode ser nulo).
        /// </summary>
        public string? FullName { get; }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="UserDto"/> com os valores fornecidos.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <param name="email">E-mail do usuário.</param>
        /// <param name="fullName">Nome completo do usuário (opcional).</param>
        public UserDto(string id, string email, string? fullName)
        {
            Id = id;
            Email = email;
            FullName = fullName;
        }
    }
}
