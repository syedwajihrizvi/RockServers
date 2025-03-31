using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.Models;

namespace RockServers.DTO.Comments
{
    public class ReplyDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; } = DateTime.Now;
        public string? AppUserId { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;

    }

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

    public class DiscussionCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public List<ReplyDto> Replies { get; set; } = [];
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}