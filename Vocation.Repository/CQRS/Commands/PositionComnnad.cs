using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Commands
{
    public interface IPostionCommand
    {
        Task<Position> AddAsync(Position item);
        Task<bool> UpdateAsync(Position item);
        Task<bool> DeleteAsync(string id);
    }
    public class PostionCommand : IPostionCommand
    {
        private readonly IUnitOFWork unitOfWork;
        public PostionCommand(IUnitOFWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        private readonly string AddSql = @"Insert into EmployeePosition (Id,Code,Name,Info,Status,DeleteStatus)
	                                       values(NEWID(),@Code,@Name,@Info,@Status,0)
                                           Select Top(1) * from EmployeePosition order by RowNum desc";

        private readonly string SameEntity = @"Select Case When Exists (
                                               SELECT * FROM EmployeePosition WHERE Code=@Code 
                                               AND Name=@Name AND DeleteStatus=0 )
                                               Then Cast(1 AS BIT)
                                               Else Cast(0 AS BIT) End";
        private readonly string SameEntityUpdate = @"Select Case When Exists (
                                               SELECT * FROM EmployeePosition WHERE Code=@Code 
                                               AND Name=@Name AND DeleteStatus=0 AND Id!=@Id)
                                               Then Cast(1 AS BIT)
                                               Else Cast(0 AS BIT) End";

        private readonly string UpdateSql = @" Update EmployeePosition Set Code=@Code,
							                    Name=@Name,
							                    Info=@Info,
                                                Status=@Status
						                        Where Id=@Id";

        private readonly string DeleteSql = @"Update EmployeePosition
			                                  Set DeleteStatus=1
			                                  Where Id=@Id";

        private readonly string ExistsinEmployee = @"Select Case When Exists (
                                                                Select * From Employees 
                                                                Where PositionId =@Id
                                                                            )
                                                            Then Cast(1 AS BIT)
                                                            Else Cast(0 AS BIT) End";

        public async Task<Position> AddAsync(Position item)
        {
            try
            {
                var existinData = unitOfWork.GetConnection().QueryFirstOrDefault<bool>(SameEntity, item, unitOfWork.GetTransaction());
                if (!existinData)
                {
                    var data = await unitOfWork.GetConnection().QuerySingleAsync<Position>(AddSql, item, unitOfWork.GetTransaction());
                    return data;
                }
                else return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateAsync(Position item)
        {
            try
            {
                var existinData = unitOfWork.GetConnection().QueryFirstOrDefault<bool>(SameEntityUpdate, item, unitOfWork.GetTransaction());
                if (!existinData)
                {
                    var result = await unitOfWork.GetConnection().QueryAsync(UpdateSql, item, unitOfWork.GetTransaction());
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var existinEmployee = unitOfWork.GetConnection().QueryFirstOrDefault<bool>(ExistsinEmployee, new { id }, unitOfWork.GetTransaction());
                if (!existinEmployee)
                {
                    await unitOfWork.GetConnection().ExecuteAsync(DeleteSql, new { id }, unitOfWork.GetTransaction());
                    return true;
                }
                {
                    return false;
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
