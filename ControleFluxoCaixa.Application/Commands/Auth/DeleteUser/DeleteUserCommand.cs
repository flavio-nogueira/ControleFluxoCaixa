using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Auth.DeleteUser
{
    /// <summary>
    /// Comando para remover um usuário com base no ID.
    /// </summary>
    public class DeleteUserCommand : IRequest
    {
        /// <summary>
        /// ID do usuário a ser removido.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Inicializa uma nova instância do comando DeleteUserCommand.
        /// </summary>
        /// <param name="id">ID do usuário a ser removido.</param>
        public DeleteUserCommand(string id)
        {
            Id = id;
        }
    }
}
