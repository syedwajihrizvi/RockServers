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
        public static UserDto ToCreatedUserDto(this AppUser user, string token)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Username = user.UserName!,
                Avatar = user.Avatar?.Name!,
                ProfileImage = user.ProfileImage,
                Token = token,
            };
        }

        public static PostedByUserDto ToPostedByUserDto(this AppUser user)
        {
            return new PostedByUserDto
            {
                Email = user.Email!,
                Username = user.UserName!,
                Psn = user.GamerId!,
                Avatar = user.Avatar?.Name!,
                ProfileImage = user.ProfileImage
            };
        }

        public static UserInformationDto ToUserInformationDto(this AppUser user)
        {
            return new UserInformationDto
            {
                Id = user.Id!,
                Email = user.Email!,
                Username = user.UserName!,
                Avatar = user.Avatar?.Name!,
                ProfileImage = user.ProfileImage,
                LikedPosts = [.. user.LikedPosts.Select(p => p.Id)!],
                LikedDiscussions = [.. user.LikedDicussions.Select(d => d.Id)!],
                LikedComments = [.. user.LikedComments.Select(c => c.Id)!],
                LikedDiscussionComments = [.. user.LikedDiscussionComments.Select(d => d.Id)!],
                Following = [.. user.Following?.Select(u => u.ToMinimalUserInformationDto()) ?? []],
                Followers = [.. user.Followers?.Select(u => u.ToMinimalUserInformationDto()) ?? []]
            };
        }

        public static MinimalUserInformationDto ToMinimalUserInformationDto(this AppUser user)
        {
            return new MinimalUserInformationDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Avatar = user.Avatar?.Name,
                ProfileImage = user.ProfileImage!,
            };
        }
    }
}