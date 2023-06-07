using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.Infrastucture;

namespace Vocation.Repository.CQRS.Queries
{
    public interface IPositionQuery
    {
        Task<IEnumerable<Position>> GetAllAsync();
        Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit);
        Task<Position> GetByIdAsync(string id);
    }
    public class PositionQuery : IPositionQuery
    {
        private readonly IUnitOFWork unitOfWork;

        private readonly string GetAllSql = $@"SELECT P.*, D.ShortName DepartmentName FROM Positions P
                                            Left Join Departments D on D.Id=P.DepartmentId
                                            WHERE P.DELETESTATUS=0 ";

        private readonly string GetPaginationSql = @"SELECT * FROM Positions
		                                                    WHERE DELETESTATUS=0  
		                                                    ORDER BY ROWNUM DESC OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY
                                
		                                                    SELECT COUNT(Id) TotalCount
		                                                    FROM  Positions
		                                                    WHERE DeleteStatus = 0
                                                            ";

        private readonly string GetByIdSql = "SELECT * FROM Positions WHERE DeleteStatus=0 AND Id=@Id";

        private readonly string GetFullSearchSql = @"DECLARE @searchtext NVARCHAR(MAX) SET @searchtext='%' + @search + '%'
                                                    SELECT * FROM EmployeePosition WHERE DeleteStatus=0 and
                                                        (Name like @searchtext or Code like @searchtext or Info like @searchtext)
                                                    ORDER BY RowNum OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY
                                                    SELECT COUNT(Id) TotalCount From EmployeePosition Where DeleteStatus=0 and
                                                        (Name like @searchtext or Code like @searchtext or Info like @searchtext)";

        public PositionQuery(IUnitOFWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            try
            {
                var result = await unitOfWork.GetConnection().QueryAsync<Position>(GetAllSql, null, unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<Position> GetByIdAsync(string id)
        {
            try
            {
                var result = await unitOfWork.GetConnection().QuerySingleAsync<Position>(GetByIdSql, new { id }, unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<ListResult<Position>> GetPaginationAsync(string searchtext, int offset, int limit)
        {


            try
            {
                if (!String.IsNullOrWhiteSpace(searchtext))
                {
                    var par = new { search = searchtext, offset, limit };
                    var res = await unitOfWork.GetConnection().QueryMultipleAsync(GetFullSearchSql, par, unitOfWork.GetTransaction());
                    var result = new ListResult<Position>
                    {
                        List = res.Read<Position>(),
                        TotalCount = res.ReadFirst<int>()
                    };
                    return result;
                }
                else
                {
                    var par = new { offset, limit };
                    var res = await unitOfWork.GetConnection().QueryMultipleAsync(GetPaginationSql, par, unitOfWork.GetTransaction());
                    var result = new ListResult<Position>
                    {
                        List = res.Read<Position>(),
                        TotalCount = res.ReadFirst<int>()
                    };
                    return result;
                }


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
