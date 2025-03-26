using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Helpers
{
    public class GameQueryObject
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public string SortBy { get; set; } = string.Empty;
        public bool Ascending { get; set; } = true;
        public int? Posts_gte { get; set; }
        public int? Posts_lte { get; set; }
        public int? Posts_eq { get; set; }
    }

    public class PostQueryObject
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public int? PlatformId { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public int? Views_lte { get; set; }
        public int? Views_eq { get; set; }
        public int? Views_gte { get; set; }
        public int? Likes_gte { get; set; }
        public int? Likes_lte { get; set; }
        public int? Likes_eq { get; set; }
        public int? Dislikes_gte { get; set; }
        public int? Dislikes_lte { get; set; }
        public int? Dislikes_eq { get; set; }
        public int? Comments_gte { get; set; }
        public int? Comments_lte { get; set; }
        public int Comments_eq { get; set; }
        public bool Latest { get; set; }
    }

    public class CommentQueryObject
    {
        public int? PostId { get; set; }
        public bool Latest { get; set; }
        public bool SortByMostLikes { get; set; }
        public bool SortByMostDislikes { get; set; }
    }

    public class SessionQueryObject
    {
        public int? PostId { get; set; }
        public bool Completed { get; set; }
        public bool Active { get; set; }
    }
}