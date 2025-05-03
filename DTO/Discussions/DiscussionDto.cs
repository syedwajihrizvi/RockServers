using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;
using RockServers.DTO.Comments;
using RockServers.Models;

namespace RockServers.DTO.Discussions
{
    public class DiscussionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public ThumbnailType ThumbnailType { get; set; }
        public List<string>? VideoPaths { get; set; }
        public string? AppUserId { get; set; }
        public PostedByUserDto? AppUser { get; set; }
        public int Comments { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;
    }
    public class GetDiscussionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public ThumbnailType ThumbnailType { get; set; }
        public List<string>? OtherImages { get; set; }
        public List<string>? VideoPaths { get; set; }
        public string? AppUserId { get; set; }
        public PostedByUserDto? AppUser { get; set; }
        public List<CommentDto> Comments { get; set; } = [];
        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;
    }
}