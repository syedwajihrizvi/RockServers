using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.Models;

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
        public string? SearchValue { get; set; }
        public int? PostToRemoveId { get; set; }
        public int? PlatformId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public bool MostRecent { get; set; }
        public int? Limit { get; set; }
        public int? Page { get; set; }
        public int PageSize { get; set; } = 8;
    }

    public class DiscussionQueryObject
    {
        public int? GameId { get; set; }
        public string? SearchValue { get; set; }
        public int? DiscussionToRemoveId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public bool MostRecent { get; set; }
        public int? Limit { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 8;

    }


    public class CommentQueryObject
    {
        public int? ContentId { get; set; }
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

    public class NotifactionQueryObject
    {
        public NotificationType? Type { get; set; }
    }
}