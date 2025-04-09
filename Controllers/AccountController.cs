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
using RockServers.Extensions;
namespace RockServers.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User ID Provided");
            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null)
                return Unauthorized("Invalid User ID Provided");
            return Ok(appUser.ToUserInformationDto());
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername!);
            if (user == null)
                user = await _userManager.FindByNameAsync(loginDto.EmailOrUsername!);
            if (user == null)
                return Unauthorized("Invalid Login Details Provided");
            var result = _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.IsCompletedSuccessfully)
                return Ok(new BaseUserDto
                {
                    Email = user.Email!,
                    Username = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = _tokenService.CreateToken(user)
                });
            return Unauthorized("Invalid Login Details Provided");
        }
    }
}