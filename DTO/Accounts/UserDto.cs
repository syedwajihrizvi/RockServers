using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RockServers.Models;
using SixLabors.ImageSharp;

namespace RockServers.DTO.Accounts
{
    public class BaseUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
    }

    public class UserDto : BaseUserDto
    {
        public string Id { get; set; } = string.Empty;
    }

    public class PostedByUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Psn { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
    }

    public class UserInformationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public List<int> LikedPosts { get; set; } = [];
        public List<int> LikedDiscussions { get; set; } = [];
        public List<int> LikedComments { get; set; } = [];
        public List<int> LikedDiscussionComments { get; set; } = [];
        public List<int> LikedPostReplys { get; set; } = [];
        public List<MinimalUserInformationDto> Following { get; set; } = [];
        public List<MinimalUserInformationDto> Followers { get; set; } = [];
        public int? TotalPostings { get; set; }
    }

    public class MinimalUserInformationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? ProfileImage { get; set; }
    }

    public class FollowDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    };
}
