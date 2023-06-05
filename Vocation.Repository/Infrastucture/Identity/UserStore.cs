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
    public interface IVacUserStore<TUser> : IUserStore<TUser> where TUser : class
    {
        Task<ApplicationUser> FindUniqueByNameAsync(string normalizedUserName, string userId, CancellationToken cancellationToken);
        Task<ApplicationUser> FindByFullNameAsync(string fullName, CancellationToken cancellationToken);
        Task<ApplicationUser> FindUniqueByEmailAsync(string email, string userId, CancellationToken cancellationToken);
        //Task<IList<ApplicationRoleUI>> GetRolesByIdAsync(string userId, CancellationToken cancellationToken);
        //Task<ApplicationUser> Block(ApplicationUser user, CancellationToken cancellationToken);
        Task<IList<ApplicationUser>> GetUsersByRoleName(string roleName, CancellationToken cancellationToken);
        Task<ApplicationUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    }

    public class UserStore : IVacUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserPhoneNumberStore<ApplicationUser>,
       IUserTwoFactorStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserAuthenticationTokenStore<ApplicationUserToken>
    {
        private readonly string _connectionString;

        private readonly IUnitOFWork _unitOfWork;

        public UserStore(IConfiguration configuration, IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    user.Id = await connection.QuerySingleAsync<Guid>($@"
                                                                       DECLARE @MyTableVar table([ID] [uniqueidentifier]);
                                                                       INSERT INTO [dbo].[AppUsers]
                                                                      ([Email] ,[NormalizedEmail]
                                                                      ,[EmailConfirmed] ,[LockoutEnabled] ,[LockoutEnd]
                                                                      ,[PasswordHash] ,[PhoneNumber] ,[PhoneNumberConfirmed]
                                                                      ,[TwoFactorEnabled],[UserName] ,[NormalizedUserName]
                                                                      ,[SecurityStamp], DisplayName,[Delete])
                                                                       OUTPUT INSERTED.[Id] INTO @MyTableVar
                                                                       VALUES
                                                                      (@{nameof(ApplicationUser.Email)}
                                                                      ,@{nameof(ApplicationUser.NormalizedEmail)}
                                                                      ,1
                                                                      ,1
                                                                      ,null
                                                                      ,@{nameof(ApplicationUser.PasswordHash)}
                                                                      ,@{nameof(ApplicationUser.PhoneNumber)}
                                                                      ,0
                                                                      ,0
                                                                      ,@{nameof(ApplicationUser.UserName)}
                                                                      ,@{nameof(ApplicationUser.NormalizedUserName)}
                                                                      ,@{nameof(ApplicationUser.SecurityStamp)}
                                                                      ,@{nameof(ApplicationUser.DisplayName)},0)
                                                                       SELECT [ID] FROM @MyTableVar;
                                                                       ", user);
                }
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed();
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($"UPDATE [AppUsers] SET [Delete]=1 WHERE [Id] = @{nameof(ApplicationUser.Id)}", user);
            }

            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers] 
                    WHERE [Id] = @{nameof(userId)} and [delete]=0", new { userId });
            }
        }
        public async Task<ApplicationUser> FindByFullNameAsync(string fullName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers] 
                    WHERE [DisplayName] = @{nameof(fullName)} and [delete]=0", new { fullName });
            }
        }
        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers]
                    WHERE [NormalizedUserName] = @{nameof(normalizedUserName)} and [delete]=0", new { normalizedUserName });
            }
        }
        //public async Task<ApplicationUser> Block(ApplicationUser user, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ApplicationUser res = new ApplicationUser();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        user.Active = user.Blocked;
        //        user.Blocked = !user.Blocked;

        //        res = await connection.QueryFirstOrDefaultAsync<ApplicationUser>($@"UPDATE [AppUsers] SET
        //            [Blocked]=@{nameof(ApplicationUser.Blocked)}
        //            WHERE [Id] = @{nameof(ApplicationUser.Id)};

        //         UPDATE [Employees] SET
        //            [Active]=@{nameof(ApplicationUser.Active)}
        //            WHERE [UserId] = @{nameof(ApplicationUser.Id)}

        //            SELECT AU.*,E.Active FROM [AppUsers] AU LEFT JOIN [Employees] AS E ON  E.UserId=AU.Id  WHERE AU.[Id] = @{nameof(ApplicationUser.Id)}", user);
        //    }

        //    return res;
        //}

        public async Task<IList<ApplicationUser>> GetUsersByRoleName(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var queryResults = await connection.QueryAsync<ApplicationUser>($@"SELECT [DisplayName], AU.[Id]
                    FROM dbo.AppRoles as AR
                    INNER JOIN dbo.[AppUserRoles] AS AUR ON AR.[Id] = AUR.[RoleId]
                    INNER JOIN dbo.[AppUsers] AS AU ON AUR.[UserId] = AU.[Id]
                    WHERE [Name] = @roleName AND AU.[Blocked] = 0", new { roleName = roleName });

                return queryResults.ToList();
            }
        }

        public async Task<ApplicationUser> FindUniqueByNameAsync(string normalizedUserName, string userId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers] 
                    WHERE [NormalizedUserName] = @{nameof(normalizedUserName)} and [id]<>@{nameof(userId)}", new { normalizedUserName, userId });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> FindUniqueByEmailAsync(string email, string userId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers] 
                    WHERE [Email] = @{nameof(email)} and [id]<>@{nameof(userId)}", new { email, userId });
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($@"UPDATE [AppUsers] SET
                    [UserName] = @{nameof(ApplicationUser.UserName)},
                    [NormalizedUserName] = @{nameof(ApplicationUser.NormalizedUserName)},
                    [Email] = @{nameof(ApplicationUser.Email)},
                    [NormalizedEmail] = @{nameof(ApplicationUser.NormalizedEmail)},
                    [EmailConfirmed] = @{nameof(ApplicationUser.EmailConfirmed)},
                    [PhoneNumber] = @{nameof(ApplicationUser.PhoneNumber)},
                    [PasswordHash] = @{nameof(ApplicationUser.PasswordHash)},
                    [PhoneNumberConfirmed] = @{nameof(ApplicationUser.PhoneNumberConfirmed)},
                    [TwoFactorEnabled] = @{nameof(ApplicationUser.TwoFactorEnabled)},
                    [PasswordRecovery] = @{nameof(ApplicationUser.PasswordRecovery)},
[DisplayName] = @{nameof(ApplicationUser.DisplayName)}
                    WHERE [Id] = @{nameof(ApplicationUser.Id)}", user);
            }

            return IdentityResult.Success;
        }

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers]
                    WHERE [NormalizedEmail] = @{nameof(normalizedEmail)} and [delete]=0", new { normalizedEmail });
            }
        }

        public async Task<ApplicationUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>($@"SELECT * FROM [AppUsers] 
                    WHERE [PHONENUMBER] = @{nameof(phoneNumber)} AND [DELETE]=0", new { phoneNumber });
            }
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var normalizedName = roleName.ToUpper();
                var roleId = await connection.ExecuteScalarAsync<int?>($"SELECT [Id] FROM [AppRoles] WHERE [NormalizedName] = @{nameof(normalizedName)}", new { normalizedName });
                if (!roleId.HasValue)
                    roleId = await connection.QuerySingleAsync<int>(@$"INSERT INTO [AppRoles]([Name], [NormalizedName]) VALUES(@{nameof(roleName)}, @{nameof(normalizedName)}) 
                        SELECT SCOPE_IDENTITY()",
                        new { roleName, normalizedName });

                await connection.ExecuteAsync($"IF NOT EXISTS(SELECT 1 FROM [AppUserRoles] WHERE [UserId] = @userId AND [RoleId] = @{nameof(roleId)}) " +
                    $"INSERT INTO [AppUserRoles]([UserId], [RoleId]) VALUES(@userId, @{nameof(roleId)})",
                    new { userId = user.Id, roleId });
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var roleId = await connection.ExecuteScalarAsync<int?>("SELECT [Id] FROM [AppRoles] WHERE [NormalizedName] = @normalizedName", new { normalizedName = roleName.ToUpper() });
                if (roleId.HasValue)
                    await connection.ExecuteAsync($"DELETE FROM [AppUserRoles] WHERE [UserId] = @userId AND [RoleId] = @{nameof(roleId)}", new { userId = user.Id, roleId });
            }
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var queryResults = await connection.QueryAsync<string>("SELECT   r.[Name],r.[NormalizedName],r.[Id] FROM [AppRoles] r INNER JOIN [AppUserRoles] ur ON ur.[RoleId] = r.Id " +
                    "WHERE ur.UserId = @userId", new { userId = user.Id });

                return queryResults.ToList();
            }
        }

        //public async Task<IList<ApplicationRoleUI>> GetRolesByIdAsync(string userId, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        var queryResults = await connection.QueryAsync<ApplicationRoleUI>("SELECT   r.[Name],r.[NormalizedName],r.[Id] FROM [AppRoles] r INNER JOIN [AppUserRoles] ur ON ur.[RoleId] = r.Id " +
        //            "WHERE ur.UserId = @userId", new { userId = userId });

        //        return queryResults.ToList();
        //    }
        //}

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                var roleId = await connection.ExecuteScalarAsync<int?>("SELECT [Id] FROM [AppRoles] WHERE [NormalizedName] = @normalizedName", new { normalizedName = roleName.ToUpper() });
                if (roleId == default(int)) return false;
                var matchingRoles = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM [AppUserRoles] WHERE [UserId] = @userId AND [RoleId] = @{nameof(roleId)}",
                    new { userId = user.Id, roleId });

                return matchingRoles > 0;
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                var queryResults = await connection.QueryAsync<ApplicationUser>("SELECT u.* FROM [ApplicationUser] u " +
                    "INNER JOIN [AppUserRoles] ur ON ur.[UserId] = u.[Id] INNER JOIN [AppRoles] r ON r.[Id] = ur.[RoleId] WHERE r.[NormalizedName] = @normalizedName",
                    new { normalizedName = roleName.ToUpper() });

                return queryResults.ToList();
            }
        }

        public async Task<ApplicationUserToken> FindTokenAsync(
            ApplicationUser user,
            string loginProvider,
            string name,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<ApplicationUserToken>("SELECT * FROM AppUserTokens WHERE userId = @userId && loginProvider = @loginProvider && Name = @name",
                    new { userId = user.Id, loginProvider = loginProvider, name = name });

                return result;
            }
        }


        public Task SetTokenAsync(ApplicationUserToken user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTokenAsync(ApplicationUserToken user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetTokenAsync(ApplicationUserToken user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<ApplicationUserToken>("SELECT * FROM AppUserTokens WHERE userId = @userId && loginProvider = @loginProvider && Name = @name",
                    new { userId = user.Id, loginProvider = loginProvider, name = name });

                return result.Value;
            }
        }

        public Task<string> GetUserIdAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(ApplicationUserToken user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(ApplicationUserToken user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUserToken user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<ApplicationUserToken> IUserStore<ApplicationUserToken>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<ApplicationUserToken> IUserStore<ApplicationUserToken>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            // Nothing to dispose.
        }


    }
}
