using Dapper;
using System;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IDepartmentCommand
    {
        Task<Guid> Add(Department model);
        Task Update(Department model);
    }

    public class DepartmentCommand : IDepartmentCommand
    {
        private readonly IUnitOFWork _unitOfWork;

        public DepartmentCommand(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string _addSql = $@"
            INSERT Into Departments(ShortName,FullName,Notes,CreatedDate,DeleteStatus)
                                        OUTPUT Inserted.Id
                                        VALUES(@{nameof(Department.ShortName)},
                                               @{nameof(Department.FullName)},
                                               @{nameof(Department.Notes)},
                                               GetDate(),
                                               0)
        ";

        private string _updateSql = $@"UPDATE Departments SET 
                                    ShortName = @{nameof(Department.ShortName)},
                                    FullName = @{nameof(Department.FullName)},
                                    Notes = @{nameof(Department.Notes)}
                                    WHERE Id = @{nameof(Department.Id)}";

        public async Task<Guid> Add(Department model)
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QuerySingleAsync<Guid>(_addSql, model, _unitOfWork.GetTransaction());
                return result;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task Update(Department model)
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync(_updateSql, model, _unitOfWork.GetTransaction());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    
    }
}
