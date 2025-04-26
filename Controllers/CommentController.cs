using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

                if (queryObject.ContentId != null)
                    comments = comments.Where(c => c.PostId == queryObject.ContentId);

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
                                            .ThenInclude(a => a!.Avatar)
                                            .Select(c => c.ToCommentDto())
                                            .ToListAsync();
            return Ok(commentsDto);
        }

        [HttpGet("{commentId:int}")]
        public async Task<IActionResult> GetComment([FromRoute] int commentId)
        {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with ID {commentId} not found");
            return Ok(comment);
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

        [HttpPatch("{commentId:int}/updateLikes")]
        public async Task<IActionResult> UpdatePostLikes([FromRoute] int commentId, [FromBody] bool increment)
        {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return base.NotFound($"Post with {comment} does not exist");
            comment.Likes += increment ? 1 : -1;
            comment.Likes = comment.Likes < 0 ? 0 : comment.Likes;
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User not valid");
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikedComments)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("User not valid");
            if (increment)
                appUser.LikedComments.Add(comment);
            else
                appUser.LikedComments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpPatch("{commentId:int}/updateComment")]
        [Authorize]
        public async Task<IActionResult> UpdateComment([FromRoute] int commentId, [FromBody] CreateCommentDto commentDto)
        {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with ID ${commentId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid user request");
            // Ensure the user sending the request matches the user that made the comment
            if (comment.AppUserId != appUserId)
                return Unauthorized("Invalid user Id");
            if (!string.IsNullOrWhiteSpace(commentDto.Title))
                comment.Title = commentDto.Title;
            if (!string.IsNullOrWhiteSpace(commentDto.Content))
                comment.Content = commentDto.Content;
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound($"Comment with {commentId} not found");
            _context.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }
    }
}