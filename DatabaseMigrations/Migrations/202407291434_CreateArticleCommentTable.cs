using FluentMigrator;

[Migration(2024_07_29_14_34, "Create article comment table")]
public class CreateArticleCommentTable : Migration
{
    public const string ArticleCommentTable = @"""article_comment""";
    public const string ArticleCommentTableName = "article_comment";
    public const string Id = "id";
    public const string ArticleId = "article_id";
    public const string UserId = "user_id";
    public const string Content = "content";
    public const string Likes = "likes";
    public const string Views = "views";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ArticleCommentTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {ArticleId} bigint NOT NULL,
            {UserId} bigint NOT NULL,
            {Content} varchar(255) NOT NULL,
            {Likes} integer NOT NULL DEFAULT 0,
            {Views} integer NOT NULL DEFAULT 0,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );

        CREATE INDEX idx_{ArticleCommentTableName}_{Id} ON {ArticleCommentTable} USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ArticleCommentTable};
        ");
    }
}