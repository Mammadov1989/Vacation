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
    public interface IEmployeeQuery
    {
        Task<Employee> GetByIdAsync(string Id);
        Task<Employee> GetByUserIdAsync(string UserId);
        Task<ListResult<Employee>> GetByTeamMembersAsync(string searchtext, int offset, int limit);

        Task<IEnumerable<Employee>> GetAllAsync();

        Task<ListResult<Employee>> GetAllPaginationAsync(string searchtext, int offset, int limit);

        Task<ListResult<Employee>> GetByHeading(string searchtext, int offset, int limit);
    }

    public class EmployeeQuery : IEmployeeQuery
    {
        private readonly IUnitOFWork _unitOfWork;

        private readonly string getAllSql = @"
                                                SELECT 
                                                  Distinct (E.Name),
                                                  E.*, 
                                                  EP.[NAME] Position  ,
                                                  AU.UserName UserName  ,
                                                  D.ShortName Department
                                                FROM 
                                                  EMPLOYEE E 
                                                  LEFT JOIN Positions EP ON E.POSITIONID = EP.ID
                                                  LEFT JOIN Departments D ON E.DepartmentId = D.ID
                                                  LEFT JOIN AppUsers AU ON E.UserId = AU.ID
                                                WHERE 
                                                  E.DELETESTATUS = 0";

        private readonly string getAllPagingSearch = @"
                                                    DECLARE @SEARCHTEXT NVARCHAR(MAX) 
                                                    SET 
                                                      @SEARCHTEXT = '%' + @SEARCH + '%' 
                                                    SELECT 
                                                      Distinct (E.Name),
                                                      E.*, 
                                                      EP.[NAME] POSITION 
                                                    FROM 
                                                      EMPLOYEES E 
                                                      LEFT JOIN Positions EP ON E.POSITIONID = EP.ID
												      LEFT JOIN AppUserRoles AUR ON E.UserId=AUR.UserId
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND E.Active=1
                                                      AND (
                                                        E.NAME LIKE @SEARCHTEXT 
                                                        OR CC.NAME LIKE @SEARCHTEXT 
                                                        OR EP.NAME LIKE @SEARCHTEXT 
                                                        OR E.PHONE LIKE @SEARCHTEXT 
                                                        OR E.EMAIL LIKE @SEARCHTEXT 
                                                        OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                        OR E.SALARY LIKE @SEARCHTEXT
                                                        OR E.INFO LIKE @SEARCHTEXT
                                                      ) 
                                                    ORDER BY 
                                                      E.ROWNUM DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
                                                    SELECT 
                                                       COUNT(DISTINCT(E.Name)) TOTALCOUNT
                                                    FROM 
                                                      EMPLOYEES E 
                                                      LEFT JOIN CITIES CC ON E.CITYID = CC.ID 
                                                      LEFT JOIN Positions EP ON E.POSITIONID = EP.ID 
												      LEFT JOIN AppUserRoles AUR ON E.UserId=AUR.UserId
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND E.Active=1
                                                      AND (
                                                        E.NAME LIKE @SEARCHTEXT 
                                                        OR CC.NAME LIKE @SEARCHTEXT 
                                                        OR EP.NAME LIKE @SEARCHTEXT 
                                                        OR E.PHONE LIKE @SEARCHTEXT 
                                                        OR E.EMAIL LIKE @SEARCHTEXT 
                                                        OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                        OR E.SALARY LIKE @SEARCHTEXT
                                                        OR E.INFO LIKE @SEARCHTEXT
                                                      )";

        private readonly string GetByHeadingSearchSql = @" DECLARE @SEARCHTEXT NVARCHAR(MAX) 
                                                    SET 
                                                      @SEARCHTEXT = '%' + @SEARCH + '%' 
                                                    SELECT 
                                                      E.*, 
                                                      CC.NAME CITY, 
                                                      EP.[NAME] POSITION 
                                                    FROM 
                                                      EMPLOYEES E 
                                                      LEFT JOIN CITIES CC ON E.CITYID = CC.ID 
                                                      LEFT JOIN AppUsers AU ON AU.ID = E.UserId
                                                      LEFT JOIN EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID 
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND AU.Blocked=0
                                                      AND E.isHead=1 AND (
                                                        E.NAME LIKE @SEARCHTEXT 
                                                        OR CC.NAME LIKE @SEARCHTEXT 
                                                        OR EP.NAME LIKE @SEARCHTEXT 
                                                        OR E.PHONE LIKE @SEARCHTEXT 
                                                        OR E.EMAIL LIKE @SEARCHTEXT 
                                                        OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                        OR E.SALARY LIKE @SEARCHTEXT
                                                        OR E.INFO LIKE @SEARCHTEXT
                                                      ) 
                                                    ORDER BY 
                                                      E.ROWNUM DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
                                                    SELECT 
                                                      COUNT(E.ID) TOTALCOUNT 
                                                    FROM 
                                                      EMPLOYEES E 
                                                      LEFT JOIN CITIES CC ON E.CITYID = CC.ID 
                                                      LEFT JOIN AppUsers AU ON AU.ID = E.UserId
                                                      LEFT JOIN EMPLOYEEPOSITION EP ON E.POSITIONID = EP.ID 
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND AU.Blocked=0
                                                      AND E.isHead=1 AND (
                                                        E.NAME LIKE @SEARCHTEXT 
                                                        OR CC.NAME LIKE @SEARCHTEXT 
                                                        OR EP.NAME LIKE @SEARCHTEXT 
                                                        OR E.PHONE LIKE @SEARCHTEXT 
                                                        OR E.EMAIL LIKE @SEARCHTEXT 
                                                        OR E.INITIALBALANCE LIKE @SEARCHTEXT
                                                        OR E.SALARY LIKE @SEARCHTEXT
                                                        OR E.INFO LIKE @SEARCHTEXT
                                                      ) ";

        private readonly string getAllPaging = @"SELECT 
                                                      Distinct (E.Name),
                                                      E.*, 
                                                      EP.[NAME] POSITION 
                                                    FROM 
                                                      EMPLOYEE E 
                                                      LEFT JOIN Positions EP ON E.POSITIONID = EP.ID
												      LEFT JOIN AppUserRoles AUR ON E.UserId=AUR.UserId
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND E.Active=1
                                                    ORDER BY 
                                                      E.Name DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
                                                    SELECT 
                                                       COUNT(DISTINCT(E.Name)) TOTALCOUNT
                                                    FROM 
                                                      EMPLOYEES E 
                                                      LEFT JOIN Positions EP ON E.POSITIONID = EP.ID 
												      LEFT JOIN AppUserRoles AUR ON E.UserId=AUR.UserId
                                                    WHERE 
                                                      E.DELETESTATUS = 0 AND E.Active=1";

        private readonly string getAllByHeadingPaging = @"
                                                SELECT 
                                                  E.*, 
                                                  CC.NAME CITY, 
                                                  EP.[NAME] POSITION  
                                                FROM 
                                                  EMPLOYEES E 
                                                  LEFT JOIN CITIES CC ON E.CITYID = CC.ID 
                                                  LEFT JOIN AppUsers AU ON AU.ID = E.UserId
                                                  LEFT JOIN Positions EP ON E.POSITIONID = EP.ID 
                                                WHERE AU.Blocked=0 AND
                                                  E.DELETESTATUS = 0 
                                                  AND E.isHead=1
                                                ORDER BY 
                                                  E.ROWNUM DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
                                                SELECT 
                                                  COUNT(E.ID) TOTALCOUNT 
                                                FROM 
                                                  EMPLOYEES E 
                                                LEFT JOIN AppUsers AU ON AU.ID = E.UserId
                                                WHERE 
                                                  E.DELETESTATUS = 0 AND AU.Blocked=0
                                                  AND E.isHead=1 ";

        private readonly string getById = @"     SELECT 
                                                  E.*, 
                                                  EP.[NAME] POSITION  
                                                FROM 
                                                  EMPLOYEE E 
                                                  LEFT JOIN Positions EP ON E.POSITIONID = EP.ID 
                                                WHERE 
                                                  E.DELETESTATUS = 0 AND E.Id=@ID";

        private readonly string getByUserId = @"     SELECT 
                                                  E.*, 
                                                  EP.[NAME] POSITION  
                                                FROM 
                                                  EMPLOYEE E 
                                                  LEFT JOIN positions EP ON E.POSITIONID = EP.ID 
                                                WHERE 
                                                  E.DELETESTATUS = 0 AND E.UserId=@ID";

        private readonly string getByTeamMembersSql = @"
        
        Select * from Employees E 
        Where E.DeleteStatus=0 AND E.Active=1 AND
        (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=E.UserId AND AUR.RoleId=61)=0
        AND E.Id Not In  (Select EmployeeId from TeamMembers WHERE DeleteStatus=0)
        ORDER BY E.ROWNUM DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
        SELECT  COUNT(E.ID) TOTALCOUNT  FROM 
        EMPLOYEES E 
        WHERE E.DELETESTATUS = 0 AND E.Active=1 AND 
        (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=E.UserId AND AUR.RoleId=61)=0 AND
        E.Id Not In (Select EmployeeId from TeamMembers WHERE DeleteStatus=0)";

        private readonly string getByTeamMembersSearch = @"
        DECLARE @SEARCHTEXT NVARCHAR(MAX) 
        SET @SEARCHTEXT = '%' + @SEARCH + '%'      

        Select * from Employees E
        Where E.DeleteStatus=0 AND
        (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=E.UserId AND AUR.RoleId=61)=0
        AND
        E.Id Not In (Select EmployeeId from TeamMembers WHERE DeleteStatus=0)
        AND (E.NAME LIKE @SEARCHTEXT 
        OR E.PHONE LIKE @SEARCHTEXT 
        OR E.EMAIL LIKE @SEARCHTEXT 
        OR E.INITIALBALANCE LIKE @SEARCHTEXT
        OR E.SALARY LIKE @SEARCHTEXT
        OR E.INFO LIKE @SEARCHTEXT)
        ORDER BY E.ROWNUM DESC OFFSET @OFFSET ROWS FETCH NEXT @LIMIT ROWS ONLY 
        SELECT COUNT(E.ID) TOTALCOUNT FROM EMPLOYEES E 
        WHERE E.DELETESTATUS = 0 AND 
        (SELECT COUNT(AUR.UserId) FROM dbo.AppUserRoles AUR WHERE AUR.UserId=E.UserId AND AUR.RoleId=61)=0
        AND E.Id Not In  (Select EmployeeId from TeamMembers WHERE DeleteStatus=0)
        AND ( E.NAME LIKE @SEARCHTEXT 
        OR E.PHONE LIKE @SEARCHTEXT 
        OR E.EMAIL LIKE @SEARCHTEXT 
        OR E.INITIALBALANCE LIKE @SEARCHTEXT
        OR E.SALARY LIKE @SEARCHTEXT
        OR E.INFO LIKE @SEARCHTEXT)";
        public EmployeeQuery(IUnitOFWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ListResult<Employee>> GetAllPaginationAsync(string searchtext, int offset, int limit)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(searchtext))
                {
                    var parameters = new
                    {
                        search = searchtext,
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection()
                        .QueryMultipleAsync(getAllPagingSearch, parameters, _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }
                else
                {
                    var parameters = new
                    {
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection()
                        .QueryMultipleAsync(getAllPaging, parameters, _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            try
            {
                var result = await _unitOfWork.GetConnection()
                    .QueryAsync<Employee>(getAllSql, null, _unitOfWork.GetTransaction());
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Employee> GetByIdAsync(string Id)
        {
            try
            {
                var parameters = new
                {
                    Id
                };
                var result = await _unitOfWork.GetConnection()
                    .QueryFirstOrDefaultAsync<Employee>(getById, parameters, _unitOfWork.GetTransaction());

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Employee> GetByUserIdAsync(string Id)
        {
            try
            {
                var parameters = new
                {
                    Id
                };
                var result = await _unitOfWork.GetConnection()
                    .QueryFirstOrDefaultAsync<Employee>(getByUserId, parameters, _unitOfWork.GetTransaction());

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<Employee>> GetByHeading(string searchtext, int offset, int limit)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(searchtext))
                {
                    var parameters = new
                    {
                        search = searchtext,
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection().QueryMultipleAsync(GetByHeadingSearchSql, parameters,
                        _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }
                else
                {
                    var parameters = new
                    {
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection().QueryMultipleAsync(getAllByHeadingPaging, parameters,
                        _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListResult<Employee>> GetByTeamMembersAsync(string searchtext, int offset, int limit)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(searchtext))
                {
                    var parameters = new
                    {
                        search = searchtext,
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection()
                        .QueryMultipleAsync(getByTeamMembersSearch, parameters, _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }
                else
                {
                    var parameters = new
                    {
                        offset,
                        limit
                    };
                    var result = await _unitOfWork.GetConnection()
                        .QueryMultipleAsync(getByTeamMembersSql, parameters, _unitOfWork.GetTransaction());

                    return new ListResult<Employee>()
                    {
                        List = result.Read<Employee>(),
                        TotalCount = result.ReadFirst<int>()
                    };
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
