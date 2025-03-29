using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;

namespace RockServers.DTO.Discussions
{
    public class DiscussionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string? AppUserId { get; set; }
        public PostedByUserDto? AppUser { get; set; }
        public int Likes = 0;
        public int Views = 0;

    }
}