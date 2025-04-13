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
        public static ReplyDto ToReplyDto(this Reply reply)
        {
            return new ReplyDto
            {
                Content = reply.Content,
                RepliedAt = reply.RepliedAt,
                AppUserId = reply.AppUser!.Id,
                CommentedBy = reply.AppUser.UserName!
            };
        }

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
                Id = comment.Id,
                Title = comment.Title,
                Content = comment.Content,
                CommentedBy = comment.AppUser!.UserName!,
                AppUserId = comment.AppUserId!,
                CommentedAt = comment.CommentedAt,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes
            };
        }

        public static DiscussionCommentDto ToDiscussionCommentDto(this DiscussionComment discussionComment)
        {
            return new DiscussionCommentDto
            {
                Id = discussionComment.Id,
                Content = discussionComment.Content,
                CommentedBy = discussionComment.AppUser!.UserName!,
                AppUserId = discussionComment.AppUserId,
                Replies = [.. discussionComment.Replies.Select(s => s.ToReplyDto())],
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
    }
}