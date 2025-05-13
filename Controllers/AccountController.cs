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
using RockServers.Data;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
namespace RockServers.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager,
            ApplicationDBContext context,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User ID Provided");
            // var appUser = await _userManager.FindByIdAsync(appUserId);
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikedPosts)
                                              .Include(u => u.Avatar)
                                              .Include(u => u.LikedDicussions)
                                              .Include(u => u.LikesPostComments)
                                              .Include(u => u.LikedDiscussionComments)
                                              .Include(u => u.LikedPostReplys)
                                              .Include(u => u.LikedDiscussionReplys)
                                              .Include(u => u.Following)
                                              .ThenInclude(a => a.Avatar)
                                              .Include(u => u.Followers)
                                              .ThenInclude(a => a.Avatar)
                                              .FirstOrDefaultAsync();
            // Find posts and discussions made by the user
            var posts = await _context.Posts.Where(p => p.AppUserId == appUserId).ToListAsync();
            var discussions = await _context.Discussions.Where(d => d.AppUserId == appUserId).ToListAsync();
            if (appUser == null)
                return Unauthorized("Invalid User ID Provided");
            var appUserDto = appUser.ToUserInformationDto();
            appUserDto.TotalPostings = posts.Count + discussions.Count;
            return Ok(appUserDto);
        }

        [HttpGet("profile/{appUsername}")]
        public async Task<IActionResult> GetProfileInfo([FromRoute] string appUsername)
        {
            var appUser = await _context.Users.Where(u => u.UserName == appUsername)
                                              .Include(u => u.Avatar)
                                              .Include(u => u.Following)
                                              .ThenInclude(a => a.Avatar)
                                              .Include(u => u.Followers)
                                              .ThenInclude(a => a.Avatar)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return NotFound($"User with {appUsername} does not exist");
            // Find posts and discussions made by the users
            var posts = await _context.Posts.Where(p => p.AppUserId == appUser.Id).ToListAsync();
            var discussions = await _context.Discussions.Where(d => d.AppUserId == appUser.Id).ToListAsync();
            if (appUser == null)
                return Unauthorized("Invalid User ID Provided");
            var appUserDto = appUser.ToUserInformationDto();
            appUserDto.TotalPostings = posts.Count + discussions.Count;
            return Ok(appUserDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            // Create the user
            var newUser = new AppUser
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Username
            };

            // Process the image
            if (registerDto.Avatar != null)
            {
                var avatar = await _context.Avatars.Where(a => a.Name == registerDto.Avatar).FirstOrDefaultAsync();
                if (avatar == null)
                    return NotFound("Invalid avatar Id sent");
                newUser.Avatar = avatar;
            }
            if (registerDto.ImageFile != null)
            {
                var imageFile = registerDto.ImageFile;
                if (imageFile == null || imageFile.Length == 0)
                    return BadRequest("No profile image or avatar provided");
                var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var outputPath = Path.Combine("wwwroot/uploads/profile_images", $"{fileName}.webp");
                using var image = await Image.LoadAsync(imageFile.OpenReadStream());
                await image.SaveAsync(outputPath, new WebpEncoder());
                newUser.ProfileImage = fileName;
            }

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
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.Succeeded)
            {
                return Ok(new BaseUserDto
                {
                    Email = user.Email!,
                    Username = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = _tokenService.CreateToken(user)
                });
            }
            return Unauthorized("Invalid Login Details Provided");
        }

        [HttpPatch("follow")]
        public async Task<IActionResult> Follow([FromForm] FollowDto followDto)
        {
            var user = await _context.Users.Where(u => u.UserName == followDto.Username)
                                           .Include(u => u.Followers).FirstOrDefaultAsync();
            if (user == null)
                return NotFound("The user requested to follow was not found");
            var currentUserId = User.GetUserId();
            if (currentUserId == null)
                return Unauthorized("Unauthorized request");
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Unauthorized("User not found");
            if (user.Followers.Contains(currentUser))
                user.Followers.Remove(currentUser);
            else
            {
                user.Followers.Add(currentUser);
                var notification = new Notification
                {
                    Type = NotificationType.Follow,
                    EngagerId = currentUserId,
                    TargetId = user.Id,
                };
                await Notification.SaveNotification(notification, _context);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("update/{field}")]
        public async Task<IActionResult> UpdateField([FromRoute] string field, [FromForm] UpdateUserDto updateUser)
        {
            var appUserId = User.GetUserId();
            var appUser = await _context.Users.Where(u => u.Id == appUserId).FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("Invalid user provided");
            var passwordResult = _signInManager.CheckPasswordSignInAsync(appUser, updateUser.ConfirmPassword, false);
            if (!passwordResult.IsCompletedSuccessfully)
                return Unauthorized("Invalid user credentials");
            if (field == "email")
            {
                appUser.Email = updateUser.Email;
                await _context.SaveChangesAsync();
                return Ok(appUser.ToUserInformationDto());
            }
            else if (field == "username")
            {
                appUser.UserName = updateUser.Username;
                await _context.SaveChangesAsync();
                return Ok(appUser.ToUserInformationDto());
            }
            else if (field == "password")
            {
                var result = await _userManager.ChangePasswordAsync(appUser, updateUser.Password, updateUser.Password);
                if (result.Succeeded)
                    return Ok(appUser.ToUserInformationDto());
            }
            return BadRequest("Field type not supported.");
        }
    }
}