using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class SessionUser
    {
        public int SessionId { get; set; }
        public Session? Session { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}