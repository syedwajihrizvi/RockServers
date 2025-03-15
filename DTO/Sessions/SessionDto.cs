using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.Models;

namespace RockServers.DTO.Sessions
{
    public class SessionDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<SessionUserDto> Users { get; set; } = [];
        public bool Active { get; set; }
    }
}