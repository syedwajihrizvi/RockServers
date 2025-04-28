using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace RockServers.Models
{
    public class CommentReply
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; } = DateTime.Now;
        public AppUser? AppUser { get; set; }
        public string? AppUserId { get; set; } = string.Empty;
        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }
        public int Likes { get; set; } = 0;

    }
}