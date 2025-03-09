using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RockServers.Extensions
{
    public static class ClaimsExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            var claimId = user.FindFirst(ClaimTypes.NameIdentifier);
            var claimUsername = user.FindFirst(ClaimTypes.GivenName);
            if (claimId == null)
                return null;
            return claimId.Value;
        }
    }
}