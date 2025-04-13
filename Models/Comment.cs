using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; } = DateTime.Now;

        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public List<AppUser> LikedByUsers { get; set; } = [];

    }
}