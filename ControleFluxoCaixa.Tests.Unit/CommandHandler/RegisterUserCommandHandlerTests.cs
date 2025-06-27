using ControleFluxoCaixa.Application.Commands.Auth.RegisterUser;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Tests.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace ControleFluxoCaixa.Tests.Unit.CommandHandler
{
    public class RegisterUserCommandHandlerTests
    {
        [Fact(DisplayName = "Deve registrar usuário com sucesso")]
        public async Task Deve_registrar_usuario_com_sucesso()
        {
            // Arrange
            var command = new RegisterUserCommand("valido@teste.com", "Senha123!", "Usuário Teste");
            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                       .ReturnsAsync(IdentityResult.Success);

            var cache = new Mock<ICacheService>();
            var logger = new Mock<ILogger<RegisterUserCommandHandler>>();
            var handler = new RegisterUserCommandHandler(userManager.Object, cache.Object, logger.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Email, result.Email);
            Assert.Equal(command.FullName, result.FullName);
        }

        [Fact(DisplayName = "Deve falhar se senha estiver fora do padrão")]
        public async Task Deve_falhar_ao_registrar_com_senha_fora_do_padrao()
        {
            // Arrange
            var command = new RegisterUserCommand("teste@teste.com", "123", "Usuário Invalido");

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Senha fraca" }));

            var cache = new Mock<ICacheService>();
            var logger = new Mock<ILogger<RegisterUserCommandHandler>>();
            var handler = new RegisterUserCommandHandler(userManager.Object, cache.Object, logger.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Falha ao criar usuário.", ex.Message);
        }


        [Fact(DisplayName = "Deve falhar se e-mail for inválido")]
        public async Task Deve_falhar_ao_registrar_com_email_invalido()
        {
            // Arrange
            var command = new RegisterUserCommand("email-invalido", "Senha123!", "Usuário Teste");

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "E-mail inválido" }));

            var cache = new Mock<ICacheService>();
            var logger = new Mock<ILogger<RegisterUserCommandHandler>>();
            var handler = new RegisterUserCommandHandler(userManager.Object, cache.Object, logger.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Falha ao criar usuário.", ex.Message);
        }

    }
}
