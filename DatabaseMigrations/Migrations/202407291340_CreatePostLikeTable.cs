using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_13_40, "Create PostLike table")]
public class CreatePostLikeTable : Migration
{
    public const string PostLikeTable = @"""post_like""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string PostId = "post_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostLikeTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {PostId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {PostLikeTable};
        ");
    }
}