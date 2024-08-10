using Google.Protobuf.WellKnownTypes;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

public static class PostCommentExtension
{
    public static CommentModel ToCommentModel(this Comment comment)
    {
        return new CommentModel
        {
            Id = comment.Id,
            UserId = comment.UserId,
            Login = comment.Login,
            Name = comment.Name,
            Surname = comment.Surname,
            Content = comment.Content,
            Likes = comment.Likes,
            Liked = comment.Liked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(comment.CreationTimestamp)
        };
    }
}