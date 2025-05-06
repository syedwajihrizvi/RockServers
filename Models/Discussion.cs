using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public enum ThumbnailType
    {
        Image,
        Video
    }

    public class Discussion
    {
        public int Id { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.Now;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public Game? Game { get; set; }
        public AppUser? AppUser { get; set; }
        public string ThumbnailPath { get; set; } = string.Empty;
        public ThumbnailType ThumbnailType { get; set; } = ThumbnailType.Image;
        [Column(TypeName = "json")]
        public List<string> OtherImages { get; set; } = [];
        [Column(TypeName = "json")]
        public List<string> VideoPaths { get; set; } = [];
        public string? AppUserId { get; set; }
        public List<DiscussionComment> DiscussionComments { get; set; } = [];
        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;
        public List<AppUser> LikedByUsers { get; set; } = [];
        public string Tags { get; set; } = string.Empty;
    }
}