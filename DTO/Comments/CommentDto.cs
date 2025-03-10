using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Comments
{
    public class CommentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}