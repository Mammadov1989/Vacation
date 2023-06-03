using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vocation.Api.Controllers.Models;
using Vocation.Core.Models.Identity;
using Vocation.Service.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Vocation.Service.Services;
using Vocation.Repository.Repositories;
using Mapster;
using Vocation.Core.Models;

namespace Vocation.Api.Controllers.Identity
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly JsonSerializerOptions _jsonSerializerSettings;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        //private readonly IPagePermissionService pagePermissionService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRepository _employeeRepository;

        public AccountController(IUserService userService, ITokenService tokenService, IConfiguration configuration,
            IMapper mapper
            //IPagePermissionService pagePermissionService,
            //IEmployeeService employeeService,
            //IEmployeeRepository employeeRepository
            )
        {
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
            _mapper = mapper;
            //this.pagePermissionService = pagePermissionService;
            //_employeeService = employeeService;
            //_employeeRepository = employeeRepository;
            _jsonSerializerSettings = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] RegisterViewModel model)
        {
            if (model == null)
            {
                return new StatusCodeResult(500);
            }

            var user = await _userService.FindByEmailAsync(model.Email);

            if (user != null)
            {
                return StatusCode(208);
            }

            user = await _userService.FindByPhoneNumberAsync(model.PhoneNumber);

            if (user != null)
            {
                return StatusCode(209);
            }

            var now = DateTime.Now;

            user = new ApplicationUser()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                Email = model.Email,
                CreatedDate = now,
                LastModifiedDate = now,
                EmailConfirmed = true,
                LockoutEnabled = false,
                DisplayName = model.DisplayName,
                PhoneNumber = model.PhoneNumber
            };

            Employee employee = model.Employee;
            employee.Name = model.DisplayName;
            var passwordErros = await _userService.ValidatePassword(model.Password);

            if (passwordErros != null)
            {
                return BadRequest(passwordErros);
            }

            try
            {
                var result = await _userService.CreateAsync(user, model.Password);
                var data = await _userService.GetAllUsers(0, 1);
                employee.UserId = data.List.First().Id;

                await _employeeService.Add(employee);
            }
            catch (Exception e)
            {
                return new UnprocessableEntityResult();
            }


            await _userService.AddToRoleAsync(user, "RegisteredUser");

            return Json(user.Adapt<RegisterViewModel>(), _jsonSerializerSettings);
        }

        //[Authorize]
        //[Microsoft.AspNetCore.Mvc.HttpGet("[action]")]
        //public async Task<IActionResult> GetById([FromQuery] string userId)
        //{
        //    try
        //    {
        //        var result = await _userService.FindByIdAsync(userId);
        //        return Json(result);
        //    }
        //    catch (Exception e)
        //    {
        //        return new UnprocessableEntityResult();
        //    }
        //}

        //[Authorize]
        //[HttpDelete("{userId}")]
        //public async Task<IActionResult> Delete(Guid userId)
        //{
        //    if (userId == null) return new StatusCodeResult(500);

        //    var now = DateTime.Now;

        //    ApplicationUser user = new ApplicationUser()
        //    {
        //        Id = userId
        //    };

        //    try
        //    {
        //        var result = await _userService.DeleteAsync(user);
        //        await _employeeService.Delete(userId.ToString());
        //        return Ok(result);
        //    }
        //    catch (Exception e)
        //    {
        //        return new UnprocessableEntityResult();
        //    }
        //}

        //[Authorize]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> Block([FromBody] BlockUserRequestModel requestModel)
        //{
        //    var user = await _userService.FindByIdAsync(requestModel.UserId);
        //    var result = await _userService.Block(user);

        //    return Ok(result);
        //}

        //[HttpPost("Auth")]
        //public async Task<IActionResult>
        //    Jwt([FromBody] TokenRequestViewModel model)
        //{
        //    // return a generic HTTP Status 500 (Server Error)
        //    // if the client payload is invalid.
        //    if (model == null) return new StatusCodeResult(500);
        //    return model.grant_type switch
        //    {
        //        "password" => await GetToken(model),
        //        "refresh_token" => await RefreshToken(model),
        //        "sing_out" => await SignOut(),
        //        _ => new UnauthorizedResult()
        //    };
        //}

        //[Authorize]
        //[HttpGet("[action]")]
        //public async Task<IActionResult> GetCurrentUser()
        //{
        //    var currentUser = this.User;

        //    var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    var result = await _userService.FindByIdAsync(currentUserId);

        //    var model = _mapper.Map<ApplicationUserViewModel>(result);

        //    return Json(model);
        //}

        //[Authorize]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> SignOut([FromBody] TokenRequestViewModel viewModel)
        //{
        //    await HttpContext.SignOutAsync();
        //    _tokenService.RemoveByRefreshToken(viewModel.refresh_token);
        //    return Ok();
        //}

        //[Authorize]
        //[HttpPut("[action]")]
        //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel requestModel)
        //{
        //    var currentUser = this.User;
        //    //var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    var currUserId = User.Claims.ToList().FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier).Value;

        //    var user = await _userService.FindByIdAsync(currUserId);


        //    var passwordErros = await _userService.ValidatePassword(requestModel.NewPassword);

        //    if (passwordErros != null)
        //    {
        //        return BadRequest(passwordErros);
        //    }

        //    try
        //    {
        //        if (requestModel.NewPassword == requestModel.ConfirmPassword)
        //        {
        //            var result =
        //                await _userService.ResetPassword(user, requestModel.OldPassword, requestModel.NewPassword);
        //            if (!result.Succeeded)
        //            {
        //                return StatusCode(204);
        //            }

        //            return Ok(result);
        //        }
        //        else return BadRequest();
        //    }
        //    catch (Exception e)
        //    {
        //        return new UnprocessableEntityResult();
        //    }
        //}

        //[Authorize]
        //[HttpPut("Update")]
        //public async Task<IActionResult> Update([FromBody] RegisterViewModel model)
        //{
        //    if (model == null)
        //    {
        //        return new StatusCodeResult(500);
        //    }

        //    var user = await _userService.FindByEmailAsync(model.Email);

        //    if (user != null && user.Id.ToString() != model.Id)
        //    {
        //        return StatusCode(208);
        //    }

        //    user = await _userService.FindByPhoneNumberAsync(model.PhoneNumber);

        //    if (user != null && user.Id.ToString() != model.Id)
        //    {
        //        return StatusCode(209);
        //    }

        //    var existUser = await _userService.FindByIdAsync(model.Id);

        //    var now = DateTime.Now;

        //    ApplicationUser userRequest = new ApplicationUser()
        //    {
        //        Id = Guid.Parse(model.Id),
        //        SecurityStamp = Guid.NewGuid().ToString(),
        //        UserName = model.UserName,
        //        Email = model.Email,
        //        CreatedDate = now,
        //        LastModifiedDate = now,
        //        EmailConfirmed = true,
        //        LockoutEnabled = false,
        //        DisplayName = model.DisplayName,
        //        PhoneNumber = model.PhoneNumber,
        //        PasswordHash = existUser.PasswordHash
        //    };

        //    model.Employee.Name = existUser.DisplayName;

        //    try
        //    {
        //        var result = await _userService.UpdateAsync(userRequest);
        //        await _employeeService.Update(model.Employee);
        //    }
        //    catch (Exception e)
        //    {
        //        return new UnprocessableEntityResult();
        //    }

        //    return Json(userRequest.Adapt<RegisterViewModel>(), _jsonSerializerSettings);
        //}


        //[Authorize]
        //[HttpGet("GetPagination")]
        //public async Task<IActionResult> GetAllAsync([FromQuery] int offset, [FromQuery] int limit)
        //{
        //    var result = await _userService.GetAllUsers(offset, limit);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("GetAll")]
        //public async Task<IActionResult> GetAllAsync()
        //{
        //    var result = await _userService.GetAllUsers();
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("GetBySearchPagination")]
        //public async Task<IActionResult> GetBySearchPagination([FromQuery] string searchText, [FromQuery] int offset,
        //    [FromQuery] int limit)
        //{
        //    var result = await _userService.GetAllUsers(searchText, offset, limit);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}


        //[Authorize]
        //[HttpGet]
        //[Route("GetActiveUsers")]
        //public async Task<IActionResult> GetActiveUsers()
        //{
        //    var result = await _userService.GetActiveUsers();
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        //[HttpGet]
        //[Route("ResetPassword")]
        //public async Task<IActionResult> ResetPassword([FromQuery] string email, [FromQuery] string newPassword)
        //{
        //    var result = await _userService.ResetPassword(email, newPassword);
        //    if (result == null)
        //    {
        //        return BadRequest();
        //    }
        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("GetActiveUsersPagination")]
        //public async Task<IActionResult> GetActiveUsersPagination([FromQuery] int offset, [FromQuery] int limit)
        //{
        //    var result = await _userService.GetActiveUsers(offset, limit);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("GetBlockedUsersPagination")]
        //public async Task<IActionResult> GetBlockedUsersPagination([FromQuery] int offset, [FromQuery] int limit)
        //{
        //    var result = await _userService.GetBlockedUsers(offset, limit);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("GetActiveUsersSearchPagination")]
        //public async Task<IActionResult> GetActiveUsersSearchPagination([FromQuery] string searchText, [FromQuery] int offset,
        //    [FromQuery] int limit)
        //{
        //    var result = await _userService.GetActiveUsers(searchText, offset, limit);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(result);
        //}

        ////[Authorize]
        ////[HttpGet]
        ////[Route("GetRoles")]
        ////public async Task<IActionResult> GetRolesByIdAsync([FromQuery] string Id)
        ////{
        ////    var result = await _userService.GetRolesByIdAsync(Id);
        ////    if (result == null)
        ////    {
        ////        return NotFound();
        ////    }

        ////    return Ok(result);
        ////}

        ////[Authorize]
        ////[HttpGet("[action]")]
        ////public IEnumerable<AppUserToken> GetAllTokens()
        ////{
        ////    var result = _tokenService.FindAll();
        ////    return result;
        ////}

        //private async Task<IActionResult> SignOut()
        //{
        //    await HttpContext.SignOutAsync();
        //    return Ok();
        //}

        //private async Task<IActionResult>
        //    GetToken(TokenRequestViewModel model)
        //{
        //    try
        //    {
        //        // check if there's an user with the given username
        //        var user = await
        //            _userService.FindByNameAsync(model.username);
        //        // fallback to support e-mail address instead of username
        //        if (user == null && model.username.Contains("@"))
        //            user = await
        //                _userService.FindByEmailAsync(model.username);
        //        if (user == null
        //            || !await _userService.CheckPasswordAsync(user,
        //                model.password))
        //        {
        //            // user does not exists or password mismatch
        //            return new UnauthorizedResult();
        //        }

        //        var employee = await _employeeRepository.GetByUserIdAsync(user.Id.ToString());

        //        if (user.Blocked || !employee.Active)
        //        {
        //            return new UnauthorizedResult();
        //        }

        //        // username & password matches: create the refresh token
        //        var refreshToken = CreateRefreshToken(model.provider_id, user.Id.ToString(), model.username);

        //        // delete user token if it is exist in DB
        //        _tokenService.Remove(new ApplicationUserToken
        //        {
        //            LoginProvider = model.provider_id,
        //            UserId = user.Id.ToString(),
        //            Name = model.username
        //        });

        //        // add the new refresh token to the DB
        //        _tokenService.Add(refreshToken);

        //        // create & return the access token
        //        var token = await CreateAccessToken(user, refreshToken.Value);
        //        return Json(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new UnauthorizedResult();
        //    }
        //}

        //private async Task<IActionResult> RefreshToken(TokenRequestViewModel model)
        //{
        //    try
        //    {
        //        var rt = _tokenService.FindByKeys(model.provider_id, model.refresh_token);

        //        if (rt == null)
        //        {
        //            return new UnauthorizedResult();
        //        }

        //        var user = await _userService.FindByIdAsync(rt.UserId);

        //        if (user == null)
        //        {
        //            return new UnauthorizedResult();
        //        }

        //        var rtNew = CreateRefreshToken(rt.LoginProvider, rt.UserId, user.UserName);

        //        _tokenService.Remove(rt);
        //        _tokenService.Add(rtNew);

        //        var response = await CreateAccessToken(user, rtNew.Value);

        //        return Json(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new UnauthorizedResult();
        //    }
        //}

        //private ApplicationUserToken CreateRefreshToken(string clientId, string userId, string name)
        //{
        //    return new ApplicationUserToken()
        //    {
        //        LoginProvider = clientId,
        //        UserId = userId,
        //        Name = name,
        //        Type = 0,
        //        Value = Guid.NewGuid().ToString("N"),
        //        AddedDate = DateTime.UtcNow
        //    };
        //}

        //private async Task<TokenResponseViewModel> CreateAccessToken(ApplicationUser user, string
        //    refreshToken)
        //{
        //    var now = DateTime.UtcNow;

        //    // add the registered claims for JWT (RFC7519).
        //    // For more info, see https://tools.ietf.org/html/rfc7519#section-4.1
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
        //        new Claim(JwtRegisteredClaimNames.GivenName, user.DisplayName.ToString()),
        //        // TODO: add additional claims here
        //    };

        //    var roles = _userService.GetRolesAsync(user).Result;

        //    if (roles != null && roles.Count > 0)
        //    {
        //        claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        //    }

        //    var disabledPages = await pagePermissionService.GetAllDisabledPages();

        //    if (disabledPages != null && disabledPages.Count > 0)
        //    {
        //        claims.AddRange(disabledPages.Select(i => new Claim("DisabledPages", i.PageKey)));
        //    }

        //    var tokenExpirationMins = _configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
        //    var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Auth:Jwt:Key"]));

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Auth:Jwt:Issuer"],
        //        audience: _configuration["Auth:Jwt:Audience"],
        //        claims: claims.ToArray(),
        //        notBefore: now,
        //        expires: now.Add(TimeSpan.FromMinutes(tokenExpirationMins)),
        //        signingCredentials: new SigningCredentials(
        //            issuerSigningKey, SecurityAlgorithms.HmacSha256)
        //    );

        //    var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

        //    return new TokenResponseViewModel()
        //    {
        //        token = encodedToken,
        //        expiration = tokenExpirationMins,
        //        refresh_token = refreshToken
        //    };
        //}
    }
}
