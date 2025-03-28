using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Discussions;
using RockServers.Models;

namespace RockServers.Mappers
{
    public static class DiscussionMappers
    {
        public static DiscussionDto ToDiscussionDto(this Discussion discussion)
        {
            return new DiscussionDto
            {
                Id = discussion.Id,
                Title = discussion.Title,
                Content = discussion.Content,
                GameId = discussion.GameId,
                GameName = discussion.Game!.Title,
                ImagePath = discussion.ImagePath,
                AppUserId = discussion.AppUserId,
                AppUser = discussion.AppUser!.ToPostedByUserDto(),
                Likes = discussion.Likes,
                Views = discussion.Views
            };
        }
    }
}