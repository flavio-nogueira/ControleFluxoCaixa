// Importa as entidades de domínio relacionadas ao usuário
using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Application.Interfaces.Auth;

// Importa as classes do Identity Framework
using Microsoft.AspNetCore.Identity;

// Moq para simular comportamentos de dependências
using Moq;

// JWT e segurança
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// Biblioteca de testes xUnit
using Xunit;

namespace ControleFluxoCaixa.Tests.Unit.Identity
{
    /// <summary>
    /// Classe de testes unitários que valida regras relacionadas ao gerenciamento de usuários (UserManager)
    /// e à autenticação via tokens JWT utilizando ITokenService e IRefreshTokenService mockados.
    /// </summary>
    public class UserManagerTests
    {
        /// <summary>
        /// Cria um mock do UserManager com comportamentos simulados para criação de usuário,
        /// cobrindo casos válidos e inválidos (e-mail e senha).
        /// </summary>
        private Mock<UserManager<ApplicationUser>> CriarUserManagerMock_ComCreate()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();

            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                null, null, null, null, null, null, null, null
            );

            userManagerMock.Setup(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
                u.Email == "validuser@teste.com"), "Senha123!"))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "123"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Senha muito curta" }));

            userManagerMock.Setup(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
                u.Email == "invalido"), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "E-mail inválido" }));

            return userManagerMock;
        }

        /// <summary>
        /// Verifica se um usuário válido é criado com sucesso usando UserManager mockado.
        /// </summary>
        [Fact]
        public async Task Deve_criar_usuario_valido_com_sucesso()
        {
            var userManager = CriarUserManagerMock_ComCreate();
            var usuario = new ApplicationUser { Email = "validuser@teste.com", UserName = "validuser" };

            var resultado = await userManager.Object.CreateAsync(usuario, "Senha123!");

            Assert.True(resultado.Succeeded);
        }

        /// <summary>
        /// Verifica se o sistema impede a criação de um usuário com senha fraca.
        /// Espera que o UserManager retorne erro de validação.
        /// </summary>
        [Fact]
        public async Task Nao_deve_criar_usuario_com_senha_invalida()
        {
            var userManager = CriarUserManagerMock_ComCreate();
            var usuario = new ApplicationUser { Email = "usuario@teste.com", UserName = "usuario" };

            var resultado = await userManager.Object.CreateAsync(usuario, "123");

            Assert.False(resultado.Succeeded);
            Assert.Contains(resultado.Errors, e => e.Description.Contains("curta"));
        }

        /// <summary>
        /// Verifica se o sistema bloqueia a criação de um usuário com e-mail inválido.
        /// Espera erro relacionado ao formato ou estrutura do e-mail.
        /// </summary>
        [Fact]
        public async Task Nao_deve_criar_usuario_com_email_invalido()
        {
            var userManager = CriarUserManagerMock_ComCreate();
            var usuario = new ApplicationUser { Email = "invalido", UserName = "usuario" };

            var resultado = await userManager.Object.CreateAsync(usuario, "Senha123!");

            Assert.False(resultado.Succeeded);
            Assert.Contains(resultado.Errors, e => e.Description.Contains("inválido"));
        }

        /// <summary>
        /// Testa a validação de um token expirado simulando a lógica do ITokenService.
        /// Um token assinado corretamente mas com data de expiração passada deve disparar SecurityTokenExpiredException.
        /// </summary>
        [Fact]
        public void Deve_detectar_token_expirado()
        {
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-secreta-teste-para-token-expirado"));
            var creds = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Gera um token já expirado (há 10 segundos)
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }),
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddSeconds(-10),
                SigningCredentials = creds
            });

            var tokenString = tokenHandler.WriteToken(token);

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = chave,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Verifica se a exceção esperada é lançada ao validar um token expirado
            Assert.Throws<SecurityTokenExpiredException>(() =>
            {
                tokenHandler.ValidateToken(tokenString, tokenValidationParams, out _);
            });
        }

        /// <summary>
        /// Testa se um token assinado com uma chave diferente da esperada é rejeitado.
        /// Isso simula uma falha de validação de assinatura, como o ITokenService faria.
        /// </summary>
        [Fact]
        public void Deve_detectar_token_invalido()
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Chave original usada para assinar o token
            var chaveCorreta = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-para-teste-token-invalido-12345678"));
            var creds = new SigningCredentials(chaveCorreta, SecurityAlgorithms.HmacSha256);

            // Criação do token
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds
            });

            var tokenString = tokenHandler.WriteToken(token);

            // Chave errada usada na validação
            var chaveErrada = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("outra-chave-totalmente-errada-99999999"));

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = chaveErrada,
                RequireSignedTokens = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Espera que a validação falhe com a exceção apropriada
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            {
                tokenHandler.ValidateToken(tokenString, tokenValidationParams, out _);
            });
        }
    }
}
