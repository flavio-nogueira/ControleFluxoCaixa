using ControleFluxoCaixa.Application.Commands.Auth.DeleteUser;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Tests.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ControleFluxoCaixa.Tests.Unit.AuthTests
{
    public class DeleteUserCommandHandlerTests
    {
        [Fact(DisplayName = "Deve excluir usuário com sucesso")]
        public async Task Deve_excluir_usuario_com_sucesso()
        {
            // Arrange
            // Gera um ID fictício para o usuário e instancia um objeto ApplicationUser
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser { Id = Guid.Parse(userId), Email = "teste@teste.com" };

            // Cria o mock do UserManager e configura o comportamento esperado
            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Cria o handler e o comando de exclusão
            var handler = new DeleteUserCommandHandler(userManager.Object);
            var command = new DeleteUserCommand(userId);

            // Act
            // Espera que o retorno seja Unit.Value, indicando sucesso
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(MediatR.Unit.Value, result);
        }

        [Fact(DisplayName = "Deve falhar se usuário não for encontrado")]
        public async Task Deve_falhar_se_usuario_nao_existir()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            var handler = new DeleteUserCommandHandler(userManager.Object);
            var command = new DeleteUserCommand(userId);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Usuário não encontrado.", ex.Message);
        }

        [Fact(DisplayName = "Deve falhar se erro ocorrer ao excluir o usuário")]
        public async Task Deve_falhar_se_erro_ocorrer_ao_excluir_usuario()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ApplicationUser { Id = Guid.Parse(userId), Email = "erro@teste.com" };

            var userManager = UserManagerMockHelper.CreateMock();
            userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Erro ao excluir" }));

            var handler = new DeleteUserCommandHandler(userManager.Object);
            var command = new DeleteUserCommand(userId);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Erro ao excluir", ex.Message);
        }

    }
}
