using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Comments;
using RockServers.Models;

namespace RockServers.Mappers
{
    public static class CommentMapper
    {
        public static Comment ToCommentFromCreate(this CreateCommentDto createCommentDto, string appUserId)
        {
            return new Comment
            {
                Title = createCommentDto.Title,
                Content = createCommentDto.Content,
                PostId = createCommentDto.PostId,
                AppUserId = appUserId
            };
        }

        public static CommentDto ToCommentDto(this Comment comment)
        {
            return new CommentDto
            {
                Title = comment.Title,
                Content = comment.Content,
                CommentedBy = comment.AppUser!.UserName!,
                AppUserId = comment.AppUserId!,
                CommentedAt = comment.CommentedAt,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes
            };
        }
    }
}