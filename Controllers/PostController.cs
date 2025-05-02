using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using RockServers.Data;
using RockServers.DTO.Posts;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;
using RockServers.Models;

namespace RockServers.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public PostController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PostQueryObject queryObject)
        {
            var posts = _context.Posts.AsQueryable();
            if (queryObject != null)
            {
                if (!string.IsNullOrWhiteSpace(queryObject.SearchValue))
                {
                    posts = posts.Include(p => p.Game).Where(p => (
                        p.Title.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", "")) ||
                        p.Description.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", "")) ||
                        p.Game!.Title.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", ""))
                        ));
                }
                if (!string.IsNullOrWhiteSpace(queryObject.Title))
                    posts = posts.Where(p => p.Title.ToLower().Contains(queryObject.Title.ToLower()));
                if (!string.IsNullOrWhiteSpace(queryObject.Description))
                    posts = posts.Where(p => p.Description.ToLower().Contains(queryObject.Description.ToLower()));
                if (queryObject.GameId != null)
                    posts = posts.Where(p => p.GameId == queryObject.GameId);
                if (!string.IsNullOrWhiteSpace(queryObject.UserId))
                    posts = posts.Where(p => p.AppUserId == queryObject.UserId);
                if (queryObject.PlatformId != null)
                    posts = posts.Where(p => p.PlatformId == queryObject.PlatformId);

                // Check for latest
                if (queryObject.MostRecent)
                    posts = posts.OrderByDescending(p => p.PostedAt);

                if (queryObject.PostToRemoveId != null)
                    posts = posts.Where(p => p.Id != queryObject.PostToRemoveId);

                // Check if we want posts based on sessions
                if (queryObject.SessionType == "active")
                    posts = posts.Where(p => p.Sessions.Any(s => s.EndTime == null));

                if (queryObject.SessionType == "joinable")
                    posts = posts.Where(p => !p.Sessions.Any(s => s.EndTime == null));

                if (!string.IsNullOrWhiteSpace(queryObject.UserId))
                    posts = posts.Where(p => p.AppUserId == queryObject.UserId);

                // If there is a limit
                if (queryObject.Limit != null)
                    posts = posts.Take((int)queryObject.Limit);

                if (!string.IsNullOrWhiteSpace(queryObject.OrderBy))
                {
                    if (queryObject.OrderBy == "likes")
                        posts = posts.OrderByDescending(p => p.Likes);
                    else if (queryObject.OrderBy == "comments")
                        posts = posts.OrderByDescending(p => p.Comments.Count);
                    else if (queryObject.OrderBy == "views")
                        posts = posts.OrderByDescending(p => p.Views);
                }
            }

            var postsDtos = await posts.Include(p => p.Game)
                                 .Include(p => p.AppUser)
                                 .ThenInclude(a => a!.Avatar)
                                 .Include(p => p.Platform)
                                 .Include(p => p.Comments)
                                 .ThenInclude(c => c.AppUser)
                                 .ThenInclude(a => a!.Avatar)
                                 .Include(p => p.Sessions)
                                 .ThenInclude(s => s.Users)
                                 .ThenInclude(s => s.AppUser)
                                 .Select(p => p.ToPostDto()).ToListAsync();
            // Check the type of posts we are fetching
            return Ok(postsDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var post = await _context.Posts.Where(p => p.Id == id)
                                     .Include(p => p.Game)
                                     .Include(p => p.AppUser)
                                     .ThenInclude(a => a!.Avatar)
                                     .Include(p => p.Platform)
                                     .Include(p => p.Comments)
                                     .ThenInclude(c => c.AppUser)
                                     .ThenInclude(a => a!.Avatar)
                                     .Include(p => p.Sessions)
                                     .ThenInclude(s => s.Users)
                                     .ThenInclude(s => s.AppUser).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {id} not found");
            post.Comments = post.Comments.OrderByDescending(c => c.CommentedAt).ToList();
            return Ok(post.ToPostDto());
        }

        [HttpPost("customImage")]
        [Authorize]
        public async Task<IActionResult> CreateWithCustomImage([FromForm] CreatePostDtoWithCustomImage createPostDto)
        {
            // Ensure that gameId is valid
            var gameId = createPostDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with {gameId} does not exist.");
            var platformId = createPostDto.PlatformId;
            var platform = await _context.Platforms.Where(p => p.Id == platformId).FirstOrDefaultAsync();
            if (platform == null)
                return NotFound($"Platform with id {platformId} not found");
            // Get the user Id from the JWT Token
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User ID Provided");
            var imageFile = createPostDto.ImageFile;
            // Create custom image
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No file found in request");

            // Currently, store the image in localhost wwwroot
            // TODO: Move image storage to cloud instead of localhost
            var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            var outputpath = Path.Combine("wwwroot/uploads/post_images", $"{fileName}.webp");
            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            await image.SaveAsync(outputpath, new WebpEncoder());
            var publicUrl = $"/uploads/post_images/{fileName}.webp";
            var newPost = new Post
            {
                GameId = gameId,
                PlatformId = platformId,
                AppUserId = appUserId,
                Title = createPostDto.Title,
                Description = createPostDto.Description,
                ImagePath = fileName
            };

            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newPost.Id }, newPost);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto createPostDto)
        {
            // Ensure that gameId is valid
            var gameId = createPostDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with {gameId} does not exist.");
            var platformId = createPostDto.PlatformId;
            var platform = await _context.Platforms.Where(p => p.Id == platformId).FirstOrDefaultAsync();
            if (platform == null)
                return NotFound($"Platform with id {platformId} not found");
            // Get the user Id from the JWT Token
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User ID Provided");

            var newPost = new Post
            {
                GameId = gameId,
                PlatformId = platformId,
                AppUserId = appUserId,
                Title = createPostDto.Title,
                Description = createPostDto.Description,
                ImagePath = createPostDto.ImagePath
            };

            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newPost.Id }, newPost);
        }


        [HttpPatch("{postId:int}/updateLikes")]
        public async Task<IActionResult> UpdatePostLikes([FromRoute] int postId, [FromBody] bool increment)
        {
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {postId} does not exist");
            post.Likes += increment ? 1 : -1;
            post.Likes = post.Dislikes < 0 ? 0 : post.Likes;
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User not valid");
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikedPosts)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("User not valid");
            if (increment)
            {
                appUser.LikedPosts.Add(post);
                var notification = new Notification
                {
                    Type = NotificationType.PostLike,
                    EngagerId = appUserId,
                    TargetId = post.AppUserId!,
                    EntityId = post.Id,
                };
                await _context.Notifications.AddAsync(notification);
            }
            else
                appUser.LikedPosts.Remove(post);

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [HttpDelete("{postId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound($"Post with {postId} not found");
            _context.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    };
}