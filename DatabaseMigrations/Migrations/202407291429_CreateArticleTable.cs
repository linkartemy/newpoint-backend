using FluentMigrator;

[Migration(2024_07_29_14_29, "Create article table")]
public class CreateArticleTable : Migration
{
    public const string ArticleTable = @"""article""";
    public const string ArticleTableName = "article";
    public const string Id = "id";
    public const string AuthorId = "author_id";
    public const string Title = "title";
    public const string Content = "content";
    public const string Images = "images";
    public const string Likes = "likes";
    public const string Shares = "shares";
    public const string Comments = "comments";
    public const string Views = "views";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ArticleTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {AuthorId} bigint NOT NULL,
            {Title} varchar(255) NOT NULL,
            {Content} text NOT NULL,
            {Images} jsonb NOT NULL DEFAULT '{"{}"}'::jsonb,
            {CreationTimestamp} timestamp with time zone NOT NULL,
            {Likes} integer DEFAULT 0,
            {Shares} integer DEFAULT 0,
            {Comments} integer DEFAULT 0,
            {Views} bigint DEFAULT 0
        );

        CREATE INDEX idx_{ArticleTableName}_{Id} ON {ArticleTable} USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ArticleTable};
        ");
    }
}