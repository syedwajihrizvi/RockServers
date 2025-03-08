using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO
{
    public class CreateGameDto
    {
        public string Title { get; set; } = string.Empty;

        public string GetSlug()
        {
            return Title.Replace(" ", "-").ToLower();
        }
    }
}