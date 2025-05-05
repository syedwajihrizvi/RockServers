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
        public string ThumbnailPath { get; set; } = string.Empty;
        public IFormFile? ThumbnailFile { get; set; }
        public IFormFile[] OtherImages { get; set; } = [];
        public IFormFile[] OtherVideos { get; set; } = [];
    }

    public class UpdateDiscussionDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? GameId { get; set; }
        public string? ThumbnailPath { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public IFormFile[]? NewImages { get; set; }
        public IFormFile[]? NewVideos { get; set; }
        public List<string>? ExistingImages { get; set; }
        public List<string>? ExistingVideos { get; set; }
    }
}