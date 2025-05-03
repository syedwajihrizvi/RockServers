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
                PostedAt = discussion.PostedAt,
                Content = discussion.Content,
                ThumbnailPath = discussion.ThumbnailPath,
                ThumbnailType = discussion.ThumbnailType,
                VideoPaths = discussion.VideoPaths,
                AppUserId = discussion.AppUserId,
                AppUser = discussion.AppUser!.ToPostedByUserDto(),
                Likes = discussion.Likes,
                Comments = discussion.DiscussionComments.Count,
                Views = discussion.Views
            };
        }
        public static GetDiscussionDto ToGetDiscussionDto(this Discussion discussion)
        {
            return new GetDiscussionDto
            {
                Id = discussion.Id,
                Title = discussion.Title,
                PostedAt = discussion.PostedAt,
                Content = discussion.Content,
                GameId = discussion.GameId,
                GameName = discussion.Game!.Title,
                ThumbnailPath = discussion.ThumbnailPath,
                ThumbnailType = discussion.ThumbnailType,
                OtherImages = discussion.OtherImages,
                VideoPaths = discussion.VideoPaths,
                AppUserId = discussion.AppUserId,
                AppUser = discussion.AppUser!.ToPostedByUserDto(),
                Comments = [.. discussion.DiscussionComments.Select(d => d.ToCommentDto())],
                Likes = discussion.Likes,
                Views = discussion.Views
            };
        }

    }
}