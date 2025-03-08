using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Helpers
{
    public class QueryObject
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public bool Ascending { get; set; } = true;
        public int? Posts_gt { get; set; }
        public int? Posts_lt { get; set; }
        public int? Posts_eq { get; set; }
    }
}