using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Domain.Entities.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ControleFluxoCaixa.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            // Lê as configurações do JWT (com fallback caso faltem)
            var key = _configuration["JwtSettings:SecretKey"]
                      ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado");

            var issuer = _configuration["JwtSettings:Issuer"] ?? "default-issuer";
            var audience = _configuration["JwtSettings:Audience"] ?? "default-audience";
            var expiresInMins = _configuration.GetValue<int?>("JwtSettings:ExpiresInMinutes") ?? 60;

            // Cria os claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("fullname", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Gera o token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMins),
                signingCredentials: credentials);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
