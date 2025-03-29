using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Posts;
using RockServers.Models;
using RockServers.Mappers;

namespace RockServers.Mappers
{
    public static class PostMappers
    {
        public static PostDto ToPostDto(this Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                PostedAt = post.PostedAt,
                GameId = post.GameId,
                GameName = post.Game!.Title,
                PlatformId = post.PlatformId,
                PlatformName = post.Platform!.Name,
                AppUserId = post.AppUserId,
                AppUser = post.AppUser!.ToPostedByUserDto(),
                Comments = [.. post.Comments.Select(c => c.ToCommentDto())],
                Sessions = [.. post.Sessions.Select(s => s.ToSessionDto())],
                ImagePath = post.ImagePath,
                Views = post.Views,
                Likes = post.Likes,
                Dislikes = post.Dislikes
            };
        }
    }
}