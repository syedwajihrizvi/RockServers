using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public List<SessionUser> Users { get; set; } = [];

        [NotMapped]
        public bool Active => !EndTime.HasValue;


    }
}