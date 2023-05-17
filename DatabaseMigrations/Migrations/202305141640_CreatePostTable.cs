using FluentMigrator;

[Migration(2023_05_14_16_40, "Create Post table")]
public class CreatePostTable : Migration {
    public const string PostTable = @"""post""";
    public const string Id = "id";
    public const string AuthorId = "author_id";
    public const string Content = "content";
    public const string Images = "images";
    public const string CreationTimeStamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostTable} (
            {Id} BIGSERIAL PRIMARY KEY,
            {AuthorId} BIGINT NOT NULL,
            {Content} TEXT NOT NULL,
            {Images} JSONB,
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