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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;

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
                    var searchValue = queryObject.SearchValue.ToLower().Trim().Replace(" ", "");
                    posts = posts.Include(p => p.Game).Where(p => (
                        p.Title.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        p.Description.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        p.Game!.Title.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        p.Tags!.ToLower().Trim().Contains(searchValue)
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
                if (queryObject.PlatformId != null && queryObject.PlatformId != 4)
                    posts = posts.Where(p => p.PlatformId == queryObject.PlatformId);

                // Check for latest
                if (queryObject.MostRecent)
                    posts = posts.OrderByDescending(p => p.PostedAt);

                if (queryObject.PostToRemoveId != null)
                    posts = posts.Where(p => p.Id != queryObject.PostToRemoveId);

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
                                     .FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {id} not found");
            post.Comments = post.Comments.OrderByDescending(c => c.CommentedAt).ToList();
            return Ok(post.ToPostDto());
        }

        [RequestSizeLimit(52428800)] // 50 MB
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePostDto createPostDto)
        {
            var gameId = createPostDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with ID {gameId} does not exist.");
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
            };

            if (createPostDto.ThumbnailFile != null)
            {
                var thumbnailFile = createPostDto.ThumbnailFile;
                if (thumbnailFile == null || thumbnailFile.Length == 0)
                    return BadRequest("No file found in request");
                // Get the file type
                var fileType = thumbnailFile.ContentType;
                if (fileType.ToLower().Contains("video"))
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
                    var outputpath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(outputpath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    newPost.ThumbnailType = ThumbnailType.Video;
                    newPost.ThumbnailPath = generatedUniqueFileName;

                }
                else if (fileType.ToLower().Contains("image"))
                {
                    // Extract the main image into a path first
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var outputpath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var image = await Image.LoadAsync(thumbnailFile.OpenReadStream());
                    await image.SaveAsync(outputpath, new WebpEncoder());
                    var publicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    newPost.ThumbnailPath = generatedUniqueFileName;
                }
                else
                {
                    return BadRequest("Invalid file type detected");
                }
            }
            else
            {
                newPost.ThumbnailPath = createPostDto.ThumbnailPath;

            }

            // Generate Tags
            newPost.Tags = string.Join(" ", TagHelper.GenerateTags(newPost.GameId, newPost.PlatformId));
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newPost.Id }, newPost);
        }


        [RequestSizeLimit(52428800)] // 50 MB
        [HttpPatch("{postId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost([FromForm] UpdatePostDto updatePostDto, [FromRoute] int postId)
        {
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound("Post not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid credentials");
            if (appUserId != post.AppUserId)
                return Unauthorized("Invalid credentials");

            // See what field need to be updated
            if (updatePostDto.Title != null)
                post.Title = updatePostDto.Title;
            if (updatePostDto.Description != null)
                post.Description = updatePostDto.Description;
            if (updatePostDto.GameId != null)
                post.GameId = updatePostDto.GameId;
            if (updatePostDto.PlatformId != null)
                post.PlatformId = updatePostDto.PlatformId;

            if (updatePostDto.ThumbnailFile != null)
            {
                var thumbnailFile = updatePostDto.ThumbnailFile;
                if (thumbnailFile == null || thumbnailFile.Length == 0)
                    return BadRequest("No file found in request");
                // Get the file type
                var fileType = thumbnailFile.ContentType;
                if (fileType.ToLower().Contains("video"))
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
                    var outputpath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(outputpath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    post.ThumbnailType = ThumbnailType.Video;
                    post.ThumbnailPath = generatedUniqueFileName;

                }
                else if (fileType.ToLower().Contains("image"))
                {
                    // Extract the main image into a path first
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var outputpath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var image = await Image.LoadAsync(thumbnailFile.OpenReadStream());
                    await image.SaveAsync(outputpath, new WebpEncoder());
                    var publicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    post.ThumbnailPath = generatedUniqueFileName;
                    post.ThumbnailType = ThumbnailType.Image;
                }
                else
                {
                    return BadRequest("Invalid file type detected");
                }
            }
            if (updatePostDto.ThumbnailPath != null)
            {
                post.ThumbnailPath = updatePostDto.ThumbnailPath;
                post.ThumbnailType = ThumbnailType.Image;
            }

            // Save the changes
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [HttpPatch("{postId:int}/updateLikes")]
        public async Task<IActionResult> UpdatePostLikes([FromRoute] int postId, [FromBody] bool increment)
        {
            var post = await _context.Posts
                                    .Where(p => p.Id == postId)
                                    .FirstOrDefaultAsync();
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
                await Notification.SaveNotification(notification, _context);
            }
            else
                appUser.LikedPosts.Remove(post);

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [HttpPatch("{postId:int}/updateViews")]
        public async Task<IActionResult> UpdatePostViews([FromRoute] int postId)
        {
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Discussion with {postId} not found");
            post.Views += 1;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{postId:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int postId)
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User Credentials");
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound($"Post with {postId} not found");
            if (post.AppUserId != appUserId)
                return Unauthorized("Invalid User Credentials Provided");
            _context.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    };
}