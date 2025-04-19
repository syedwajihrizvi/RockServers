using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class Images
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public Game? Game { get; set; }
    }
}