using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Accounts
{
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}