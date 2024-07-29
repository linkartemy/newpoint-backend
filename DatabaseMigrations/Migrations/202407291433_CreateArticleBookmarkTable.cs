using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_33, "Create article bookmark table")]
public class CreateArticleBookmarkTable : Migration
{
    public const string ArticleBookmarkTable = @"""article_bookmark""";
    public const string ArticleBookmarkTableName = "article_bookmark";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string ArticleId = "article_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ArticleBookmarkTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {ArticleId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );

        CREATE INDEX idx_{ArticleBookmarkTableName}_{UserId} ON {ArticleBookmarkTable}({UserId});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ArticleBookmarkTable};
        ");
    }
}