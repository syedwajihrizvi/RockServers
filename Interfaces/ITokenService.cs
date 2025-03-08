using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RockServers.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(IdentityUser identityUser);
    }
}