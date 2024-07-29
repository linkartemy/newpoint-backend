using FluentMigrator;

[Migration(2024_07_29_13_35, "Create Post table")]
public class CreatePostTable : Migration
{
    public const string PostTable = @"""post""";
    public const string PostTableName = "post";
    public const string Id = "id";
    public const string AuthorId = "author_id";
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
        CREATE TABLE {PostTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {AuthorId} bigint NOT NULL,
            {Content} text NOT NULL,
            {Images} jsonb NOT NULL DEFAULT '{"{}"}'::jsonb,
            {CreationTimestamp} timestamp with time zone NOT NULL,
            {Likes} integer DEFAULT 0,
            {Shares} integer DEFAULT 0,
            {Comments} integer DEFAULT 0,
            {Views} bigint DEFAULT 0
        );

        CREATE INDEX idx_{PostTableName}_{Id} ON {PostTable} USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {PostTable};
        ");
    }
}