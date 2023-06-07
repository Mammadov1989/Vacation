using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IEmployeeCommand
    {
        Task<Guid> AddAsync(Employee item);
        Task UpdateAsync(Employee item);
        Task<bool> DeleteAsync(string id);
    }

    public class EmployeeCommand : IEmployeeCommand
    {
        private readonly IUnitOFWork _unitOfWork;

        private string addSql = $@"
                    Insert into Employee (
                             Id
                            ,UserId
                            ,Name
                            ,SurName
                            ,PositionId
                            ,DepartmentId
                            ,Email
                            ,DeleteStatus)
                    Output Inserted.Id 
                    Values (
                                NewId(),
                                @{nameof(Employee.UserId)},
                                @{nameof(Employee.Name)},
                                @{nameof(Employee.SurName)},
                                @{nameof(Employee.PositionId)},
                                @{nameof(Employee.DepartmentId)},
                                @{nameof(Employee.Email)},0)";

        private string updateSql = $@"
                            Update Employee Set
                             Name=@{nameof(Employee.Name)},
                             SurName=@{nameof(Employee.SurName)}
                            ,PositionId=@{nameof(Employee.PositionId)}
                            ,DepartmentId=@{nameof(Employee.DepartmentId)}
                            ,Email=@{nameof(Employee.Email)}
                            Where Id=@{nameof(Employee.Id)}";

        private string deleteSql = $@"Update Employee Set
                                            DeleteStatus=1
                                                 Where Id=@{nameof(Employee.Id)}";

        private string ExistsInEmployeeSalaries = $@"Select Case When Exists (
Select * From EmployeeSalaries 
Where EmployeeId =@{nameof(Employee.Id)}
)

Then Cast(1 AS BIT)
Else Cast(0 AS BIT) End";

        private string ExistsInEmployeeUsers = $@"SELECT CASE WHEN EXISTS (
SELECT * FROM Employees WHERE UserId = @{nameof(Employee.UserId)})

THEN CAST (1 AS BIT)
ELSE CAST (0 AS BIT) END";

        public EmployeeCommand(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> AddAsync(Employee item)
        {
            try
            {
                var exists = await _unitOfWork.GetConnection().QueryFirstOrDefaultAsync<bool>(ExistsInEmployeeUsers, new { item.UserId }, _unitOfWork.GetTransaction());

                if (exists) return Guid.Empty;
                var result = await _unitOfWork.GetConnection().QuerySingleAsync<Guid>(addSql, item, _unitOfWork.GetTransaction());
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                await _unitOfWork.GetConnection().QueryAsync(deleteSql, new { id }, _unitOfWork.GetTransaction());
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateAsync(Employee item)
        {
            try
            {
                var result = await _unitOfWork.GetConnection().QueryAsync(updateSql, item, _unitOfWork.GetTransaction());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
