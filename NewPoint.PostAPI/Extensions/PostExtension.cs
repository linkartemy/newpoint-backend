using Google.Protobuf.WellKnownTypes;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;

namespace NewPoint.PostAPI.Extensions;

public static class PostExtension
{
    public static PostModel ToPostModel(this Post post)
    {
        return new PostModel
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            Login = post.Login,
            Name = post.Name,
            Surname = post.Surname,
            ProfileImageId = post.ProfileImageId,
            Content = post.Content,
            Images = post.Images,
            Likes = post.Likes,
            Shares = post.Shares,
            Comments = post.Comments,
            Views = post.Views,
            Liked = post.Liked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(post.CreationTimestamp)
        };
    }
}