using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Comments
{
    public class CreateDiscussionCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int DiscussionId { get; set; }
    }
}