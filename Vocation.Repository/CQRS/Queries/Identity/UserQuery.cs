using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vocation.Core.Models.Helpers;
using Vocation.Repository.Infrastucture;
using Vocation.Repository.Infrastucture.Models;

namespace Vocation.Repository.CQRS.Queries.Identity
{
    public interface IUserQuery
    {
        Task<ListResult<UserEmployeeModel>> GetAllByNameAsync(string name, int offset, int limit);
        Task<IEnumerable<UserEmployeeModel>> GetActiveUsers();
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit);
        Task<IEnumerable<UserEmployeeModel>> GetAllUsers();
        Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit);
        Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit);
    }

    public class UserQuery : IUserQuery
    {
        private readonly IUnitOFWork _unitOfWork;

        private const string getSearchPaginationSql = @"
                                                      DECLARE @SEARCHTEXT NVARCHAR(MAX)
                                                      SET @SEARCHTEXT = '%' + @search+ '%'
                                                      
                                                      SELECT
                                                      U.*,
                                                      E.InitialBalance,
                                                      E.Salary,
                                                      E.StartDate,
                                                      E.EndDate,
                                                      E.Active,
                                                      E.Info,
                                                      E.IsHead,
                                                      E.PermissionLimit,
                                                      E.PositionId,
                                                      E.CityId,
                                                      C.NAME    AS CITY,
                                                      EP.[NAME] AS POSITION
                                                      FROM dbo.AppUsers AS U
                                                      LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                      LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                      LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                      WHERE U.[Delete]=0 AND (
                                                      U.[UserName] LIKE @SEARCHTEXT OR
                                                      U.[Email] LIKE @SEARCHTEXT OR
                                                      U.[PhoneNumber] LIKE @SEARCHTEXT OR
                                                      U.[DisplayName] LIKE @SEARCHTEXT OR
                                                      E.NAME LIKE @SEARCHTEXT
                                                      OR C.NAME LIKE @SEARCHTEXT
                                                      OR EP.NAME LIKE @SEARCHTEXT
                                                      OR E.PHONE LIKE @SEARCHTEXT
                                                      OR E.EMAIL LIKE @SEARCHTEXT
                                                      OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                      OR E.SALARY LIKE @SEARCHTEXT
                                                      OR E.INFO LIKE @SEARCHTEXT)
                                                      ORDER BY U.[RowNum] DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY
                                                      
                                                      SELECT COUNT(U.Id) TOTALCOUNT
                                                      FROM dbo.AppUsers U
                                                      LEFT JOIN dbo.Employees E ON E.UserId=U.Id
                                                      LEFT JOIN dbo.CITIES C ON E.CITYID = C.ID
                                                      LEFT JOIN dbo.EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID
                                                      WHERE U.[Delete]=0 AND (
                                                      U.[UserName] LIKE @SEARCHTEXT OR
                                                      U.[Email] LIKE @SEARCHTEXT OR
                                                      U.[PhoneNumber] LIKE @SEARCHTEXT OR
                                                      U.[DisplayName] LIKE @SEARCHTEXT OR
                                                      E.NAME LIKE @SEARCHTEXT
                                                      OR C.NAME LIKE @SEARCHTEXT
                                                      OR EP.NAME LIKE @SEARCHTEXT
                                                      OR E.PHONE LIKE @SEARCHTEXT
                                                      OR E.EMAIL LIKE @SEARCHTEXT
                                                      OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                      OR E.SALARY LIKE @SEARCHTEXT
                                                      OR E.INFO LIKE @SEARCHTEXT)";

        private const string getAllUserSql = @"SELECT
                                               U.*,
                                               E.InitialBalance,
                                               E.Salary,
                                               E.StartDate,
                                               E.EndDate,
                                               E.Active,
                                               E.Info,
                                               E.IsHead,
                                               E.PermissionLimit,
                                               E.PositionId,
                                               E.CityId,
                                               C.NAME    AS CITY,
                                               EP.[NAME] AS POSITION
                                               FROM dbo.AppUsers AS U
                                               LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                               LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                               LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                               WHERE U.[Delete]=0 ORDER BY U.RowNum DESC";

        private const string getPaginationSql = @"SELECT U.*,
                                                  E.InitialBalance,
                                                  E.Salary,
                                                  E.StartDate,
                                                  E.EndDate,
                                                  E.Active,
                                                  E.Info,
                                                  E.IsHead,
                                                  E.PermissionLimit,
                                                  E.PositionId,
                                                  E.CityId,
                                                  C.NAME    AS CITY,
                                                  EP.[NAME] AS POSITION
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0
                                                  ORDER BY U.RowNum DESC
                                                  OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY
                                                  
                                                  SELECT COUNT(U.Id) TOTALCOUNT
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0";


        private const string getActiveUserSql = @"SELECT U.*,
                                                  E.InitialBalance,
                                                  E.Salary,
                                                  E.StartDate,
                                                  E.EndDate,
                                                  E.Active,
                                                  E.Info,
                                                  E.IsHead,
                                                  E.PermissionLimit,
                                                  E.PositionId,
                                                  E.CityId,
                                                  C.NAME    AS CITY,
                                                  EP.[NAME] AS POSITION
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0 AND E.Active=1 
                                                  ORDER BY U.RowNum DESC";

        private const string getActiveUserPaginationSql = @"SELECT U.*,
                                                  E.InitialBalance,
                                                  E.Salary,
                                                  E.StartDate,
                                                  E.EndDate,
                                                  E.Active,
                                                  E.Info,
                                                  E.IsHead,
                                                  E.PermissionLimit,
                                                  E.PositionId,
                                                  E.CityId,
                                                  C.NAME    AS CITY,
                                                  EP.[NAME] AS POSITION
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0 AND E.Active=1
                                                  ORDER BY U.RowNum DESC
                                                  OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY
                                                  
                                                  SELECT COUNT(U.Id) TOTALCOUNT
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0 AND E.Active=1";

        private const string getBlockedUserPaginationSql = @"SELECT U.*,
                                                  E.InitialBalance,
                                                  E.Salary,
                                                  E.StartDate,
                                                  E.EndDate,
                                                  E.Active,
                                                  E.Info,
                                                  E.IsHead,
                                                  E.PermissionLimit,
                                                  E.PositionId,
                                                  E.CityId,
                                                  C.NAME    AS CITY,
                                                  EP.[NAME] AS POSITION
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0 AND E.Active=0
                                                  ORDER BY U.RowNum DESC
                                                  OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY
                                                  
                                                  SELECT COUNT(U.Id) TOTALCOUNT
                                                  FROM dbo.AppUsers AS U
                                                  LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                  LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                  LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                  WHERE U.[Delete] = 0 AND E.Active=0";

        private const string getActiveUserSearchPaginationSql = @"
                                                      DECLARE @SEARCHTEXT NVARCHAR(MAX)
                                                      SET @SEARCHTEXT = '%' + @Search + '%'
                                                      
                                                      SELECT
                                                      U.*,
                                                      E.InitialBalance,
                                                      E.Salary,
                                                      E.StartDate,
                                                      E.EndDate,
                                                      E.Active,
                                                      E.Info,
                                                      E.IsHead,
                                                      E.PermissionLimit,
                                                      E.PositionId,
                                                      E.CityId,
                                                      C.NAME    AS CITY,
                                                      EP.[NAME] AS POSITION
                                                      FROM dbo.AppUsers AS U
                                                      LEFT JOIN dbo.Employees AS E ON E.UserId = U.Id
                                                      LEFT JOIN dbo.Cities AS C ON E.CityId = C.Id
                                                      LEFT JOIN dbo.EmployeePosition AS EP ON E.PositionId = EP.Id
                                                      WHERE U.[Delete]=0 AND E.Active=1 AND 
                                                      (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=U.Id AND AUR.RoleId=61)=0
                                                      AND (U.[UserName] LIKE @SEARCHTEXT OR
                                                      U.[Email] LIKE @SEARCHTEXT OR
                                                      U.[PhoneNumber] LIKE @SEARCHTEXT OR
                                                      U.[DisplayName] LIKE @SEARCHTEXT OR
                                                      E.NAME LIKE @SEARCHTEXT
                                                      OR C.NAME LIKE @SEARCHTEXT
                                                      OR EP.NAME LIKE @SEARCHTEXT
                                                      OR E.PHONE LIKE @SEARCHTEXT
                                                      OR E.EMAIL LIKE @SEARCHTEXT
                                                      OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                      OR E.SALARY LIKE @SEARCHTEXT
                                                      OR E.INFO LIKE @SEARCHTEXT)
                                                      ORDER BY U.[RowNum] DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY
                                                      
                                                      SELECT COUNT(U.Id) TOTALCOUNT
                                                      FROM dbo.AppUsers U
                                                      LEFT JOIN dbo.Employees E ON E.UserId=U.Id
                                                      LEFT JOIN dbo.CITIES C ON E.CITYID = C.ID
                                                      LEFT JOIN dbo.EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID
                                                      WHERE U.[Delete]=0 AND E.Active=1 AND 
                                                      (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=U.Id AND AUR.RoleId=61)=0 AND (
                                                      U.[UserName] LIKE @SEARCHTEXT OR
                                                      U.[Email] LIKE @SEARCHTEXT OR
                                                      U.[PhoneNumber] LIKE @SEARCHTEXT OR
                                                      U.[DisplayName] LIKE @SEARCHTEXT OR
                                                      E.NAME LIKE @SEARCHTEXT
                                                      OR C.NAME LIKE @SEARCHTEXT
                                                      OR EP.NAME LIKE @SEARCHTEXT
                                                      OR E.PHONE LIKE @SEARCHTEXT
                                                      OR E.EMAIL LIKE @SEARCHTEXT
                                                      OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                      OR E.SALARY LIKE @SEARCHTEXT
                                                      OR E.INFO LIKE @SEARCHTEXT)";

        public UserQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllByNameAsync(string name, int offset, int limit)
        {
            var query = "";
            try
            {
                if (name != null)
                {
                    query = @"DECLARE   @SEARCHTEXT NVARCHAR(MAX)
                                                        SET @SEARCHTEXT='%' + @SEARCH + '%' 

                                                        SELECT U.* , E.InitialBalance,
                                                        E.Salary , E.StartDate,
                                                        E.EndDate , E.Active, E.Info,
                                                        E.IsHead ,E.PermissionLimit , E.PositionId , E.CityId,
                                                        CC.NAME CITY,
                                                        EP.[NAME] POSITION
                                                        
                                                        FROM AppUsers U
                                                        LEFT JOIN Employees E ON E.UserId=U.Id
                                                        LEFT JOIN CITIES CC ON E.CITYID = CC.ID
                                                        LEFT JOIN EMPLOYEEPOSITION EP 
                                                                   ON E.POSITIONID = EP.ID
                                                        WHERE [Delete]=0
                                                        AND( 
								                        U.[DisplayName] LIKE @SEARCHTEXT )
			                                            ORDER BY U.[RowNum] DESC OFFSET @OFFSET ROWS                                    
                                                                FETCH NEXT @LIMIT ROWS ONLY

                                                        SELECT COUNT(U.Id) TOTALCOUNT
                                                        FROM AppUsers U
                                                        LEFT JOIN Employees E ON E.UserId=U.Id
                                                        LEFT JOIN CITIES CC ON E.CITYID = CC.ID
                                                        LEFT JOIN EMPLOYEEPOSITION EP 
                                                                    ON E.POSITIONID = EP.ID
                                                        WHERE [Delete]=0 AND(U.[DisplayName] LIKE @SEARCHTEXT )";
                }
                else
                {
                    query = @"SELECT U.* , E.InitialBalance,
                                                        E.Salary , E.StartDate,
                                                        E.EndDate , E.Active, E.Info,
                                                        E.IsHead ,E.PermissionLimit , E.PositionId , E.CityId,
                                                        CC.NAME CITY,
                                                        EP.[NAME] POSITION

                                                        FROM AppUsers U
                                                        LEFT JOIN Employees E ON E.UserId = U.Id
                                                        LEFT JOIN CITIES CC ON E.CITYID = CC.ID
                                                        LEFT JOIN EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID
                                                        WHERE [Delete] = 0
                                                        ORDER BY U.[RowNum] DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 

                            SELECT COUNT(U.Id) TOTALCOUNT
                                                        FROM AppUsers U
                                                        LEFT JOIN Employees E ON E.UserId=U.Id
                                                        LEFT JOIN CITIES CC ON E.CITYID = CC.ID
                                                        LEFT JOIN EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID
                                                        WHERE [Delete]=0";
                }

                var parameters = new { search = name, offset, limit };
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(query, parameters, _unitOfWork.GetTransaction());

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception ex)
            {
                throw null;
            }
        }

        public async Task<IEnumerable<UserEmployeeModel>> GetActiveUsers()
        {
            try
            {
                var result = await _unitOfWork.GetConnection()
                    .QueryAsync<UserEmployeeModel>(getActiveUserSql, null, _unitOfWork.GetTransaction());

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(int offset, int limit)
        {
            var parameters = new
            {
                offset,
                limit,
            };
            try
            {
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(getActiveUserPaginationSql, parameters, _unitOfWork.GetTransaction());
                ;

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetBlockedUsers(int offset, int limit)
        {
            var parameters = new
            {
                offset,
                limit,
            };
            try
            {
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(getBlockedUserPaginationSql, parameters, _unitOfWork.GetTransaction());
                ;

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetActiveUsers(string searchText, int offset, int limit)
        {
            var parameters = new
            {
                offset,
                limit,
                search = searchText,
            };
            try
            {
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(getActiveUserSearchPaginationSql, parameters, _unitOfWork.GetTransaction());
                ;

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<UserEmployeeModel>> GetAllUsers()
        {
            try
            {
                var result = await _unitOfWork.GetConnection()
                    .QueryAsync<UserEmployeeModel>(getAllUserSql, null, _unitOfWork.GetTransaction());

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(int offset, int limit)
        {
            var parameters = new
            {
                offset,
                limit
            };
            try
            {
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(getPaginationSql, parameters, _unitOfWork.GetTransaction());
                ;

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ListResult<UserEmployeeModel>> GetAllUsers(string searchText, int offset, int limit)
        {
            var parameters = new
            {
                offset,
                limit,
                search = searchText ?? "",
            };
            try
            {
                var grid = await _unitOfWork.GetConnection()
                    .QueryMultipleAsync(getSearchPaginationSql, parameters, _unitOfWork.GetTransaction());
                ;

                var result = new ListResult<UserEmployeeModel>
                {
                    List = grid.Read<UserEmployeeModel>(),
                    TotalCount = grid.ReadFirst<int>()
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
