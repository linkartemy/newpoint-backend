using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_35, "Create article comment like table")]
public class CreateArticleCommentLikeTable : Migration
{
    public const string ArticleCommentLikeTable = @"""article_comment_like""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string CommentId = "comment_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ArticleCommentLikeTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {CommentId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ArticleCommentLikeTable};
        ");
    }
}