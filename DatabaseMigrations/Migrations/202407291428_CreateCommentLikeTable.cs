using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_28, "Create comment like table")]
public class CreateCommentLikeTable : Migration
{
    public const string CommentLikeTable = @"""comment_like""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string CommentId = "comment_id";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {CommentLikeTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {UserId} bigint NOT NULL,
            {CommentId} bigint NOT NULL,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {CommentLikeTable};
        ");
    }
}