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
    public class VacationDayController : ControllerBase
    {
        private readonly IVacationDayService _vacationDayService;

        public VacationDayController(IVacationDayService vacationDayService)
        {
            _vacationDayService = vacationDayService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vacationDayService.GetAll();
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VacationDay model)
        {
            var result = await _vacationDayService.Add(model);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] VacationDay model)
        {
            var result = await _vacationDayService.Update(model);
            return Ok(result);
        }
    }
}
