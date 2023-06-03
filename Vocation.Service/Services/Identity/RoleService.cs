using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Vocation.Core.Models.Helpers;
using Vocation.Core.Models.Identity;
using Vocation.Repository.Infrastucture.Identity;

namespace Vocation.Service.Services.Identity
{
    public interface IRoleService
    {
        IQueryable<ApplicationRole> GetAll();
        ListResult<ApplicationRole> GetAll(int offset, int limit);
        ListResult<ApplicationRole> GetAll(string searchText, int offset, int limit);
        Task<ApplicationRole> GetByIdAsync(int id);
        Task<ApplicationRole> CreateAsync(ApplicationRole role);
        Task<ApplicationRole> UpdateAsync(ApplicationRole role);
        Task DeleteAsync(int roleId);
        Task<ApplicationRole> FindByIdAsync(string id);
        Task<ApplicationRole> FindByNameAsync(string name);
        Task<ApplicationRole> FindUniqueByNameAsync(string normalizedUserName, string roleId);
        List<ApplicationRoleGroupby> GetAllForUI();
    }

    public class RoleService : IRoleService
    {
        private readonly VacRoleManager _roleManager;

        public RoleService(VacRoleManager roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ApplicationRole> CreateAsync(ApplicationRole role)
        {
            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                return null;
            }


            await _roleManager.CreateAsync(role);
            var result = await _roleManager.FindByNameAsync(role.Name);
            return result;
        }

        public async Task DeleteAsync(int roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ReasonPhrase = "Role is in use"
                });
            }
        }

        public async Task<ApplicationRole> UpdateAsync(ApplicationRole role)
        {
            await _roleManager.UpdateAsync(role);
            var result = await _roleManager.FindByNameAsync(role.Name);
            return result;
        }

        public async Task<ApplicationRole> FindByIdAsync(string id)
        {
            var result = await _roleManager.FindByIdAsync(id);
            return result;
        }

        public async Task<ApplicationRole> FindByNameAsync(string name)
        {
            var result = await _roleManager.FindByNameAsync(name);
            return result;
        }

        public IQueryable<ApplicationRole> GetAll()
        {
            var result = _roleManager.Roles;
            return result;
        }
        public ListResult<ApplicationRole> GetAll(int offset, int limit)
        {
            var roles = _roleManager.Roles;
            var dataPagination = roles.OrderByDescending(j => j.Id).Skip(offset).Take(limit).ToList();
            ListResult<ApplicationRole> result = new ListResult<ApplicationRole>()
            {
                List = dataPagination,
                TotalCount = roles.Count()
            };
            return result;
        }

        public ListResult<ApplicationRole> GetAll(string searchText, int offset, int limit)
        {
            var roles = _roleManager.Roles.Where(row => ((row.Name != null && row.Name.ToUpper().Contains(searchText.ToUpper())) || (row.GroupName != null && row.GroupName.ToUpper().Contains(searchText.ToUpper()))));

            var dataPagination = roles.OrderBy(j => j.GroupId).Skip(offset).Take(limit).ToList();

            ListResult<ApplicationRole> result = new ListResult<ApplicationRole>()
            {
                List = dataPagination,
                TotalCount = roles.Count()
            };
            return result;
        }

        public List<ApplicationRoleGroupby> GetAllForUI()
        {
            var result = _roleManager.Roles;
            var items = result.OrderBy(s => s.GroupId);
            var listResult = new List<ApplicationRole>(items);

            var MyDataIQ = from i in result orderby i.GroupId group i by i.GroupId into grp select new { GroupId = grp.Key, GroupName = grp.FirstOrDefault().GroupName, cnt = grp.Count() };
            var groupList = MyDataIQ.ToList();

            var Roles = new List<ApplicationRoleGroupby>();
            var roleUi = new List<ApplicationRoleUI>();

            int index = 0;
            for (int i = 0; i < groupList.Count; i++)
            {
                roleUi = new List<ApplicationRoleUI>();

                for (int j = 0; j < groupList[i].cnt; j++)
                {
                    roleUi.Add(new ApplicationRoleUI
                    {
                        Id = listResult[index].Id,
                        Name = listResult[index].Name,
                        NormalizedName = listResult[index].NormalizedName
                    });
                    index += 1;
                }
                Roles.Add(new ApplicationRoleGroupby
                {
                    GroupId = groupList[i].GroupId,
                    GroupName = groupList[i].GroupName,
                    Roles = roleUi
                }); ;

            }
            return Roles;
        }

        public async Task<ApplicationRole> GetByIdAsync(int id)
        {
            var result = await _roleManager.FindByIdAsync(id.ToString());
            return result;
        }

        public async Task<ApplicationRole> FindUniqueByNameAsync(string normalizedUserName, string roleId)
        {
            var result = await _roleManager.FindUniqueByNameAsync(normalizedUserName, roleId);
            return result;
        }
    }
}
