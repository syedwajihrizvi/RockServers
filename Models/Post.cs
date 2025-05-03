using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RockServers.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; } = DateTime.Now;
        // Convention to have ID and Navigation reference (for us)
        public int? GameId { get; set; }

        public Game? Game { get; set; }

        public int? PlatformId { get; set; }

        public Platform? Platform { get; set; }

        public List<Session> Sessions { get; set; } = [];

        public string? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        public List<PostComment> Comments { get; set; } = [];

        public string ThumbnailPath { get; set; } = string.Empty;

        public int Views { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public List<AppUser> LikedByUsers { get; set; } = [];
    }
}