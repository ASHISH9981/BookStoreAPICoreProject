using BookStore_API.Contracts;
using BookStore_API.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration   _configuration;


        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILoggerService loggerService,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loggerService;
            _configuration = configuration;
        }

        /// <summary>
        /// User Login EndPoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var getLocation = GetControllerActionNames();
            try
            {
                _logger.logInfo($"{getLocation} Login Started");
                var Username = userDTO.Username;
                var Password = userDTO.Password;
                var result = await _signInManager.PasswordSignInAsync(Username, Password, false, false);
                if (result.Succeeded)
                {
                    _logger.logInfo($" Login Successful");
                    var user = await _userManager.FindByNameAsync(Username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { tokenString = tokenString });
                }
                _logger.logInfo($"{getLocation} Login UnAuthorized");
                return Unauthorized(userDTO);
            }
            catch(Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }

        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultNameClaimType, r)));
            var token = new JwtSecurityToken(_configuration["Jwt:Issueer"],
                _configuration["Jwt:Issueer"],
                claims,
                null,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ObjectResult Internalserver(string message)
        {
            _logger.logError(message);
            return StatusCode(500, "Somthing went wrong contact your adminstrator");
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";

        }
    }

}
