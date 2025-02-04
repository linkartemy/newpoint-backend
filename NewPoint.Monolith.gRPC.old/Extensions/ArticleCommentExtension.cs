using Google.Protobuf.WellKnownTypes;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

public static class ArticleCommentExtension
{
    public static ArticleCommentModel ToArticleCommentModel(this ArticleComment articleComment)
    {
        return new ArticleCommentModel
        {
            Id = articleComment.Id,
            UserId = articleComment.UserId,
            Login = articleComment.Login,
            Name = articleComment.Name,
            Surname = articleComment.Surname,
            Content = articleComment.Content,
            Likes = articleComment.Likes,
            Liked = articleComment.Liked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(articleComment.CreationTimestamp)
        };
    }
}