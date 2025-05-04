using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;
using RockServers.DTO.Comments;
using RockServers.DTO.Sessions;
using RockServers.Models;

namespace RockServers.DTO.Posts
{
    public class PostDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime PostedAt { get; set; } = DateTime.Now;
        public DateTime StartTime { get; set; } = DateTime.Now;

        // Convention to have ID and Navigation reference (for us)
        public int? GameId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string? AppUserId { get; set; }

        public PostedByUserDto? AppUser { get; set; }

        public int? PlatformId { get; set; }

        public string PlatformName { get; set; } = string.Empty;

        public List<CommentDto> Comments { get; set; } = [];
        public string ThumbnailPath { get; set; } = string.Empty;
        public ThumbnailType? ThumbnailType { get; set; }
        public List<SessionDto> Sessions { get; set; } = [];
        public bool ActiveSession { get; set; }
        public int Views { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
    }
}