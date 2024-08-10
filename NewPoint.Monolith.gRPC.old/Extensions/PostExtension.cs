using Google.Protobuf.WellKnownTypes;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

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
            Shared = post.Shared,
            Bookmarked = post.Bookmarked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(post.CreationTimestamp)
        };
    }

    public static NullablePost ToNullablePost(this PostModel? data)
    {
        return data is null
            ? new NullablePost { Null = new NullValue() }
            : new NullablePost { Data = data };
    }

    public static NullableInt64 ToNullableInt64(this long? data)
    {
        return data is null
            ? new NullableInt64 { Null = new NullValue() }
            : new NullableInt64 { Data = data.Value };
    }
}