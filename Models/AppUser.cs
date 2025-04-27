using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;

namespace RockServers.Models
{
    public class AppUser : IdentityUser
    {
        public string? GamerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? AvatarId { get; set; }
        public Avatar? Avatar { get; set; }
        public string ProfileImage { get; set; } = string.Empty;
        public List<Post> LikedPosts { get; set; } = [];
        public List<Discussion> LikedDicussions { get; set; } = [];
        public List<Comment> LikedComments { get; set; } = [];
        public List<DiscussionComment> LikedDiscussionComments { get; set; } = [];
        public List<AppUser> Following { get; set; } = [];
        public List<AppUser> Followers { get; set; } = [];
    }
}