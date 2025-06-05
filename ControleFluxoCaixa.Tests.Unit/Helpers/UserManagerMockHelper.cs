using ControleFluxoCaixa.Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ControleFluxoCaixa.Tests.Shared.Helpers
{
    public static class UserManagerMockHelper
    {
        // Factory para criar mock do UserManager
        public static Mock<UserManager<ApplicationUser>> CreateMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null, // IOptions<IdentityOptions>
                null, // IPasswordHasher<TUser>
                null, // IEnumerable<IUserValidator<TUser>>
                null, // IEnumerable<IPasswordValidator<TUser>>
                null, // ILookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<TUser>>
            );
        }
    }
}
