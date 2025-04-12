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
        public static UserDto ToCreatedUserDto(this IdentityUser user, string token)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Username = user.UserName!,
                Token = token,
            };
        }

        public static PostedByUserDto ToPostedByUserDto(this AppUser user)
        {
            return new PostedByUserDto
            {
                Email = user.Email!,
                Username = user.UserName!,
                Psn = user.GamerId!
            };
        }

        public static UserInformationDto ToUserInformationDto(this AppUser user)
        {
            return new UserInformationDto
            {
                Id = user.Id!,
                Email = user.Email!,
                Username = user.UserName!,
                LikedPosts = [.. user.LikedPosts.Select(p => p.Id)!]
            };
        }
    }
}