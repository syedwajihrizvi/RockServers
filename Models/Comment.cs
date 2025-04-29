using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class PostComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public List<AppUser> LikedByUsers { get; set; } = [];
        public List<PostReply> Replies { get; set; } = [];
        public int PostId { get; set; }
        public Post? Post { get; set; }
    }

    public class DiscussionComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public List<AppUser> LikedByUsers { get; set; } = [];
        public List<DiscussionReply> Replies { get; set; } = [];
        public int DiscussionId { get; set; }
        public Discussion? Discussion { get; set; }
    }
}