using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class Discussion
    {
        public int Id { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.Now;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public Game? Game { get; set; }
        public AppUser? AppUser { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        [Column(TypeName = "json")]
        public List<string>? OtherImages { get; set; }
        public string? AppUserId { get; set; }
        public List<DiscussionComment> DiscussionComments { get; set; } = [];
        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;
    }
}