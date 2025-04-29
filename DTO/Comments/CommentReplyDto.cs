using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;

namespace RockServers.DTO.Comments
{
    public class ReplyDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; }
        public MinimalUserInformationDto? AppUser { get; set; }
        public int? Likes { get; set; } = 0;
    }

}