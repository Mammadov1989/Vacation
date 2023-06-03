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
    public class VacationRequestController: ControllerBase
    {
        private readonly IVacationRequestService _vacationService;

        public VacationRequestController(IVacationRequestService vacationService)
        {
            _vacationService = vacationService;
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vacationService.GetAll();
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VacationRequest model)
        {
            var result = await _vacationService.Add(model);
            return Ok(result);
        }
    }
}
