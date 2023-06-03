using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocation.Core.Models.Identity;

namespace Vocation.Repository.Infrastucture.Identity
{
    public class VacRoleManager : RoleManager<ApplicationRole>
    {
        private readonly IVacRoleStore<ApplicationRole> _roleStore;

        public VacRoleManager(IVacRoleStore<ApplicationRole> store, IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<ApplicationRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
            _roleStore = store;
        }

        public async Task<ApplicationRole> FindUniqueByNameAsync(string normalizedUserName, string roleId)
        {
            using (var cancellationToken = new CancellationTokenSource())
            {
                var result = await _roleStore.FindUniqueByNameAsync(normalizedUserName, roleId, cancellationToken.Token);

                return result;
            }
        }
    }
}
