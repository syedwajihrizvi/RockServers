using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.Models;

namespace RockServers.DTO.Posts
{
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public int PlatformId { get; set; }
        public string ThumbnailPath { get; set; } = string.Empty;
        public IFormFile? ThumbnailFile { get; set; }
    }

}