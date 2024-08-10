using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_13_41, "Create PostShare table")]
public class CreatePostShareTable : Migration
{
    public const string PostShareTable = @"""post_share""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string PostId = "post_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostShareTable}(
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
        DROP TABLE IF EXISTS {PostShareTable};
        ");
    }
}