using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Discussions;
using RockServers.DTO.Posts;

namespace RockServers.Helpers
{
    public class PostDataObject
    {
        public List<PostDto> Data { get; set; } = [];
        public bool hasMore { get; set; }
    }

    public class DiscussionDataObject
    {
        public List<DiscussionDto> Data { get; set; } = [];
        public bool hasMore { get; set; }
    }
}