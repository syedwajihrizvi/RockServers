using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Discussions
{
    public class CreateDiscussionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public IFormFile[] OtherImages { get; set; } = [];
    }

    public class CreateDiscussionDtoWithCustomImage
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile[] OtherImages { get; set; } = [];
    }
}