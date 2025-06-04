using ControleFluxoCaixa.Domain.Entities.User;

namespace ControleFluxoCaixa.Application.DTOs.Auth
{
    /// <summary>
    /// Representa o resultado da validação de um refresh token, indicando se ele é válido
    /// e, em caso afirmativo, qual é o usuário associado.
    /// </summary>
    public class RefreshValidationResult
    {
        /// <summary>
        /// Indica se o refresh token fornecido é válido (true) ou inválido (false).
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Usuário associado ao refresh token válido. Será nulo se o token não for válido.
        /// </summary>
        public ApplicationUser? User { get; set; }
    }
}
