using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Accounts
{
    public class CreatedUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}