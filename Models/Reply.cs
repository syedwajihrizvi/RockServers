using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class DiscussionReply
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; } = DateTime.Now;
        public AppUser? AppUser { get; set; }
        public string? AppUserId { get; set; } = string.Empty;
        public int? Likes { get; set; } = 0;
        public int? DiscussionCommentId { get; set; }
        public DiscussionComment? DiscussionComment { get; set; }
        public List<AppUser> LikedByUsers { get; set; } = [];
    }

    public class PostReply
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; } = DateTime.Now;
        public AppUser? AppUser { get; set; }
        public string? AppUserId { get; set; } = string.Empty;
        public int? Likes { get; set; } = 0;
        public int? PostCommentId { get; set; }
        public PostComment? PostComment { get; set; }
        public List<AppUser> LikedByUsers { get; set; } = [];
    }
}