using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.CQRS.Commands;
using Vocation.Repository.CQRS.Queries;

namespace Vocation.Repository.Repositories
{
    public interface IDepartmentRepository
    {
        Task<Guid> Add(Department model);
        Task Update(Department model);
        Task<IEnumerable<Department>> GetAll();
    }

    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDepartmentCommand _departmentCommand;
        private readonly IDepartmentQuery _departmentQuery;

        public DepartmentRepository(IDepartmentCommand departmentCommand, IDepartmentQuery departmentQuery)
        {
            _departmentCommand = departmentCommand;
            _departmentQuery = departmentQuery;
        }

        public async Task<Guid> Add(Department model)
        {
            var result = await _departmentCommand.Add(model);
            return result;
        }

        public async Task<IEnumerable<Department>> GetAll()
        {
            var result = await _departmentQuery.GetAll();
            return result;
        }

        public async Task Update(Department model)
        {
            await _departmentCommand.Update(model);
        }
    }
}
