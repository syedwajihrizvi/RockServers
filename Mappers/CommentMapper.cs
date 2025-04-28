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

        public static ReplyDto ToReplyDto(this CommentReply commentReply)
        {
            return new ReplyDto
            {
                Content = commentReply.Content,
                RepliedAt = commentReply.RepliedAt,
                AppUser = commentReply.AppUser?.ToMinimalUserInformationDto(),
                Likes = commentReply.Likes,
            };
        }

        public static CommentDto ToCommentDto(this Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Title = comment.Title,
                Content = comment.Content,
                CommentedBy = comment.AppUser!.UserName!,
                AppUser = comment.AppUser.ToMinimalUserInformationDto(),
                Replies = [.. comment.CommentReply.Select(c => c.ToReplyDto())],
                CommentedAt = comment.CommentedAt,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
            };
        }

        public static DiscussionCommentDto ToDiscussionCommentDto(this DiscussionComment discussionComment)
        {
            return new DiscussionCommentDto
            {
                Id = discussionComment.Id,
                Content = discussionComment.Content,
                CommentedBy = discussionComment.AppUser!.UserName!,
                AppUser = discussionComment.AppUser.ToMinimalUserInformationDto(),
                CommentedAt = discussionComment.CommentedAt,
                Likes = discussionComment.Likes,
                Dislikes = discussionComment.Dislikes
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

        public static CommentReply ToCommentReply(this ReplyDto replyDto, string appUserId)
        {
            return new CommentReply
            {
                Content = replyDto.Content,
                AppUserId = appUserId,
            };
        }
    }
}