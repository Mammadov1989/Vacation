using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocation.Core.Models.Identity;

namespace Vocation.Repository.Infrastucture.Identity
{
    public interface IVacRoleStore<TRole> : IRoleStore<TRole> where TRole : class
    {
        Task<ApplicationRole> FindUniqueByNameAsync(string normalizedUserName, string roleId, CancellationToken cancellationToken);
    }

    public class RoleStore : IVacRoleStore<ApplicationRole>, IQueryableRoleStore<ApplicationRole>
    {
        private readonly string _connectionString;

        public IQueryable<ApplicationRole> Roles
        {
            get
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var result = connection.Query<ApplicationRole>($@"SELECT R.ID,R.Name,R.NormalizedName,R.GroupId,RG.NAME 'GROUPNAME' FROM [AppRoles] R
LEFT JOIN AppRoleGroups RG ON R.GroupId=RG.Id ORDER BY R.ID DESC", null).AsQueryable();
                    return result;
                }
            }
        }

        public RoleStore(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                role.Id = await connection.QuerySingleAsync<int>($@"INSERT INTO [AppRoles] ([Name], [NormalizedName], [GroupId])
                    VALUES (@{nameof(ApplicationRole.Name)}, @{nameof(ApplicationRole.NormalizedName)}, @{nameof(ApplicationRole.GroupId)});
                    SELECT CAST(SCOPE_IDENTITY() as int)", role);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($@"UPDATE [AppRoles] SET
                    [Name] = @{nameof(ApplicationRole.Name)},
                    [NormalizedName] = @{nameof(ApplicationRole.NormalizedName)},
                    [GroupId] = @{nameof(ApplicationRole.GroupId)}
                    WHERE [Id] = @{nameof(ApplicationRole.Id)}", role);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($"DELETE FROM [AppRoles] WHERE [Id] = @{nameof(ApplicationRole.Id)}", role);
            }

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationRole>($@"SELECT R.ID,R.Name,R.NormalizedName,R.GroupId,RG.NAME 'GROUPNAME' FROM [AppRoles] R
                        LEFT JOIN AppRoleGroups RG ON R.GroupId=RG.Id
                    WHERE R.[Id] = @{nameof(roleId)}", new { roleId });
            }
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationRole>($@"SELECT * FROM [AppRoles]
                    WHERE [NormalizedName] = @{nameof(normalizedRoleName)}", new { normalizedRoleName });
            }
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }


        public async Task<ApplicationRole> FindUniqueByNameAsync(string normalizedUserName, string roleId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return await connection.QuerySingleOrDefaultAsync<ApplicationRole>($@"SELECT * FROM [AppRoles] 
                    WHERE [NormalizedName] = @{nameof(normalizedUserName)} and [id]<>@{nameof(roleId)}", new { normalizedUserName, roleId });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
