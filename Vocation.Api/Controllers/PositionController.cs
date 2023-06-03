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
    public class PositionController: ControllerBase
    {
        private readonly IPositionService _employeePositionService;

        public PositionController(IPositionService employeePositionService)
        {
            _employeePositionService = employeePositionService;
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetFullSearch([FromQuery] string id)
        {
            var result = await _employeePositionService.GetById(id);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeePositionService.GetAll();
            return Ok(result);
        }

        [HttpGet("GetPagination")]
        public async Task<IActionResult> GetPagination([FromQuery] string searchText, [FromQuery] int offset, int limit)
        {
            var result = await _employeePositionService.GetPaginationAsync(searchText, offset, limit);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Position employeePosition)
        {
            var response = await _employeePositionService.Add(employeePosition);
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Position employeePosition)
        {
            await _employeePositionService.Update(employeePosition);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {

            var response = await _employeePositionService.Delete(id);
            if (response)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(204);
            }
        }
    }
}
