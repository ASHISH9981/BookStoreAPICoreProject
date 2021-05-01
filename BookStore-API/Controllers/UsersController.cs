using BookStore_API.Contracts;
using BookStore_API.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILoggerService loggerService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loggerService;
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
                    return Ok(user);
                }
                _logger.logInfo($"{getLocation} Login UnAuthorized");
                return Unauthorized(userDTO);
            }
            catch(Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }

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
