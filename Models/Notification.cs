using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}