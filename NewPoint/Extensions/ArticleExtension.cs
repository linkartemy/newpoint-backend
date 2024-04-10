﻿using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

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
}