using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.CQRS.Commands;
using Vocation.Repository.CQRS.Queries;

namespace Vocation.Repository.Repositories
{
    public interface IEmployeeRepository : IEmployeeQuery, IEmployeeCommand { }

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IEmployeeQuery _employeeQuery;
        private readonly IEmployeeCommand _employeeCommand;

        public EmployeeRepository(IEmployeeCommand employeeCommand, IEmployeeQuery employeeQuery)
        {
            _employeeCommand = employeeCommand;
            _employeeQuery = employeeQuery;
        }

        public async Task<Guid> AddAsync(Employee item)
        {
            var result = await _employeeCommand.AddAsync(item);
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _employeeCommand.DeleteAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _employeeQuery.GetAllAsync();
        }

        public async Task<ListResult<Employee>> GetAllPaginationAsync(string searchtext, int offset, int limit)
        {
            var result = await _employeeQuery.GetAllPaginationAsync(searchtext, offset, limit);
            return result;
        }

        public async Task<ListResult<Employee>> GetByHeading(string searchtext, int offset, int limit)
        {
            var result = await _employeeQuery.GetByHeading(searchtext, offset, limit);
            return result;
        }

        public async Task<Employee> GetByIdAsync(string Id)
        {
            var result = await _employeeQuery.GetByIdAsync(Id);
            return result;
        }

        public async Task<ListResult<Employee>> GetByTeamMembersAsync(string searchtext, int offset, int limit)
        {
            var result = await _employeeQuery.GetByTeamMembersAsync(searchtext, offset, limit);
            return result;
        }

        public async Task<Employee> GetByUserIdAsync(string Id)
        {
            var result = await _employeeQuery.GetByUserIdAsync(Id);
            return result;
        }
        public async Task UpdateAsync(Employee item)
        {
            await _employeeCommand.UpdateAsync(item);
        }
    }
}
