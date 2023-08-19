using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

public static class PostExtension
{
    public static PostModel ToPostModel(this Post post)
    {
        return new PostModel
        {
            Id = post.Id, AuthorId = post.AuthorId, Login = post.Login, Name = post.Name,
            Surname = post.Surname, Content = post.Content, Images = post.Images,
            Likes = post.Likes, Shares = post.Shares, Comments = post.Comments, Liked = post.Liked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(post.CreationTimestamp)
        };
    }
}