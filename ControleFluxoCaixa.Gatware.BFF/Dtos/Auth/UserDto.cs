namespace ControleFluxoCaixa.BFF.Dtos.Auth
{
    /// <summary>
    /// DTO que representa os dados retornados de um usuário autenticado ou registrado.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Identificador único do usuário.
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário (pode ser nulo).
        /// </summary>
        public string? FullName { get; init; }

        /// <summary>
        /// Inicializa uma nova instância com os valores fornecidos.
        /// </summary>
        public UserDto(string id, string email, string? fullName)
        {
            Id = id;
            Email = email;
            FullName = fullName;
        }

        // Construtor vazio para serialização (obrigatório para Swagger e model binding)
        public UserDto() { }
    }
}
