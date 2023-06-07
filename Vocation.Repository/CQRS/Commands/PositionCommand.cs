using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IPositionCommand
    {
        Task<Guid> Add(Position model);
        Task Update(Position model);
        Task<bool> DeleteAsync(string id);
    }

    public class PositionCommand : IPositionCommand
    {

        private readonly IUnitOFWork _unitOfWork;

        public PositionCommand(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        private string _addSql = $@"
            INSERT Into Positions(Name,CreatedDate, DeleteStatus, DepartmentId)
                                        OUTPUT Inserted.Id
                                        VALUES(@{nameof(Position.Name)},
                                               GetDate(),0,
                                               @{nameof(Position.DepartmentId)})
        ";

        private string _updateSql = $@"UPDATE Positions SET 
                                    Name = @{nameof(Position.Name)},
                                    DepartmentId=@{nameof(Position.DepartmentId)}
                                    WHERE Id = @{nameof(Department.Id)}";

        private string _deleteSql = $@"UPDATE Positions SET DeleteStatus=1 WHERE Id=@Id";

        public async Task<Guid> Add(Position model)
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

        public async Task Update(Position model)
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

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                await _unitOfWork.GetConnection().ExecuteAsync(_deleteSql, new { id }, _unitOfWork.GetTransaction());
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
