using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Notifications;
using RockServers.Mappers;

namespace RockServers.Models
{
    public enum NotificationType
    {
        Follow,
        PostCommentLike,
        DiscussionCommentLike,
        PostLike,
        DiscussionLike,
        PostComment,
        DiscussionComment,
        PostCommentReplyLike,
        DiscussionCommentReplyLike,
        ReplyPostComment,
        ReplyDiscussionComment
    }

    public class Notification
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string EngagerId { get; set; } = string.Empty;
        public AppUser? Engager { get; set; }
        public string TargetId { get; set; } = string.Empty;
        public AppUser? Target { get; set; }
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public async static
        Task
SaveNotification(Notification notification, ApplicationDBContext context)
        {
            var notificationExists = await context.Notifications
                                                  .Where(n => n.TargetId == notification.TargetId)
                                                  .Where(n => n.EngagerId == notification.EngagerId)
                                                  .Where(n => n.Type == notification.Type)
                                                  .Where(n => n.EntityId == notification.EntityId)
                                                  .FirstOrDefaultAsync();
            if (notificationExists != null)
                context.Remove(notificationExists);
            if (notification.EngagerId != notification.TargetId)
                await context.Notifications.AddAsync(notification);
        }
    }
}