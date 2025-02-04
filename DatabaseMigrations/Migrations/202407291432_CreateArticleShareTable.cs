using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_32, "Create article share table")]
public class CreateArticleShareTable : Migration
{
    public const string ArticleShareTable = @"""article_share""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string ArticleId = "article_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ArticleShareTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {ArticleId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ArticleShareTable};
        ");
    }
}