using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Infrastructure.Context.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ControleFluxoCaixa.Infrastructure.Services.Auth
{
    /// <summary>
    /// Serviço responsável por gerar e validar tokens de atualização (Refresh Tokens).
    /// Utiliza o contexto IdentityContext para persistir e consultar RefreshTokens no banco de dados.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IdentityDBContext _ctx;

        /// <summary>
        /// Construtor que recebe o IdentityContext via injeção de dependência.
        /// </summary>
        /// <param name="ctx">Contexto de identidade para acessar a tabela de RefreshTokens.</param>
        public RefreshTokenService(IdentityDBContext ctx)
            => _ctx = ctx;

        /// <summary>
        /// Gera um novo RefreshToken para o usuário fornecido e persiste no banco de dados.
        /// </summary>
        /// <param name="user">Objeto ApplicationUser para o qual o token será gerado.</param>
        /// <param name="ipAddress">Endereço IP de onde a requisição foi feita (para auditoria).</param>
        /// <returns>Instância de RefreshToken criada e salva no banco.</returns>
        public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress)
        {
            // Cria um novo RefreshToken com valores gerados aleatoriamente e vencimento em 7 dias
            var rt = new RefreshToken
            {
                // Gera 64 bytes aleatórios e converte em Base64 para formar o token
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                // Define a data de expiração com 7 dias a partir de agora (UTC)
                Expires = DateTime.UtcNow.AddDays(7),
                // Armazena a data de criação (UTC) para auditoria
                Created = DateTime.UtcNow,
                // Guarda o IP que solicitou este token
                CreatedByIp = ipAddress,
                // Associa o RefreshToken ao usuário fornecido
                UserId = user.Id,
                User = user
            };

            // Adiciona o objeto ao DbSet para inclusão no banco
            _ctx.RefreshTokens.Add(rt);
            // Persiste a alteração no banco de dados
            await _ctx.SaveChangesAsync();

            // Retorna o RefreshToken recém-criado (incluindo ID gerado, se houver)
            return rt;
        }

        /// <summary>
        /// Valida um RefreshToken existente e, se válido, marca-o como consumido (Used = true).
        /// </summary>
        /// <param name="token">String do RefreshToken a ser validado.</param>
        /// <returns>
        /// Objeto RefreshValidationResult indicando se o token é válido e, se sim, devolvendo o usuário associado.
        /// </returns>
        public async Task<RefreshValidationResult> ValidateAndConsumeRefreshTokenAsync(string token)
        {
            // Busca o RefreshToken no banco, incluindo a navegação para User
            var rt = await _ctx.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            // Se não encontrar, estiver expirado ou já tiver sido usado, retorna inválido
            if (rt == null || rt.Expires < DateTime.UtcNow || rt.Used)
                return new RefreshValidationResult { IsValid = false, User = null };

            // Marca o token como utilizado para impedir novo uso
            rt.Used = true;
            // Persiste a alteração (Used = true) no banco
            await _ctx.SaveChangesAsync();

            // Se tudo for válido, retorna resultado com User associado
            return new RefreshValidationResult
            {
                IsValid = true,
                User = rt.User
            };
        }
    }
}
