using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Comments;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;
using RockServers.Models;

namespace RockServers.Controllers
{
    [Route("api/discussionComments")]
    [ApiController]
    public class DiscussionCommentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DiscussionCommentController(ApplicationDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                                            .ThenInclude(a => a!.Avatar)
                                            .Include(c => c.Replies)
                                            .ThenInclude(r => r.AppUser)
                                            .ThenInclude(a => a!.Avatar)
                                            .Select(c => c.ToCommentDto())
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
            var notification = new Notification
            {
                Type = NotificationType.DiscussionComment,
                EngagerId = appUserId,
                TargetId = discussion.AppUserId!,
                EntityId = comment.Id,
            };

            await _context.Notifications.AddAsync(notification);
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
            {
                var notification = new Notification
                {
                    Type = NotificationType.DiscussionCommentLike,
                    EngagerId = appUserId,
                    TargetId = comment.AppUserId!,
                    EntityId = comment.Id,
                };

                await _context.Notifications.AddAsync(notification);
                appUser.LikedDiscussionComments.Add(comment);
            }
            else
                appUser.LikedDiscussionComments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpPatch("{commentId:int}/reply")]
        [Authorize]
        public async Task<IActionResult> ReplyToComment([FromRoute] int commentId, [FromBody] CreateReplyDto replyDto)
        {
            var comment = await _context.DiscussionComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound("Invalid Comment Id Provided");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid user provided");
            var reply = replyDto.ToDiscussionCommentReply(appUserId);
            reply.DiscussionCommentId = commentId;
            await _context.DiscussionReplies.AddAsync(reply);
            await _context.SaveChangesAsync();
            var notification = new Notification
            {
                Type = NotificationType.ReplyDiscussionComment,
                EngagerId = appUserId,
                TargetId = comment.AppUserId!,
                EntityId = reply.Id,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return Ok(replyDto);
        }

        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int commentId)
        {
            var comment = await _context.DiscussionComments.FindAsync(commentId);
            if (comment == null)
                return NotFound($"Comment with {commentId} not found");
            _context.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpPatch("{commentId:int}/{replyId:int}/updateLikes")]
        [Authorize]
        public async Task<IActionResult> UpdateReplyLike([FromRoute] int commentId, [FromRoute] int replyId)
        {
            var comment = await _context.DiscussionComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with Id {commentId} was not found.");
            var reply = await _context.DiscussionReplies.Where(r => r.Id == replyId)
                                                        .Include(r => r.LikedByUsers).FirstOrDefaultAsync();
            if (reply == null)
                return NotFound($"Reply with {replyId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Please provide proper credentials");
            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null)
                return Unauthorized("Please provide valid credentials. User not found");
            // If the user has already liked this reoly, decrement its likes
            if (reply.LikedByUsers.Exists(u => u.Id == appUserId))
            {
                appUser.LikedDiscussionReplys.Remove(reply);
                reply.Likes -= 1;
            }
            else
            {
                appUser.LikedDiscussionReplys.Add(reply);
                reply.Likes += 1;
                var notification = new Notification
                {
                    Type = NotificationType.DiscussionCommentReplyLike,
                    EngagerId = appUserId,
                    TargetId = reply.AppUserId!,
                    EntityId = reply.Id,
                };

                await _context.Notifications.AddAsync(notification);
            }
            await _context.SaveChangesAsync();
            return Ok(reply.ToReplyDto());
        }

        [HttpDelete("{commentId:int}/{replyId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteReply([FromRoute] int commentId, [FromRoute] int replyId)
        {
            var comment = await _context.DiscussionComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with Id {commentId} was not found.");
            var reply = await _context.DiscussionReplies.Where(r => r.Id == replyId)
                                                        .Include(r => r.LikedByUsers).FirstOrDefaultAsync();
            if (reply == null)
                return NotFound($"Reply with {replyId} not found");
            _context.Remove(reply);
            await _context.SaveChangesAsync();
            return Ok(reply);
        }
    }
}