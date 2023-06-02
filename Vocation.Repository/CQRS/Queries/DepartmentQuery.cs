using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Queries
{
    public interface IDepartmentQuery
    {
        Task<IEnumerable<Department>> GetAll();
    }

    public class DepartmentQuery : IDepartmentQuery
    {
        private readonly IUnitOFWork _unitOfWork;

        public DepartmentQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _getAll = $@"SELECT * FROM Departments WHERE DeleteStatus = 0 ";

        public async Task<IEnumerable<Department>> GetAll()
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync<Department>(_getAll, null, _unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
