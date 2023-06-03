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
    public class DepartmentController: ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.GetAll();
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department model)
        {
            var result = await _departmentService.Add(model);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Department model)
        {
            var result = await _departmentService.Update(model);
            return Ok(result);
        }
    }
}
