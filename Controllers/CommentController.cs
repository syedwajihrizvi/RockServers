using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Comments;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;

namespace RockServers.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public CommentController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject queryObject)
        {
            var comments = _context.Comments.AsQueryable();

            if (queryObject != null)
            {
                Console.WriteLine($"Query object was not null: {queryObject.PostId}");
                if (queryObject.PostId != null)
                    comments = comments.Where(c => c.PostId == queryObject.PostId);

                // Sort by Most Likes or Dislikes
                if (queryObject.SortByMostLikes == true)
                    comments = comments.OrderByDescending(c => c.Likes);
                else if (queryObject.SortByMostDislikes == true)
                    comments = comments.OrderByDescending(c => c.Dislikes);
                else
                    comments = comments.OrderByDescending(c => c.CommentedAt);

            }
            else
            {
                comments = comments.OrderByDescending(c => c.CommentedAt);
            }
            var commentsDto = await comments.Include(c => c.AppUser)
                                            .Select(c => c.ToCommentDto())
                                            .ToListAsync();
            return Ok(commentsDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto createCommentDto)
        {
            // Ensure the Post Id sent was valid
            var post = await _context.Posts.Where(p => p.Id == createCommentDto.PostId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with Id {createCommentDto.PostId} not found.");
            // Get the user Id of the person commenting
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User is invalid. Please ensure JWT token is in the request header.");
            var comment = createCommentDto.ToCommentFromCreate(appUserId);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok(createCommentDto);
        }
    }
}