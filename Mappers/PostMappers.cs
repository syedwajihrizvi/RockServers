using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Posts;
using RockServers.Models;

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
                AppUserId = post.AppUserId,
                AppUser = post.AppUser!.ToPostedByUserDto(),
                Comments = post.Comments,
                Views = post.Views,
                Likes = post.Likes,
                Dislikes = post.Dislikes
            };
        }
    }
}