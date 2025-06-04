using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Domain.Entities.User;

namespace ControleFluxoCaixa.Application.Interfaces.Auth
{
    /// <summary>
    /// Serviço responsável por gerar e validar refresh tokens para usuários.
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Gera um novo <see cref="RefreshToken"/> para o usuário especificado e registra o IP de origem.
        /// </summary>
        /// <param name="user">Usuário para o qual o refresh token será gerado.</param>
        /// <param name="ipAddress">Endereço IP de onde a solicitação foi feita (opcional, para auditoria).</param>
        /// <returns>
        /// Uma <see cref="RefreshToken"/> contendo o token gerado e sua data de expiração.
        /// </returns>
        Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress);

        /// <summary>
        /// Valida o refresh token informado e, se válido, o consome para evitar reutilização.
        /// </summary>
        /// <param name="token">Refresh token que será validado e consumido.</param>
        /// <returns>
        /// Um <see cref="RefreshValidationResult"/> indicando se o token é válido e, em caso afirmativo, o usuário associado.
        /// </returns>
        Task<RefreshValidationResult> ValidateAndConsumeRefreshTokenAsync(string token);
    }
}
