using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocation.Core.Models.Identity;

namespace Vocation.Repository.Infrastucture.Identity
{
    public class VacUserManager : UserManager<ApplicationUser>
    {
        private readonly IVacUserStore<ApplicationUser> _userStore;

        public VacUserManager(IVacUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userStore = store;
        }

        public async Task<ApplicationUser> FindUniqueByNameAsync(string normalizedUserName, string userId)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.FindUniqueByNameAsync(normalizedUserName, userId, cancellationToken.Token);

                return result;
            }
        }
        public async Task<ApplicationUser> FindByFullNameAsync(string fullName)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.FindByFullNameAsync(fullName, cancellationToken.Token);

                return result;
            }
        }
        public async Task<ApplicationUser> FindUniqueByEmailAsync(string email, string userId)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.FindUniqueByEmailAsync(email, userId, cancellationToken.Token);

                return result;
            }
        }
        public async Task<ApplicationUser> Block(ApplicationUser user)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.Block(user, cancellationToken.Token);
                return result;
            }
        }
        //public async Task<IEnumerable<ApplicationRoleUI>> GetRoleByIdAsync(string userId)
        //{
        //    using (var cancellationToken = new CancellationTokenSource())
        //    {
        //        var result = await _userStore.GetRolesByIdAsync(userId, cancellationToken.Token);
        //        return result;
        //    }
        //}

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleName(string roleName)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.GetUsersByRoleName(roleName, cancellationToken.Token);
                return result;
            }
        }

        public async Task<ApplicationUser> FindByPhoneNumberAsync(string phonenumber)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _userStore.FindByPhoneNumberAsync(phonenumber, cancellationToken.Token);
                return result;
            }
        }
    }
}
