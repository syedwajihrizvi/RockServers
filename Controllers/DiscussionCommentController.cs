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
    [Route("api/discussionComments")]
    [ApiController]
    public class DiscussionCommentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DiscussionCommentController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject queryObject)
        {
            var comments = _context.DiscussionComments.AsQueryable();

            if (queryObject != null)
            {

                if (queryObject.ContentId != null)
                    comments = comments.Where(c => c.DiscussionId == queryObject.ContentId);

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
                                            .Select(c => c.ToDiscussionCommentDto())
                                            .ToListAsync();
            return Ok(commentsDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiscussionCommentDto createDiscussionCommentDto)
        {
            // Ensure the Discussion Id sent was valid
            var discussion = await _context.Discussions.Where(d => d.Id == createDiscussionCommentDto.DiscussionId).FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion with Id {createDiscussionCommentDto.DiscussionId} not found.");
            // Get the user Id of the person commenting
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User is invalid. Please ensure JWT token is in the request header.");
            var comment = createDiscussionCommentDto.ToDiscussionCommentFromCreate(appUserId);
            await _context.DiscussionComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok(createDiscussionCommentDto);

        }

        [HttpPatch("{commentId:int}/updateLikes")]
        public async Task<IActionResult> UpdatePostLikes([FromRoute] int commentId, [FromBody] bool increment)
        {
            var comment = await _context.DiscussionComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return base.NotFound($"Post with {comment} does not exist");
            comment.Likes += increment ? 1 : -1;
            comment.Likes = comment.Likes < 0 ? 0 : comment.Likes;
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User not valid");
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikedDiscussionComments)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("User not valid");
            if (increment)
                appUser.LikedDiscussionComments.Add(comment);
            else
                appUser.LikedDiscussionComments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }
    }
}