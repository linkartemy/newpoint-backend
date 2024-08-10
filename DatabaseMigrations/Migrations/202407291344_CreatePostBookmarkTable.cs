using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_13_44, "Create PostBookmark table")]
public class CreatePostBookmarkTable : Migration
{
    public const string PostBookmarkTable = @"""post_bookmark""";
    public const string PostBookmarkTableName = "post_bookmark";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string PostId = "post_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostBookmarkTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {PostId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );

        CREATE INDEX idx_{PostBookmarkTableName}_{UserId} ON {PostBookmarkTable}({UserId});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {PostBookmarkTable};
        ");
    }
}