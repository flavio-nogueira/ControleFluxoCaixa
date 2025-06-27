using ControleFluxoCaixa.Application.Commands.Auth.Login;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Tests.Shared.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace ControleFluxoCaixa.Tests.Unit.CommandHandler
{
    public class AuthHandlersTests
    {
        [Fact(DisplayName = "Deve autenticar com sucesso e retornar tokens")]
        public async Task Deve_autenticar_usuario_com_sucesso()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "teste@teste.com" };
            var ipAddress = "127.0.0.1";
            var loginCommand = new LoginCommand(user.Email, "Senha123!", ipAddress);

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, loginCommand.Password)).ReturnsAsync(true);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(x => x.GenerateAccessTokenAsync(user)).ReturnsAsync("access-token");

            var refreshToken = new RefreshToken
            {
                Token = "refresh-token",
                UserId = user.Id,
                CreatedByIp = ipAddress
            };

            var refreshTokenService = new Mock<IRefreshTokenService>();
            refreshTokenService.Setup(x => x.GenerateRefreshTokenAsync(user, ipAddress))
                               .ReturnsAsync(refreshToken);

            var cache = new Mock<ICacheService>();
            var logger = new Mock<ILogger<LoginCommandHandler>>();

            var handler = new LoginCommandHandler(
                userManager.Object,
                tokenService.Object,
                refreshTokenService.Object,
                cache.Object,
                logger.Object
            );

            // Act
            var result = await handler.Handle(loginCommand, CancellationToken.None);

            // Assert
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
        }

        [Fact(DisplayName = "Deve falhar ao autenticar usuário inexistente")]
        public async Task Deve_falhar_ao_autenticar_usuario_inexistente()
        {
            // Arrange
            var loginCommand = new LoginCommand("invalido@teste.com", "Senha123!", "127.0.0.1");

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByEmailAsync(loginCommand.Email))
                       .ReturnsAsync((ApplicationUser)null);

            var handler = new LoginCommandHandler(
                userManager.Object,
                Mock.Of<ITokenService>(),
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<ICacheService>(),
                Mock.Of<ILogger<LoginCommandHandler>>()
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(loginCommand, CancellationToken.None));
        }

        [Fact(DisplayName = "Deve falhar ao autenticar com senha incorreta")]
        public async Task Deve_falhar_ao_autenticar_com_senha_incorreta()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "teste@teste.com" };
            var loginCommand = new LoginCommand(user.Email, "SenhaErrada", "127.0.0.1");

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByEmailAsync(loginCommand.Email)).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, loginCommand.Password)).ReturnsAsync(false);

            var handler = new LoginCommandHandler(
                userManager.Object,
                Mock.Of<ITokenService>(),
                Mock.Of<IRefreshTokenService>(),
                Mock.Of<ICacheService>(),
                Mock.Of<ILogger<LoginCommandHandler>>()
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(loginCommand, CancellationToken.None));
        }

    }
}
