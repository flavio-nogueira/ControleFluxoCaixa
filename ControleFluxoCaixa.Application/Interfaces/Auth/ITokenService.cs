using ControleFluxoCaixa.Domain.Entities.User;

namespace ControleFluxoCaixa.Application.Interfaces.Auth
{
    /// <summary>
    /// Serviço responsável por gerar tokens de acesso (JWT) para usuários autenticados.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Gera um token de acesso (JWT) baseado nas informações do usuário.
        /// </summary>
        /// <param name="user">Usuário para o qual o token de acesso será gerado.</param>
        /// <returns>Token JWT como string.</returns>
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    }
}
