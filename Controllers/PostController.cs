using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                if (!string.IsNullOrWhiteSpace(queryObject.Title))
                    posts = posts.Where(p => p.Title.ToLower().Contains(queryObject.Title.ToLower()));
                if (!string.IsNullOrWhiteSpace(queryObject.Description))
                    posts = posts.Where(p => p.Description.ToLower().Contains(queryObject.Description.ToLower()));
                if (queryObject.GameId != null)
                    posts = posts.Where(p => p.GameId == queryObject.GameId);
                if (!string.IsNullOrWhiteSpace(queryObject.AppUserId))
                    posts = posts.Where(p => p.AppUserId == queryObject.AppUserId);
                if (queryObject.PlatformId != null)
                    posts = posts.Where(p => p.PlatformId == queryObject.PlatformId);
                // Check for Views
                if (queryObject.Views_eq != null)
                    posts = posts.Where(p => p.Views == queryObject.Views_eq);
                else if (queryObject.Views_lte != null)
                    posts = posts.Where(p => p.Views <= queryObject.Views_lte);
                else if (queryObject.Views_gte != null)
                    posts = posts.Where(p => p.Views >= queryObject.Views_gte);

                // Check for Likes
                if (queryObject.Likes_eq != null)
                    posts = posts.Where(p => p.Views == queryObject.Likes_eq);
                else if (queryObject.Views_lte != null)
                    posts = posts.Where(p => p.Views <= queryObject.Likes_lte);
                else if (queryObject.Views_gte != null)
                    posts = posts.Where(p => p.Views >= queryObject.Likes_gte);

                // Check for Dislikes
                if (queryObject.Dislikes_eq != null)
                    posts = posts.Where(p => p.Views == queryObject.Dislikes_eq);
                else if (queryObject.Views_lte != null)
                    posts = posts.Where(p => p.Views <= queryObject.Dislikes_lte);
                else if (queryObject.Views_gte != null)
                    posts = posts.Where(p => p.Views >= queryObject.Dislikes_gte);

                // Check for latest
                if (queryObject.Latest)
                    posts = posts.OrderByDescending(p => p.PostedAt);
            }
            var postsDtos = await posts.Include(p => p.Game)
                                 .Include(p => p.AppUser)
                                 .Include(p => p.Platform)
                                 .Include(p => p.Comments)
                                 .ThenInclude(c => c.AppUser)
                                 .Include(p => p.Sessions)
                                 .ThenInclude(s => s.Users)
                                 .ThenInclude(s => s.AppUser)
                                 .Select(p => p.ToPostDto()).ToListAsync();
            return Ok(postsDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var post = await _context.Posts.Where(p => p.Id == id)
                                     .Include(p => p.Game)
                                     .Include(p => p.AppUser)
                                     .Include(p => p.Platform)
                                     .Include(p => p.Comments)
                                     .ThenInclude(c => c.AppUser)
                                     .Include(p => p.Sessions)
                                     .ThenInclude(s => s.Users)
                                     .ThenInclude(s => s.AppUser).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {id} not found");
            return Ok(post.ToPostDto());
        }

        [HttpPost]
        [Authorize]
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
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [HttpPatch("{postId:int}/updateDislikes")]
        public async Task<IActionResult> UpdatePostDislikes([FromRoute] int postId, [FromBody] bool increment)
        {
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {postId} does not exist");
            post.Dislikes += increment ? 1 : -1;
            post.Dislikes = post.Dislikes < 0 ? 0 : post.Dislikes;
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