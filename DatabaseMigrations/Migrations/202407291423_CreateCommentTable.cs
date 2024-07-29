using FluentMigrator;

[Migration(2024_07_29_14_23, "Create comment table")]
public class CreateCommentTable : Migration
{
    public const string CommentTable = @"""comment""";
    public const string CommentTableName = "comment";
    public const string Id = "id";
    public const string PostId = "post_id";
    public const string UserId = "user_id";
    public const string Content = "content";
    public const string Likes = "likes";
    public const string Views = "views";
    public const string CreationTimestamp = "creation_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {CommentTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {PostId} bigint NOT NULL,
            {UserId} bigint NOT NULL,
            {Content} varchar(255) NOT NULL,
            {Likes} integer NOT NULL DEFAULT 0,
            {Views} integer NOT NULL DEFAULT 0,
            {CreationTimestamp} timestamp with time zone NOT NULL DEFAULT now()
        );

        CREATE INDEX idx_{CommentTableName}_{Id} ON {CommentTable} USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {CommentTable};
        ");
    }
}