using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class DiscussionComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public int DiscussionId { get; set; }
        public Discussion? Discussion { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
        public List<Reply> Replies { get; set; } = [];
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
    }
}