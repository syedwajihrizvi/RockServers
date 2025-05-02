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
    [Route("api/comments")]
    [ApiController]
    public class PostCommentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        public PostCommentController(ApplicationDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject queryObject)
        {
            var comments = _context.PostComments.AsQueryable();

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
                                            .Include(c => c.Replies)
                                            .ThenInclude(r => r.AppUser)
                                            .ThenInclude(a => a!.Avatar)
                                            .Select(c => c.ToCommentDto())
                                            .ToListAsync();
            return Ok(commentsDto);
        }

        [HttpGet("{commentId:int}")]
        public async Task<IActionResult> GetComment([FromRoute] int commentId)
        {
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
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
            await _context.PostComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            var notification = new Notification
            {
                Type = NotificationType.PostComment,
                EngagerId = appUserId,
                TargetId = post.AppUserId!,
                EntityId = comment.Id,
            };

            await Notification.SaveNotification(notification, _context);
            await _context.SaveChangesAsync();
            return Ok(createCommentDto);
        }

        [HttpPatch("{commentId:int}/updateLikes")]
        public async Task<IActionResult> UpdatePostLikes([FromRoute] int commentId, [FromBody] bool increment)
        {
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return base.NotFound($"Post with {comment} does not exist");
            comment.Likes += increment ? 1 : -1;
            comment.Likes = comment.Likes < 0 ? 0 : comment.Likes;
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User not valid");
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikesPostComments)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("User not valid");
            if (increment)
            {
                var notification = new Notification
                {
                    Type = NotificationType.PostCommentLike,
                    EngagerId = appUserId,
                    TargetId = comment.AppUserId!,
                    EntityId = comment.Id,
                };
                await Notification.SaveNotification(notification, _context);
                appUser.LikesPostComments.Add(comment);
            }
            else
                appUser.LikesPostComments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpPatch("{commentId:int}/updateComment")]
        [Authorize]
        public async Task<IActionResult> UpdateComment([FromRoute] int commentId, [FromBody] CreateCommentDto commentDto)
        {
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with ID ${commentId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid user request");
            // Ensure the user sending the request matches the user that made the comment
            if (comment.AppUserId != appUserId)
                return Unauthorized("Invalid user Id");
            if (!string.IsNullOrWhiteSpace(commentDto.Content))
                comment.Content = commentDto.Content;
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpPatch("{commentId:int}/reply")]
        [Authorize]
        public async Task<IActionResult> ReplyToComment([FromRoute] int commentId, [FromBody] CreateReplyDto replyDto)
        {
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound("Invalid Comment Id Provided");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid user provided");
            var reply = replyDto.ToPostCommentReply(appUserId);
            reply.PostCommentId = commentId;
            await _context.PostReplies.AddAsync(reply);
            await _context.SaveChangesAsync();
            var notification = new Notification
            {
                Type = NotificationType.ReplyPostComment,
                EngagerId = appUserId,
                TargetId = comment.AppUserId!,
                EntityId = reply.Id,
            };
            await Notification.SaveNotification(notification, _context);
            await _context.SaveChangesAsync();
            return Ok(replyDto);
        }

        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int commentId)
        {
            var comment = await _context.PostComments.FindAsync(commentId);
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
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with Id {commentId} was not found.");
            var reply = await _context.PostReplies.Where(r => r.Id == replyId)
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
                appUser.LikedPostReplys.Remove(reply);
                reply.Likes -= 1;
            }
            else
            {
                appUser.LikedPostReplys.Add(reply);
                reply.Likes += 1;
                // Create Notifaction
                var notification = new Notification
                {
                    Type = NotificationType.PostCommentReplyLike,
                    EngagerId = appUserId,
                    TargetId = reply.AppUserId!,
                    EntityId = reply.Id,
                };

                await Notification.SaveNotification(notification, _context);
            }
            await _context.SaveChangesAsync();
            return Ok(reply.ToReplyDto());
        }

        [HttpDelete("{commentId:int}/{replyId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteReply([FromRoute] int commentId, [FromRoute] int replyId)
        {
            var comment = await _context.PostComments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound($"Comment with Id {commentId} was not found.");
            var reply = await _context.PostReplies.Where(r => r.Id == replyId)
                                                  .Include(r => r.LikedByUsers).FirstOrDefaultAsync();
            if (reply == null)
                return NotFound($"Reply with {replyId} not found");
            _context.Remove(reply);
            await _context.SaveChangesAsync();
            return Ok(reply);
        }
    }
}