using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;
using RockServers.Models;

namespace RockServers.DTO.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public MinimalUserInformationDto? AppUser { get; set; }
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public List<ReplyDto> Replies { get; set; } = [];
    }

    public class DiscussionCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public MinimalUserInformationDto? AppUser { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}