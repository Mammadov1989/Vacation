using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.CQRS.Queries.Identity;
using Vocation.Repository.Infrastucture.Models;

namespace Vocation.Repository.Repositories.Identity
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserEmployeeModel>> GetActiveUsers();
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit);
        Task<IEnumerable<UserEmployeeModel>> GetAllUsers();
        Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IUserQuery _userQuery;

        public UserRepository(IUserQuery userQuery)
        {
            _userQuery = userQuery;
        }
        public async Task<ListResult<UserEmployeeModel>> GetAllByNameAsync(string name, int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetAllByNameAsync(name, offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<IEnumerable<UserEmployeeModel>> GetActiveUsers()
        {
            try
            {
                var result = await _userQuery.GetActiveUsers();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetActiveUsers(offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetActiveUsers(searchText, offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<IEnumerable<UserEmployeeModel>> GetAllUsers()
        {
            try
            {
                var result = await _userQuery.GetAllUsers();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetAllUsers(offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetAllUsers(searchText, offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit)
        {
            try
            {
                var result = await _userQuery.GetBlockedUsers(offset, limit);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
