using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Notifications;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;
using RockServers.Models;

namespace RockServers.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        public NotificationController(ApplicationDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<NotificationDto> GetNotificationDto(Notification notification)
        {


            string notificationStringFromEntity = "";
            int? postOrDescId = null;
            if (notification.Type == NotificationType.PostCommentLike)
            {
                var comment = await _context.PostComments
                                            .Where(c => c.Id == notification.EntityId)
                                            .Include(c => c.Post)
                                            .FirstOrDefaultAsync();
                if (comment != null && comment.Post != null)
                {
                    notificationStringFromEntity = comment.Post.Title;
                    postOrDescId = comment.Post.Id;
                }
            }
            else if (notification.Type == NotificationType.DiscussionCommentLike)
            {
                var comment = await _context.DiscussionComments
                                            .Where(c => c.Id == notification.EntityId)
                                            .Include(c => c.Discussion)
                                            .FirstOrDefaultAsync();
                if (comment != null && comment.Discussion != null)
                {
                    notificationStringFromEntity = comment.Discussion.Title;
                    postOrDescId = comment.Discussion.Id;
                }
            }
            else if (notification.Type == NotificationType.PostLike)
            {
                var comment = await _context.Posts
                                            .Where(c => c.Id == notification.EntityId)
                                            .FirstOrDefaultAsync();
                if (comment != null)
                    notificationStringFromEntity = comment.Title;
            }
            else if (notification.Type == NotificationType.DiscussionLike)
            {
                var comment = await _context.Discussions
                                            .Where(c => c.Id == notification.EntityId)
                                            .FirstOrDefaultAsync();
                if (comment != null)
                    notificationStringFromEntity = comment.Title;
            }
            else if (notification.Type == NotificationType.PostComment)
            {
                var comment = await _context.PostComments
                                        .Where(c => c.Id == notification.EntityId)
                                        .Include(c => c.Post)
                                        .FirstOrDefaultAsync();
                if (comment != null)
                {
                    notificationStringFromEntity = comment.Post?.Title!;
                    postOrDescId = comment.PostId;
                }
            }
            else if (notification.Type == NotificationType.DiscussionComment)
            {
                var comment = await _context.DiscussionComments
                                               .Where(d => d.Id == notification.EntityId)
                                               .Include(d => d.Discussion)
                                               .FirstOrDefaultAsync();
                if (comment != null)
                {
                    notificationStringFromEntity = comment.Discussion?.Title!;
                    postOrDescId = comment.Discussion?.Id;
                }
            }
            else if (notification.Type == NotificationType.PostCommentReplyLike)
            {
                var reply = await _context.PostReplies.Where(r => r.Id == notification.EntityId)
                                                      .Include(r => r.PostComment)
                                                      .ThenInclude(c => c!.Post)
                                                      .FirstOrDefaultAsync();
                if (reply != null)
                {
                    notificationStringFromEntity = reply.PostComment?.Post?.Title!;
                    postOrDescId = reply.PostComment?.Post?.Id;
                }
            }
            else if (notification.Type == NotificationType.DiscussionCommentReplyLike)
            {
                var reply = await _context.DiscussionReplies
                                         .Where(d => d.Id == notification.EntityId)
                                         .Include(r => r.DiscussionComment)
                                         .ThenInclude(c => c!.Discussion)
                                         .FirstOrDefaultAsync();
                if (reply != null)
                {
                    notificationStringFromEntity = reply?.DiscussionComment?.Discussion?.Title!;
                    postOrDescId = reply?.DiscussionComment?.Discussion?.Id;
                }
            }
            else if (notification.Type == NotificationType.ReplyPostComment)
            {
                var reply = await _context.PostReplies.Where(r => r.Id == notification.EntityId)
                                                      .Include(r => r.PostComment)
                                                      .ThenInclude(c => c!.Post)
                                                      .FirstOrDefaultAsync();
                if (reply != null)
                {
                    notificationStringFromEntity = reply.PostComment?.Post?.Title!;
                    postOrDescId = reply.PostComment?.Post?.Id;
                }
            }
            else if (notification.Type == NotificationType.ReplyDiscussionComment)
            {
                var reply = await _context.DiscussionReplies
                                         .Where(d => d.Id == notification.EntityId)
                                         .Include(r => r.DiscussionComment)
                                         .ThenInclude(c => c!.Discussion)
                                         .FirstOrDefaultAsync();
                if (reply != null)
                {
                    notificationStringFromEntity = reply?.DiscussionComment?.Discussion?.Title!;
                    postOrDescId = reply?.DiscussionComment?.Discussion?.Id;
                }
            }
            return notification.ToNotificationDto(notificationStringFromEntity, postOrDescId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] NotifactionQueryObject queryObject)
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Unauthorized user");
            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null)
                return Unauthorized("Unauthorized user");
            var notifications = _context.Notifications.Where(n => n.TargetId == appUserId).AsQueryable();
            if (queryObject.Type != null)
                notifications = notifications.Where(n => n.Type == queryObject.Type);
            var notificationsFromDb = await notifications
                                        .Include(n => n.Target)
                                        .ThenInclude(a => a!.Avatar)
                                        .Include(n => n.Engager)
                                        .ThenInclude(a => a!.Avatar)
                                        .ToListAsync();
            var finalNotificationDto = new List<NotificationDto>();
            foreach (var notification in notificationsFromDb)
            {
                var notificationDto = await GetNotificationDto(notification);
                finalNotificationDto.Add(notificationDto);
            }
            return Ok(finalNotificationDto);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User Credentials");
            await _context.Notifications.Where(n => n.TargetId == appUserId).ExecuteDeleteAsync();
            return Ok();
        }
    }
}