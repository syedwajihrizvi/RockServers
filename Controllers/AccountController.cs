using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RockServers.Models;
using RockServers.DTO.Accounts;
using RockServers.Mappers;
using RockServers.Services;
using RockServers.Interfaces;
namespace RockServers.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Create the user
            var newUser = new AppUser
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Psn = registerDto.Psn,
                UserName = registerDto.Username
            };

            if (registerDto.Password != null)
            {
                // Execute creation of user
                var createdUser = await _userManager.CreateAsync(newUser, registerDto.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                    if (roleResult.Succeeded)
                    {
                        var token = _tokenService.CreateToken(newUser);
                        return Ok(newUser.ToCreatedUserDto(token));
                    }
                    return StatusCode(500, "Assigning role to user failed");
                }
                else
                {
                    return StatusCode(500, createdUser.Errors.First().Description);
                }
            }
            else
            {
                return BadRequest("Error. Registeration missing password");
            }
        }
    }
}