using FluentMigrator;

[Migration(2023_05_14_16_40, "Create Post table")]
public class CreatePostTable : Migration {
    public const string PostTable = @"""post""";
    public const string Id = "id";
    public const string AuthorId = "author_id";
    public const string Content = "content";
    public const string Images = "images";
    public const string Likes = "likes";
    public const string Shares = "shares";
    public const string Comments = "comments";
    public const string CreationTimeStamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostTable} (
            {Id} BIGSERIAL PRIMARY KEY,
            {AuthorId} BIGINT NOT NULL,
            {Content} TEXT NOT NULL,
            {Images} JSONB,
            {Likes} INTEGER DEFAULT 0,
            {Shares} INTEGER DEFAULT 0,
            {Comments} INTEGER DEFAULT 0,
            {CreationTimeStamp} TIMESTAMPTZ NOT NULL
        );

        CREATE INDEX {Id}_index ON {PostTable}({Id});
        ");
    }

    public override void Down()
    {   
        Execute.Sql($@"
        DROP TABLE IF EXISTS {PostTable};
        ");
    }
}