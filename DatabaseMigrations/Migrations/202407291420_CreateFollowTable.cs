using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_20, "Create Follow table")]
public class CreateFollowTable : Migration
{
    public const string FollowTable = @"""follow""";
    public const string FollowTableName = "follow";
    public const string Id = "id";
    public const string FollowerId = "follower_id";
    public const string FollowingId = "following_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {FollowTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {FollowerId} bigint NOT NULL,
            {FollowingId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );
        CREATE UNIQUE INDEX idx_{FollowTableName}_{Id} ON image USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {FollowTable};
        ");
    }
}