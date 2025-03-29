using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class Reply
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; } = DateTime.Now;
        public AppUser? AppUser { get; set; }
        public string? AppUserId { get; set; } = string.Empty;

    }
}