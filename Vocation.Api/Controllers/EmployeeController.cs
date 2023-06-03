using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vocation.Core.Models;
using Vocation.Service.Services;

namespace Vocation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery] string id)
        {
            var result = await _employeeService.GetById(id);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetAll();
            return Ok(result);
        }

        [HttpGet("GetByTeamMembers")]
        public async Task<IActionResult> GetByTeamMembers([FromQuery] string searchText, [FromQuery] int offset, [FromQuery] int limit)
        {
            var result = await _employeeService.GetByTeamMembersAsync(searchText, offset, limit);
            return Ok(result);
        }

        [HttpGet("GetFullSearch")]
        public async Task<IActionResult> GetPagination([FromQuery] string searchText, [FromQuery] int offset, [FromQuery] int limit)
        {
            var result = await _employeeService.GetAllPaginationAsync(searchText, offset, limit);
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetByHeading([FromQuery] string searchText, [FromQuery] int offset, [FromQuery] int limit)
        {
            return Ok(await _employeeService.GetByHeading(searchText, offset, limit));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee Employee)
        {
            var result = await _employeeService.Add(Employee);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Employee Employee)
        {
            var result = await _employeeService.Update(Employee);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            var res = await _employeeService.Delete(id);
            if (!res)
            {
                return StatusCode(204);
            }

            return Ok(res);
        }

        [HttpGet("GetByUserId")]
        public async Task<IActionResult> GetByUserId([FromQuery] string userId)
        {
            var res = await _employeeService.GetByUserId(userId);
            return Ok(res);
        }
    }
}
