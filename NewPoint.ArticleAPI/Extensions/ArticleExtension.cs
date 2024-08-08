﻿using Google.Protobuf.WellKnownTypes;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;

namespace NewPoint.ArticleAPI.Extensions;

public static class ArticleExtension
{
    public static ArticleModel ToArticleModel(this Article article)
    {
        return new ArticleModel
        {
            Id = article.Id,
            AuthorId = article.AuthorId,
            Login = article.Login,
            Name = article.Name,
            Surname = article.Surname,
            ProfileImageId = article.ProfileImageId,
            Title = article.Title,
            Content = article.Content,
            Images = article.Images,
            Likes = article.Likes,
            Shares = article.Shares,
            Comments = article.Comments,
            Views = article.Views,
            Liked = article.Liked,
            CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(article.CreationTimestamp)
        };
    }

    public static NullableArticle ToNullableArticle(this ArticleModel data)
    {
        return data is null
            ? new NullableArticle { Null = new NullValue() }
            : new NullableArticle { Data = data };
    }
}