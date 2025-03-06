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

        public string? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        public List<Comment> Comments { get; set; } = [];

    }
}