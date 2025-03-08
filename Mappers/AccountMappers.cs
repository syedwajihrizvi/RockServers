using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RockServers.DTO.Accounts;
using RockServers.Models;

namespace RockServers.Mappers
{
    public static class AccountMappers
    {
        public static CreatedUserDto ToCreatedUserDto(this IdentityUser user, string token)
        {
            return new CreatedUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Username = user.UserName!,
                Token = token
            };
        }
    }
}