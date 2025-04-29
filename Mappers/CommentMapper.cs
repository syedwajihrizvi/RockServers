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
        public static PostComment ToCommentFromCreate(this CreateCommentDto createCommentDto, string appUserId)
        {
            return new PostComment
            {
                Content = createCommentDto.Content,
                PostId = createCommentDto.PostId,
                AppUserId = appUserId
            };
        }

        public static ReplyDto ToReplyDto(this PostReply reply)
        {
            return new ReplyDto
            {
                Content = reply.Content,
                RepliedAt = reply.RepliedAt,
                AppUser = reply.AppUser?.ToMinimalUserInformationDto(),
                Likes = reply.Likes,
            };
        }

        public static ReplyDto ToReplyDto(this DiscussionReply reply)
        {
            return new ReplyDto
            {
                Content = reply.Content,
                RepliedAt = reply.RepliedAt,
                AppUser = reply.AppUser?.ToMinimalUserInformationDto(),
                Likes = reply.Likes,
            };
        }

        public static CommentDto ToCommentDto(this PostComment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CommentedBy = comment.AppUser!.UserName!,
                AppUser = comment.AppUser.ToMinimalUserInformationDto(),
                Replies = [.. comment.Replies.Select(c => c.ToReplyDto())],
                CommentedAt = comment.CommentedAt,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
            };
        }

        public static CommentDto ToCommentDto(this DiscussionComment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CommentedBy = comment.AppUser!.UserName!,
                AppUser = comment.AppUser.ToMinimalUserInformationDto(),
                Replies = [.. comment.Replies.Select(c => c.ToReplyDto())],
                CommentedAt = comment.CommentedAt,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
            };
        }

        public static DiscussionComment ToDiscussionCommentFromCreate(this CreateDiscussionCommentDto createDiscussionCommentDto, string appUserId)
        {
            return new DiscussionComment
            {
                Content = createDiscussionCommentDto.Content,
                DiscussionId = createDiscussionCommentDto.DiscussionId,
                AppUserId = appUserId
            };
        }

        public static PostReply ToPostCommentReply(this CreateReplyDto replyDto, string appUserId)
        {
            return new PostReply
            {
                Content = replyDto.Content,
                AppUserId = appUserId,
            };
        }

        public static DiscussionReply ToDiscussionCommentReply(this CreateReplyDto replyDto, string appUserId)
        {
            return new DiscussionReply
            {
                Content = replyDto.Content,
                AppUserId = appUserId,
            };
        }
    }
}