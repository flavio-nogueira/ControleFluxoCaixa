using ControleFluxoCaixa.Application.Commands.Auth.UpdateUser;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Tests.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace ControleFluxoCaixa.Tests.Unit.CommandHandler
{
    public class UpdateUserCommandHandlerTests
    {
        [Fact(DisplayName = "Deve atualizar e-mail e senha do usuário com sucesso")]
        public async Task Deve_atualizar_usuario_com_sucesso()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new UpdateUserCommand(userId, "atual@teste.com", "Usuário Atualizado", "NovaSenha123!");

            var user = new ApplicationUser
            {
                Id = Guid.Parse(userId),
                Email = "atual@teste.com",
                FullName = "Nome Antigo"
            };

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("fake-reset-token");
            userManager.Setup(x => x.ResetPasswordAsync(user, "fake-reset-token", command.NewPassword!))
                       .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler(userManager);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<MediatR.Unit>(result);
            userManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
            userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
                u.Email == command.Email && u.FullName == command.FullName)), Times.Once);
            userManager.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            userManager.Verify(x => x.ResetPasswordAsync(user, "fake-reset-token", command.NewPassword!), Times.Once);
        }


        [Fact(DisplayName = "Deve falhar se usuário não for encontrado")]
        public async Task Deve_falhar_se_usuario_nao_existir()
        {
            // Arrange
            var command = new UpdateUserCommand(Guid.NewGuid().ToString(), "naoexiste@teste.com", null, "Senha123!");

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(command.Id)).ReturnsAsync((ApplicationUser)null);

            var handler = CreateHandler(userManager);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Usuário não encontrado.", ex.Message);
        }

        [Fact(DisplayName = "Deve falhar se ocorrer erro ao atualizar usuário")]
        public async Task Deve_falhar_se_ocorrer_erro_na_atualizacao()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new UpdateUserCommand(userId, "erro@teste.com", "Nome Erro", null);

            var user = new ApplicationUser
            {
                Id = Guid.Parse(userId),
                Email = "erro@teste.com",
                FullName = "Antigo"
            };

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro simulado" }));

            var handler = CreateHandler(userManager);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Contains("Erro ao atualizar dados do usuário.", ex.Message);
        }

        // Factory para criar handler com mocks
        private static UpdateUserCommandHandler CreateHandler(Mock<UserManager<ApplicationUser>> userManager)
        {
            return new UpdateUserCommandHandler(
                userManager.Object,
                Mock.Of<ICacheService>(),
                Mock.Of<ILogger<UpdateUserCommandHandler>>()
            );
        }

        //// Factory para criar mock do UserManager
        //private static Mock<UserManager<ApplicationUser>> MockUserManager()
        //{
        //    var store = new Mock<IUserStore<ApplicationUser>>();
        //    return new Mock<UserManager<ApplicationUser>>(
        //        store.Object, null, null, null, null, null, null, null, null
        //    );
        //}
    }
}
