using ControleFluxoCaixa.Application.Interfaces.Seed;
using ControleFluxoCaixa.Domain.Entities.User;
using Microsoft.AspNetCore.Identity;

namespace ControleFluxoCaixa.Infrastructure.Seeders
{
    /// <summary>
    /// Classe responsável por executar o seed do usuário administrador padrão.
    /// Essa classe garante que o seed só será executado uma única vez,
    /// registrando a execução no histórico de seeds (SeedHistory).
    /// </summary>
    public class SeedIdentityAdminUser
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISeederService _seederService;

        /// <summary>
        /// Construtor da classe. Recebe o UserManager para manipulação de usuários e o serviço de seed controlado.
        /// </summary>
        /// <param name="userManager">Gerenciador de usuários Identity</param>
        /// <param name="seederService">Serviço que controla se o seed já foi executado</param>
        public SeedIdentityAdminUser(UserManager<ApplicationUser> userManager, ISeederService seederService)
        {
            _userManager = userManager;
            _seederService = seederService;
        }

        /// <summary>
        /// Executa o seed de criação do usuário administrador, caso ainda não tenha sido executado.
        /// </summary>
        public async Task ExecuteAsync()
        {
            const string seedName = "SeedIdentityAdminUser";

            // Verifica se este seed já foi executado anteriormente
            if (await _seederService.HasRunAsync(seedName))
            {
                Console.WriteLine("Seed 'SeedIdentityAdminUser' já executado. Ignorado.");
                return;
            }

            // Dados do usuário administrador a ser criado
            var email = "arquiteto.solucoes@flavio.com";
            var password = "Fn2025@!";
            var fullName = "Usuário Administrador";

            // Verifica se o usuário já existe no banco (mesmo que o seed não tenha sido registrado)
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                Console.WriteLine("Usuário admin já existe. Seed ignorado.");
                await _seederService.MarkAsRunAsync(seedName, true, "system");
                return;
            }

            // Cria o novo usuário admin com os dados definidos
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Sucesso na criação
                Console.WriteLine("Usuário admin criado com sucesso.");
                await _seederService.MarkAsRunAsync(seedName, true, "system");
            }
            else
            {
                // Falha na criação
                Console.WriteLine("Falha ao criar usuário admin:");
                foreach (var error in result.Errors)
                    Console.WriteLine($"  - {error.Description}");

                await _seederService.MarkAsRunAsync(seedName, false, "system");
            }
        }
    }
}
