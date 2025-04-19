using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Posts
{
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int GameId { get; set; }
        public int PlatformId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }

    public class CreatePostDtoWithCustomImage
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int GameId { get; set; }
        public int PlatformId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}